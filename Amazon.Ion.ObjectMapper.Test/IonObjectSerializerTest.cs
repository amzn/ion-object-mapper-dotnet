using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree;
using Amazon.IonDotnet.Tree.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class IonObjectSerializerTest
    {
        IonSerializer defaultSerializer = new IonSerializer();
        private IValueFactory valueFactory = new ValueFactory();

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
        public void SerializesObjectsWithIgnoreReadOnlyProperties()
        {
            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreReadOnlyProperties = true});
            IIonStruct serialized = StreamToIonValue(serializer.Serialize(TestObjects.JohnGreenwood));
            
            Assert.IsTrue(serialized.ContainsField("firstName"));
            Assert.IsTrue(serialized.ContainsField("lastName"));
            Assert.IsFalse(serialized.ContainsField("<Major>k__BackingField"));
        }
        
        [TestMethod]
        public void DeserializesObjectsWithIgnoreReadOnlyProperties()
        {
            var stream = new IonSerializer().Serialize(TestObjects.JohnGreenwood);

            var serializer = new IonSerializer(new IonSerializationOptions {IgnoreReadOnlyProperties = true});
            var deserialized = serializer.Deserialize<Student>(stream);
            
            Assert.AreEqual(TestObjects.JohnGreenwood.FirstName, deserialized.FirstName);
            Assert.AreEqual(TestObjects.JohnGreenwood.LastName, deserialized.LastName);
            Assert.AreNotEqual(TestObjects.JohnGreenwood.Major, deserialized.Major);
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
        public void SerializesAndDeserializesGetterAndSetterMethods()
        {
            Check(TestObjects.SchoolDesk);
        }

        [TestMethod]
        public void DoesNotDoubleSerializeIonFieldsAlreadySerializedByMethods()
        {
            IIonStruct serialized = ToIonValue(new IonSerializer(), TestObjects.Ruler);

            Assert.IsTrue(serialized.ContainsField("length"));
            Assert.IsTrue(serialized.ContainsField("unit"));

            // We should have exactly two fields. ie. we did not double serialize the Ruler's length or unit members.
            Assert.AreEqual(2, serialized.Count);
        }

        [TestMethod]
        public void ExceptionOnMultiParameterIonPropertySetterMethods()
        {
            var serializer = new IonSerializer();

            var stream = serializer.Serialize(TestObjects.Chalkboard);
            Assert.ThrowsException<InvalidOperationException>(() => serializer.Deserialize<Chalkboard>(stream));
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

        [TestMethod]
        public void SerializesWithCustomBoolSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegationBoolIonSerializer(), true);
            Assert.AreEqual(false, serialized.BoolValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomBoolSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegationBoolIonSerializer(), true);
            Assert.AreEqual(false, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomStringSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new UpperCaseStringIonSerializer(), "test string");
            Assert.AreEqual("TEST STRING", serialized.StringValue);
        }

        [TestMethod]
        public void DeserializesWithCustomStringSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new UpperCaseStringIonSerializer(), "test string");
            Assert.AreEqual("TEST STRING", deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomByteArraySerializer()
        {
            var testArr = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26 };
            var expectedArr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var serialized = SerializeToIonWithCustomSerializer(new ZeroByteArrayIonSerializer(), testArr);
            Assert.IsTrue(expectedArr.SequenceEqual(serialized.Bytes().ToArray()));
        }

        [TestMethod]
        public void DeserializesWithCustomByteArraySerializer()
        {
            var testArr = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26 };
            var expectedArr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var deserialized = DeserializeWithCustomSerializer(new ZeroByteArrayIonSerializer(), testArr);
            Assert.IsTrue(expectedArr.SequenceEqual(deserialized));
        }

        [TestMethod]
        public void SerializesWithCustomIntSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegativeIntIonSerializer(), 15);
            Assert.AreEqual(-15, serialized.IntValue);
        }

        [TestMethod]
        public void DeserializesWithCustomIntSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegativeIntIonSerializer(), 15);
            Assert.AreEqual(-15, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomLongSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegativeLongIonSerializer(), 9223372036854775807);
            Assert.AreEqual(-9223372036854775807, serialized.LongValue);
        }

        [TestMethod]
        public void DeserializesWithCustomLongSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegativeLongIonSerializer(), 9223372036854775807);
            Assert.AreEqual(-9223372036854775807, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomFloatSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegativeFloatIonSerializer(), 3.14f);
            Assert.AreEqual(-3.14f, serialized.DoubleValue);
        }

        [TestMethod]
        public void DeserializesWithCustomFloatSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegativeFloatIonSerializer(), 3.14f);
            Assert.AreEqual(-3.14f, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomDoubleSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegativeDoubleIonSerializer(), 3.14);
            Assert.AreEqual(-3.14, serialized.DoubleValue);
        }

        [TestMethod]
        public void DeserializesWithCustomDoubleSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegativeDoubleIonSerializer(), 3.14);
            Assert.AreEqual(-3.14, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomDecimalSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(new NegativeDecimalIonSerializer(), 3.14m);
            Assert.AreEqual(-3.14m, serialized.DecimalValue);
        }

        [TestMethod]
        public void DeserializesWithCustomDecimalSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(new NegativeDecimalIonSerializer(), 3.14m);
            Assert.AreEqual(-3.14m, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomBigDecimalSerializer()
        {
            var testBigDecimal = BigDecimal.Parse("3.14159265359");
            
            var serialized = SerializeToIonWithCustomSerializer(new NegativeBigDecimalIonSerializer(), testBigDecimal);
            Assert.AreEqual(-testBigDecimal, serialized.BigDecimalValue);
        }

        [TestMethod]
        public void DeserializesWithCustomBigDecimalSerializer()
        {
            var testBigDecimal = BigDecimal.Parse("3.14159265359");
            
            var deserialized = DeserializeWithCustomSerializer(new NegativeBigDecimalIonSerializer(), testBigDecimal);
            Assert.AreEqual(-testBigDecimal, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomSymbolSerializer()
        {
            var serialized = SerializeToIonWithCustomSerializer(
                new UpperCaseSymbolIonSerializer(), 
                new SymbolToken("test symbol", 10));
            Assert.AreEqual("TEST SYMBOL", serialized.SymbolValue.Text);
        }

        [TestMethod]
        public void DeserializesWithCustomSymbolSerializer()
        {
            var deserialized = DeserializeWithCustomSerializer(
                new UpperCaseSymbolIonSerializer(), 
                new SymbolToken("test symbol", 10));
            Assert.AreEqual("TEST SYMBOL", deserialized.Text);
        }

        [TestMethod]
        public void SerializesWithCustomDateTimeSerializer()
        {
            var testDate = new DateTime(2005, 05, 23, 14, 30, 30);
            var expectedDate = testDate.AddDays(1);

            var serialized = SerializeToIonWithCustomSerializer(new NextDayDateTimeIonSerializer(), testDate);
            Assert.AreEqual(expectedDate, serialized.TimestampValue.DateTimeValue);
        }

        [TestMethod]
        public void DeserializesWithCustomDateTimeSerializer()
        {
            var testDate = new DateTime(2005, 05, 23, 14, 30, 30);
            var expectedDate = testDate.AddDays(1);

            var deserialized = DeserializeWithCustomSerializer(new NextDayDateTimeIonSerializer(), testDate);
            Assert.AreEqual(expectedDate, deserialized);
        }

        [TestMethod]
        public void SerializesWithCustomGuidSerializer()
        {
            var testGuid = new Guid(new byte[] 
            { 
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F
            });
            var expectedGuid = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var serialized = SerializeToIonWithCustomSerializer(new ZeroGuidIonSerializer(), testGuid);
            Assert.IsTrue(expectedGuid.SequenceEqual(serialized.Bytes().ToArray()));
        }

        [TestMethod]
        public void DeserializesWithCustomGuidSerializer()
        {
            var testGuid = new Guid(new byte[] 
            { 
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F
            });
            var expectedGuid = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var deserialized = DeserializeWithCustomSerializer(new ZeroGuidIonSerializer(), testGuid);
            Assert.IsTrue(expectedGuid.SequenceEqual(deserialized.ToByteArray()));
        }
        
        [TestMethod]
        public void SerializesListsWithCustomSerializers()
        {
            var customSerializers = new Dictionary<Type, IIonSerializer>()
            {
                {typeof(string), new UpperCaseStringIonSerializer()},
                {typeof(int), new NegativeIntIonSerializer()},
                {typeof(float), new NegativeFloatIonSerializer()},
            };

            List<object> testList = new List<object>(new object[] { "test", 5, 3.14f });
            
            var serialized = (IIonList)SerializeToIonWithCustomSerializers(customSerializers, testList);

            Assert.AreEqual(3, serialized.Count);
            Assert.AreEqual("TEST", serialized.GetElementAt(0).StringValue);
            Assert.AreEqual(-5, serialized.GetElementAt(1).IntValue);
            Assert.AreEqual(-3.14f, serialized.GetElementAt(2).DoubleValue);
        }
        
        [TestMethod]
        public void DeserializesListsWithCustomSerializers()
        {
            var customSerializers = new Dictionary<Type, IIonSerializer>()
            {
                {typeof(string), new UpperCaseStringIonSerializer()},
                {typeof(int), new NegativeIntIonSerializer()},
                {typeof(float), new NegativeFloatIonSerializer()},
            };

            List<object> testList = new List<object>(new object[] { "test", 5, 3.14f });
            
            var deserialized = DeserializeWithCustomSerializers(customSerializers, testList);

            Assert.AreEqual(3, deserialized.Count);
            Assert.AreEqual("TEST", deserialized[0]);
            Assert.AreEqual(-5, deserialized[1]);
            Assert.AreEqual(-3.14f, deserialized[2]);
        }

        [TestMethod]
        public void SerializesObjectsWithCustomSerializers()
        {
            var customSerializers = new Dictionary<Type, IIonSerializer>()
            {
                {typeof(string), new UpperCaseStringIonSerializer()},
                {typeof(int), new NegativeIntIonSerializer()},
                {typeof(double), new NegativeDoubleIonSerializer()},
                {typeof(DateTime), new NextDayDateTimeIonSerializer()},
            };

            var serialized = SerializeToIonWithCustomSerializers(customSerializers, TestObjects.honda);

            Assert.IsTrue(serialized.ContainsField("make"));
            Assert.AreEqual(TestObjects.honda.Make.ToUpper(), serialized.GetField("make").StringValue);

            Assert.IsTrue(serialized.ContainsField("yearOfManufacture"));
            Assert.AreEqual(-TestObjects.honda.YearOfManufacture, serialized.GetField("yearOfManufacture").IntValue);

            Assert.IsTrue(serialized.ContainsField("weightInKg"));
            Assert.AreEqual(-TestObjects.honda.Weight, serialized.GetField("weightInKg").DoubleValue);

            Assert.IsTrue(serialized.ContainsField("engine"));
            var engine = serialized.GetField("engine");
            Assert.IsTrue(engine.ContainsField("manufactureDate"));
            Assert.AreEqual(
                TestObjects.honda.Engine.ManufactureDate.AddDays(1), 
                engine.GetField("manufactureDate").TimestampValue.DateTimeValue);
        }

        [TestMethod]
        public void DeserializesObjectsWithCustomSerializers()
        {
            var customSerializers = new Dictionary<Type, IIonSerializer>()
            {
                {typeof(string), new UpperCaseStringIonSerializer()},
                {typeof(int), new NegativeIntIonSerializer()},
                {typeof(double), new NegativeDoubleIonSerializer()},
                {typeof(DateTime), new NextDayDateTimeIonSerializer()},
            };

            var deserialized = DeserializeWithCustomSerializers(customSerializers, TestObjects.honda);

            Assert.AreEqual(TestObjects.honda.Make.ToUpper(), deserialized.Make);
            Assert.AreEqual(-TestObjects.honda.YearOfManufacture, deserialized.YearOfManufacture);
            Assert.AreEqual(-TestObjects.honda.Weight, deserialized.Weight);
            Assert.AreEqual(TestObjects.honda.Engine.ManufactureDate.AddDays(1), deserialized.Engine.ManufactureDate);
        }

        [TestMethod]
        public void DeserializeAnnotatedIonToParentClassNameMatchingAnnotation()
        {
            IIonReader reader = IonReaderBuilder.Build(TestObjects.truckIonText);

            Vehicle truck = defaultSerializer.Deserialize<Vehicle>(reader);

            AssertIsTruck(truck);
        }

        [TestMethod]
        public void DeserializeAnnotatedIonToClassNameMatchingAnnotation()
        {
            IIonReader reader = IonReaderBuilder.Build(TestObjects.truckIonText);

            Truck truck = defaultSerializer.Deserialize<Truck>(reader);

            AssertIsTruck(truck);
        }

        [TestMethod]
        public void DeserializeAnnotatedIonToClassNameNotMatchingAnnotation()
        {
            string annotatedIonText = "NonMatching:: { }";

            IIonReader reader = IonReaderBuilder.Build(annotatedIonText);

            Truck truck = defaultSerializer.Deserialize<Truck>(reader);

            AssertIsTruck(truck);
        }

        [TestMethod]
        public void DeserializeAnnotatedIonToClassNameWithAnnotatedTypeAssemblies()
        {
            IonSerializationOptions options = new IonSerializationOptions
            {
                AnnotatedTypeAssemblies = new string[]
                {
                        typeof(Truck).Assembly.GetName().Name
                }
            };

            IIonReader reader = IonReaderBuilder.Build(TestObjects.truckIonText);

            IonSerializer ionSerializer = new IonSerializer(options);
            Vehicle truck = ionSerializer.Deserialize<Vehicle>(reader);

            AssertIsTruck(truck);

            // Ensure default loaded assemblies are not searched in if annotated type assemblies are given
            options = new IonSerializationOptions
            {
                AnnotatedTypeAssemblies = new string[]
                {
                    "mockAssemblyName"
                }
            };

            reader = IonReaderBuilder.Build(TestObjects.truckIonText);
            ionSerializer = new IonSerializer(options);
            truck = ionSerializer.Deserialize<Vehicle>(reader);

            Assert.AreNotEqual(TestObjects.nativeTruck.ToString(), truck.ToString());
        }

        [TestMethod]
        public void DeserializeMultipleAnnotatedIonToClassNameWithFirstAnnotated()
        {
            // Multiple annotations are ignored, it will only pick the first one.
            IIonValue ionTruck = valueFactory.NewEmptyStruct();
            ionTruck.AddTypeAnnotation("Truck");
            ionTruck.AddTypeAnnotation("SecondAnnotation");

            IIonReader reader = IonReaderBuilder.Build(ionTruck);

            Vehicle truck = defaultSerializer.Deserialize<Vehicle>(reader);

            AssertIsTruck(truck);
        }

        [TestMethod]
        public void DeserializeMultipleAnnotatedIonToClassNameWithSecondAnnotated()
        {
            // Multiple annotations are ignored, it will only pick the first one.
            IIonValue ionTruck = valueFactory.NewEmptyStruct();
            ionTruck.AddTypeAnnotation("FirstAnnotation");
            ionTruck.AddTypeAnnotation("Truck");

            IIonReader reader = IonReaderBuilder.Build(ionTruck);

            Vehicle truck = defaultSerializer.Deserialize<Vehicle>(reader);

            Assert.AreNotEqual(TestObjects.nativeTruck.ToString(), truck.ToString());
        }

        [TestMethod]
        public void SerializesAndDeserializesWithCustomObjectSerializer()
        {
            var serializer = new IonSerializer();

            var stream = serializer.Serialize(TestObjects.Rover);
            IIonStruct serialized = StreamToIonValue(stream);
            
            // These field names come from the custom serializer.
            AssertContainsFields(serialized, new string[] {"Given Name", "Male or Female", "Classification"});

            var deserialized = serializer.Deserialize<Dog>(stream);
            Assert.IsTrue(TestObjects.Rover.Equals(deserialized));
        }

        [TestMethod]
        public void ExceptionOnInvalidCustomObjectSerializer()
        {
            var serializer = new IonSerializer();
            var cat = new Cat("Kitty", "Female", "Bengal");

            Assert.ThrowsException<NotSupportedException>(() => serializer.Serialize(cat));
        }

        private void AssertIsTruck(object actual)
        {
            Assert.AreEqual(actual.ToString(), TestObjects.nativeTruck.ToString());
        }
    }
}
