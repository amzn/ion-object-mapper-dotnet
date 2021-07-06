/*
 * Copyright (c) Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

namespace Amazon.IonObjectMapper.Test
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Amazon.IonObjectMapper.Test.Utils;

    [TestClass]
    public class CustomIonSerializerTest
    {
        [TestMethod]
        public void SerializeObjectsWithCustomContextFactoryAttribute()
        {
            var customContext = new Dictionary<string, object>()
            {
                { "customCourseSerializer", new UpdateCourseSections() }
            };
            var customSerializer = new IonSerializer(new IonSerializationOptions { CustomContext = customContext });
            var stream = customSerializer.Serialize(TestObjects.bob);
            var serialized = StreamToIonValue(stream);
            Assert.IsTrue(serialized.ContainsField("name"));
            Assert.IsTrue(serialized.ContainsField("course"));
            Assert.IsTrue(serialized.ContainsField("id"));
            var course = serialized.GetField("course");
            Assert.AreEqual(9, course.GetField("Sections").IntValue);
            Assert.AreEqual("2021-10-10T13:15:21Z", course.GetField("MeetingTime").StringValue);
        }

        [TestMethod]
        public void DeserializeObjectsWithCustomContextFactoryAttribute()
        {
            var customContext = new Dictionary<string, object>()
            {
                { "customCourseSerializer", new UpdateCourseSections() }
            };
            Check(TestObjects.bob, new IonSerializationOptions { CustomContext = customContext });
        }
    }
}