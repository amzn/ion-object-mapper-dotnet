using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.IonDotnet;
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
        
        [TestMethod]
        public void SerializesWithCustomBoolSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(bool), new NegationBoolIonSerializer()},
                }
            });
            
            var testBool = true;
            
            var stream = customSerializer.Serialize(testBool);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(!testBool, serialized.BoolValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomBoolSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(bool), new NegationBoolIonSerializer()},
                }
            });
            
            var testBool = true;

            var stream = defaultSerializer.Serialize(testBool);
            var deserialized = customSerializer.Deserialize<bool>(stream);
            Assert.AreEqual(!testBool, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomStringSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(string), new UpperCaseStringIonSerializer()},
                }
            });

            var testStr = "test string";

            var stream = customSerializer.Serialize(testStr);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(testStr.ToUpper(), serialized.StringValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomStringSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(string), new UpperCaseStringIonSerializer()},
                }
            });

            var testStr = "test string";

            var stream = defaultSerializer.Serialize(testStr);
            var deserialized = customSerializer.Deserialize<string>(stream);
            Assert.AreEqual(testStr.ToUpper(), deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomByteArraySerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(byte[]), new ZeroByteArrayIonSerializer()},
                }
            });

            var testArr = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26 };
            var expectedArr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var stream = customSerializer.Serialize(testArr);
            var serialized = StreamToIonValue(stream);
            Assert.IsTrue(expectedArr.SequenceEqual(serialized.Bytes().ToArray()));
        }
        
        [TestMethod]
        public void DeserializesWithCustomByteArraySerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(byte[]), new ZeroByteArrayIonSerializer()},
                }
            });

            var testArr = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26 };
            var expectedArr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var stream = defaultSerializer.Serialize(testArr);
            var deserialized = customSerializer.Deserialize<byte[]>(stream);
            Assert.IsTrue(expectedArr.SequenceEqual(deserialized));
        }
        
        [TestMethod]
        public void SerializesWithCustomIntSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(int), new NegativeIntIonSerializer()},
                }
            });
            
            var testInt = 15;
            
            var stream = customSerializer.Serialize(testInt);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testInt, serialized.IntValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomIntSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(int), new NegativeIntIonSerializer()},
                }
            });
            
            var testInt = 15;

            var stream = defaultSerializer.Serialize(testInt);
            var deserialized = customSerializer.Deserialize<int>(stream);
            Assert.AreEqual(-testInt, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomLongSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(long), new NegativeLongIonSerializer()},
                }
            });
            
            var testLong = 9223372036854775807;
            
            var stream = customSerializer.Serialize(testLong);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testLong, serialized.LongValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomLongSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(long), new NegativeLongIonSerializer()},
                }
            });
            
            var testLong = 9223372036854775807;

            var stream = defaultSerializer.Serialize(testLong);
            var deserialized = customSerializer.Deserialize<long>(stream);
            Assert.AreEqual(-testLong, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomFloatSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(float), new NegativeFloatIonSerializer()},
                }
            });
            
            var testFloat = (float)3.14;
            
            var stream = customSerializer.Serialize(testFloat);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testFloat, serialized.DoubleValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomFloatSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(float), new NegativeFloatIonSerializer()},
                }
            });
            
            var testFloat = (float)3.14;

            var stream = defaultSerializer.Serialize(testFloat);
            var deserialized = customSerializer.Deserialize<float>(stream);
            Assert.AreEqual(-testFloat, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomDoubleSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(double), new NegativeDoubleIonSerializer()},
                }
            });
            
            var testFloat = 3.14;
            
            var stream = customSerializer.Serialize(testFloat);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testFloat, serialized.DoubleValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomDoubleSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(double), new NegativeDoubleIonSerializer()},
                }
            });
            
            var testFloat = 3.14;

            var stream = defaultSerializer.Serialize(testFloat);
            var deserialized = customSerializer.Deserialize<double>(stream);
            Assert.AreEqual(-testFloat, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomDecimalSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(decimal), new NegativeDecimalIonSerializer()},
                }
            });
            
            var testDecimal = (decimal)3.14;
            
            var stream = customSerializer.Serialize(testDecimal);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testDecimal, serialized.DecimalValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomDecimalSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(decimal), new NegativeDecimalIonSerializer()},
                }
            });
            
            var testDecimal = (decimal)3.14;

            var stream = defaultSerializer.Serialize(testDecimal);
            var deserialized = customSerializer.Deserialize<decimal>(stream);
            Assert.AreEqual(-testDecimal, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomBigDecimalSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(BigDecimal), new NegativeBigDecimalIonSerializer()},
                }
            });

            var testBigDecimal = BigDecimal.Parse("3.14159265359");
            
            var stream = customSerializer.Serialize(testBigDecimal);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(-testBigDecimal, serialized.BigDecimalValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomBigDecimalSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(BigDecimal), new NegativeBigDecimalIonSerializer()},
                }
            });
            
            var testBigDecimal = BigDecimal.Parse("3.14159265359");

            var stream = defaultSerializer.Serialize(testBigDecimal);
            var deserialized = customSerializer.Deserialize<BigDecimal>(stream);
            Assert.AreEqual(-testBigDecimal, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomSymbolSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(SymbolToken), new UpperCaseSymbolIonSerializer()},
                }
            });

            var testSymbol = new SymbolToken("test symbol", 10);

            var stream = customSerializer.Serialize(testSymbol);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(testSymbol.Text.ToUpper(), serialized.SymbolValue.Text);
        }
        
        [TestMethod]
        public void DeserializesWithCustomSymbolSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(SymbolToken), new UpperCaseSymbolIonSerializer()},
                }
            });

            var testSymbol = new SymbolToken("test symbol", 10);

            var stream = defaultSerializer.Serialize(testSymbol);
            var deserialized = customSerializer.Deserialize<SymbolToken>(stream);
            Assert.AreEqual(testSymbol.Text.ToUpper(), deserialized.Text);
        }
        
        [TestMethod]
        public void SerializesWithCustomDateSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(DateTime), new NextDayDateTimeIonSerializer()},
                }
            });

            var testDate = new DateTime(2005, 05, 23);
            var expectedDate = testDate.AddDays(1);

            var stream = customSerializer.Serialize(testDate);
            var serialized = StreamToIonValue(stream);
            Assert.AreEqual(expectedDate, serialized.TimestampValue.DateTimeValue);
        }
        
        [TestMethod]
        public void DeserializesWithCustomDateSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(DateTime), new NextDayDateTimeIonSerializer()},
                }
            });

            var testDate = new DateTime(2005, 05, 23);
            var expectedDate = testDate.AddDays(1);

            var stream = defaultSerializer.Serialize(testDate);
            var deserialized = customSerializer.Deserialize<DateTime>(stream);
            Assert.AreEqual(expectedDate, deserialized);
        }
        
        [TestMethod]
        public void SerializesWithCustomGuidSerializer()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(Guid), new ZeroGuidIonSerializer()},
                }
            });

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

            var stream = customSerializer.Serialize(testGuid);
            var serialized = StreamToIonValue(stream);
            Assert.IsTrue(expectedGuid.SequenceEqual(serialized.Bytes().ToArray()));
        }
        
        [TestMethod]
        public void DeserializesWithCustomGuidSerializer()
        {
            var defaultSerializer = new IonSerializer();
            var customSerializer = new IonSerializer(new IonSerializationOptions
            {
                IonSerializers = new Dictionary<Type, dynamic>()
                {
                    {typeof(Guid), new ZeroGuidIonSerializer()},
                }
            });

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

            var stream = defaultSerializer.Serialize(testGuid);
            var deserialized = customSerializer.Deserialize<Guid>(stream);
            Assert.IsTrue(expectedGuid.SequenceEqual(deserialized.ToByteArray()));
        }
    }
}
