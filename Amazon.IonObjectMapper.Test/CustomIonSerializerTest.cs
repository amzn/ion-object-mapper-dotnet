using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IonDotnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.IonObjectMapper.Test.Utils;

namespace Amazon.IonObjectMapper.Test
{
    [TestClass]
    public class CustomIonSerializerTest
    {
        [TestMethod]
        public void CanUseACustomSerializerFactory()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "customSerializerKey", new CustomSerializerValue()}
                }
            });
            var stream = customSerializer.Serialize(TestObjects.honda);
            var serialized = StreamToIonValue(stream);
            var engine = serialized.GetField("engine");
            
            Assert.AreEqual(3, engine.GetField("Cylinders").IntValue);
        }

        [TestMethod]
        public void CanUseACustomDeserializerFactory()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "customSerializerKey", new CustomSerializerValue()}
                }
            });

            var stream = customSerializer.Serialize(TestObjects.honda);
            var deserialized = customSerializer.Deserialize<Car>(stream);

            Assert.AreEqual(TestObjects.honda.ToString(), deserialized.ToString());
        }
    }
}