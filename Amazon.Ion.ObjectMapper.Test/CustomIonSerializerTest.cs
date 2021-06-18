using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IonDotnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class CustomIonSerializerTest
    {
        [TestMethod]
        public void CanUseACustomSerializerAnnotation()
        {

            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "translator", new Translator()}
                }
            });
            var stream = customSerializer.Serialize(TestObjects.honda);
            var serialized = StreamToIonValue(stream);

            Assert.AreEqual(3, serialized.GetField("engine").IntValue);
        }

        [TestMethod]
        public void CanUseACustomDeserializerAnnotation()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "translator", new Translator()}
                }
            });

            var stream = customSerializer.Serialize(TestObjects.honda);
            var deserialized = customSerializer.Deserialize<Car>(stream);

            Assert.AreEqual(TestObjects.honda.ToString(), deserialized.ToString());
        }
    }
}
