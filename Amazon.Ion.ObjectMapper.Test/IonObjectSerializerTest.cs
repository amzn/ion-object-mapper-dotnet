using System;
using System.Collections.Generic;
using Amazon.IonDotnet.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class IonObjectSerializerTest
    {
        [TestMethod]
        public void SerializesAndDeserializesObjects()
        {
            Check(TestObjects.honda);
        }
        
        [TestMethod]
        public void SerializesAndDeserializesObjectsWithIncludeFields()
        {
            Check(TestObjects.fieldAcademy, new IonSerializationOptions { IncludeFields = true });
        }

        [TestMethod]
        public void SerializesObjectsWithIgnoreNulls()
        {
            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreNulls = true});
            var motorcycle = new Motorcycle {canOffroad = true};

            IIonStruct serialized = StreamToIonValue(serializer.Serialize(motorcycle));

            Assert.IsFalse(serialized.ContainsField("Brand"));
            Assert.IsFalse(serialized.ContainsField("color"));
            Assert.IsTrue(serialized.ContainsField("canOffroad"));
        }
        
        [TestMethod]
        public void SerializesObjectsWithIgnoreReadOnlyFields()
        {
            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreReadOnlyFields = true, IncludeFields = true});

            IIonStruct serialized = StreamToIonValue(serializer.Serialize(TestObjects.drKyler));
            
            Assert.IsFalse(serialized.ContainsField("firstName"));
            Assert.IsFalse(serialized.ContainsField("lastName"));
            Assert.IsTrue(serialized.ContainsField("department"));
            Assert.IsFalse(serialized.ContainsField("birthDate"));
        }
        
        [TestMethod]
        public void DeserializesObjectsWithIgnoreReadOnlyFields()
        {
            var stream = new IonSerializer(new IonSerializationOptions {IncludeFields = true}).Serialize(TestObjects.drKyler);

            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreReadOnlyFields = true, IncludeFields = true});
            var deserialized = serializer.Deserialize<Teacher>(stream);
            
            Assert.IsNull(deserialized.firstName);
            Assert.IsNull(deserialized.lastName);
            Assert.IsNotNull(deserialized.department);
            Assert.IsNull(deserialized.birthDate);
        }

        [TestMethod]
        public void SerializesObjectsWithIgnoreDefaults()
        {
            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreDefaults = true});
            IIonStruct serialized = StreamToIonValue(serializer.Serialize(new Motorcycle {canOffroad = true}));

            Assert.IsFalse(serialized.ContainsField("Brand"));
            Assert.IsFalse(serialized.ContainsField("color"));
            Assert.IsTrue(serialized.ContainsField("canOffroad"));
        }
        
        [TestMethod]
        public void DeserializesObjectsWithIgnoreDefaults()
        {
            var stream = new IonSerializer().Serialize(new Motorcycle {canOffroad = true});

            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreDefaults = true});
            var deserialized = serializer.Deserialize<Motorcycle>(stream);
            
            Assert.IsNull(deserialized.Brand);
            Assert.IsNull(deserialized.color);
            Assert.IsNotNull(deserialized.canOffroad);
        }
        
        [TestMethod]
        public void SerializesAndDeserializesWithCustomSerializers()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(string), new UpperCaseStringIonSerializer()},
                    {typeof(int), new NegativeIntIonSerializer()},
                }
            });

            var testStr = "test string";

            var stream = customSerializer.Serialize(testStr);
            var serializedStr = StreamToIonValue(stream);
            Assert.AreEqual(testStr.ToUpper(), serializedStr.StringValue);

            stream = defaultSerializer.Serialize(testStr);
            var deserializedStr = customSerializer.Deserialize<string>(stream);
            Assert.AreEqual(testStr.ToUpper(), deserializedStr);

            var testInt = 15;
            
            stream = customSerializer.Serialize(testInt);
            var serializedInt = StreamToIonValue(stream);
            Assert.AreEqual(-testInt, serializedInt.IntValue);
            
            stream = defaultSerializer.Serialize(testInt);
            var deserializedInt = customSerializer.Deserialize<int>(stream);
            Assert.AreEqual(-testInt, deserializedInt);
            
            stream = customSerializer.Serialize(TestObjects.honda);
            var serializedCar = StreamToIonValue(stream);
            Assert.IsTrue(serializedCar.ContainsField("make"));
            Assert.AreEqual(TestObjects.honda.Make.ToUpper(), serializedCar.GetField("make").StringValue);
            Assert.IsTrue(serializedCar.ContainsField("yearOfManufacture"));
            Assert.AreEqual(-TestObjects.honda.YearOfManufacture, serializedCar.GetField("yearOfManufacture").IntValue);
            
            stream = defaultSerializer.Serialize(TestObjects.honda);
            var deserializedCar = customSerializer.Deserialize<Car>(stream);
            Assert.AreEqual(TestObjects.honda.Make.ToUpper(), deserializedCar.Make);
            Assert.AreEqual(-TestObjects.honda.YearOfManufacture, deserializedCar.YearOfManufacture);
        }

        [TestMethod]
        public void SerializesAndDeserializesObjectsWithCaseInsensitiveProperties()
        {
            var serializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = true });
            
            var stream = serializer.Serialize(TestObjects.Titanic);
            var deserialized = serializer.Deserialize<ShipWithVariedCasing>(stream);
            
            Assert.AreEqual(TestObjects.Titanic.Name, deserialized.name);
            Assert.AreEqual(TestObjects.Titanic.Weight, deserialized.WEIGHT);
            Assert.AreEqual(TestObjects.Titanic.Capacity, deserialized.CaPaCiTy);
        }

        [TestMethod]
        public void SerializesAndDeserializesFields()
        {
            Check(TestObjects.registration);
        }

        [TestMethod]
        public void SerializesAndDeserializesCustomPropertyNames()
        {
            Check(TestObjects.fmRadio);
        }

        [TestMethod]
        public void SerializesAndDeserializesSubtypesBasedOnTypeAnnotations()
        {
            Check(
                new List<Vehicle>()
                {
                    new Plane(), new Boat(), new Helicopter()
                }, 
                new IonSerializationOptions
                {
                    AnnotatedTypeAssemblies = new string[]
                    {
                        typeof(Vehicle).Assembly.GetName().Name
                    }
                }
            );
        }

        [TestMethod]
        public void DeserializesObjectsThatExceedMaxDepth()
        {
            var stream = new IonSerializer().Serialize(TestObjects.UnitedStates);
            
            var serializer = new IonSerializer(new IonSerializationOptions {MaxDepth = 4});
            var deserialized = serializer.Deserialize<Country>(stream);

            Assert.IsNotNull(deserialized.States[0].Capital.Mayor);
            Assert.IsNull(deserialized.States[0].Capital.Mayor.FirstName);
        }

        [TestMethod]
        public void RespectAnnotationInheritance()
        {
            var serializer = new IonSerializer(new IonSerializationOptions { TypeAnnotationPrefix = new FixedTypeAnnotationPrefix("testing") });
            AssertHasAnnotation("testing.Plane", serializer.Serialize(new Plane()));
            AssertHasNoAnnotations(serializer.Serialize(new Yacht()));
            AssertHasAnnotation("testing.Catamaran", serializer.Serialize(new Catamaran()));
            AssertHasAnnotation("testing.Jet", serializer.Serialize(new Jet()));
            AssertHasAnnotation("testing.Boeing", serializer.Serialize(new Boeing()));
        }

        [TestMethod]
        public void RespectAnnotationPrefixes()
        {
            AssertHasAnnotation(
                "my.prefix.Truck", 
                new IonSerializer(new IonSerializationOptions { IncludeTypeInformation = true, TypeAnnotationPrefix = new FixedTypeAnnotationPrefix("my.prefix") }).Serialize(new Truck()));
            AssertHasAnnotation(
                "my.universal.namespace.BussyMcBusface", 
                new IonSerializer().Serialize(new Bus()));
        }
    }
}
