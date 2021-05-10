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

            Assert.IsFalse(serialized.ContainsField("brand"));
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

            Assert.IsFalse(serialized.ContainsField("brand"));
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
        public void SerializesObjectsWithCaseInsensitiveProperties()
        {
            var serializer = new IonSerializer(new IonSerializationOptions {PropertyNameCaseInsensitive = true});
            IIonStruct serialized = StreamToIonValue(serializer.Serialize(TestObjects.harley));

            Assert.IsTrue(serialized.ContainsField("brand"));
            Assert.IsTrue(serialized.ContainsField("color"));
            Assert.IsTrue(serialized.ContainsField("canOffroad"));
        }
        
        [TestMethod]
        public void DeserializesObjectsWithCaseInsensitiveProperties()
        {
            var stream = new IonSerializer().Serialize(TestObjects.harley);

            var serializer = new IonSerializer(new IonSerializationOptions {PropertyNameCaseInsensitive = true});
            var deserialized = serializer.Deserialize<MOTORCYCLE>(stream);
            
            Assert.AreEqual("Harley", deserialized.BRAND);
            Assert.IsNull(deserialized.COLOR);
            Assert.IsFalse(deserialized.CANOFFROAD);
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
