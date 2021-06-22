using System;
using System.Collections.Generic;
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
                    { "customCourseSerializer", new CustomCourseSerializerFactory()}
                }
            });
            var stream = customSerializer.Serialize(TestObjects.bob);
            var serialized = StreamToIonValue(stream);
            Assert.IsTrue(serialized.ContainsField("name"));
            Assert.IsTrue(serialized.ContainsField("course"));
            Assert.IsTrue(serialized.ContainsField("id"));
            var course = serialized.GetField("course");
            Assert.AreEqual(9, course.GetField("Sections").IntValue);
        }

        [TestMethod]
        public void CanUseACustomDeserializerFactory()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "customCourseSerializer", new CustomCourseSerializerFactory()}
                }
            });
            var stream = customSerializer.Serialize(TestObjects.bob);
            var deserialized = customSerializer.Deserialize<Person>(stream);

            Assert.AreEqual(TestObjects.bob.ToString(), deserialized.ToString());
        }
    }
}