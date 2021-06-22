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
        public void SerializeObjectsWithCustomContextFactoryAttribute()
        {
            var customContext = new Dictionary<string, object>()
            { 
                { "customCourseSerializer", new UpdateCourseSections()}
            };
            Check(TestObjects.bob, new IonSerializationOptions { CustomContext = customContext });
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = customContext});
            var stream = customSerializer.Serialize(TestObjects.bob);
            var serialized = StreamToIonValue(stream);
            Assert.IsTrue(serialized.ContainsField("name"));
            Assert.IsTrue(serialized.ContainsField("course"));
            Assert.IsTrue(serialized.ContainsField("id"));
            var course = serialized.GetField("course");
            Assert.AreEqual(9, course.GetField("Sections").IntValue);
            Assert.AreEqual(DateTime.Now.ToString(), course.GetField("MeetingTime").StringValue);
        }

        [TestMethod]
        public void DeserializeObjectsWithCustomContextFactoryAttribute()
        {
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = new Dictionary<string, object>() 
                {
                    { "customCourseSerializer", new UpdateCourseSections()}
                }
            });
            var stream = customSerializer.Serialize(TestObjects.bob);
            var deserialized = customSerializer.Deserialize<Person>(stream);

            Assert.AreEqual(TestObjects.bob.ToString(), deserialized.ToString());
        }
    }
}