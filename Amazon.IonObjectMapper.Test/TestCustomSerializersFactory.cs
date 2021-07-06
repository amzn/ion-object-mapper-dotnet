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
    using System;
    using System.Collections.Generic;
    using Amazon.IonDotnet;

    public class CourseSerializerFactory : IonSerializerFactory<Course>
    {
        public override IonSerializer<Course> Create(IonSerializationOptions options, Dictionary<string, object> context)
        {
            return new CourseSerializer((UpdateCourseSections)context.GetValueOrDefault("customCourseSerializer", null));
        }
    }

    public class CourseSerializer : IonSerializer<Course>
    {
        private readonly UpdateCourseSections updateCourseSections;
        public CourseSerializer(UpdateCourseSections updateCourseSections)
        {
            this.updateCourseSections = updateCourseSections;
        }

        public override Course Deserialize(IIonReader reader)
        {
            return new Course {
                Sections = updateCourseSections.AddSections(reader.IntValue()),
                MeetingTime = DateTime.Parse("2009-10-10T13:15:21Z")
            };
        }

        public override void Serialize(IIonWriter writer, Course item)
        {
            writer.StepIn(IonType.Struct);
            writer.SetFieldName("Sections");
            writer.WriteInt(updateCourseSections.RemoveSections(item.Sections));
            writer.SetFieldName("MeetingTime");
            writer.WriteString(updateCourseSections.ShowNewTime());
            writer.StepOut();
        }
    }

    public class UpdateCourseSections
    {
        public int RemoveSections(int sections)
        {
            return sections - 1;
        }

        public int AddSections(int sections)
        {
            return sections + 1;
        }

        public string ShowNewTime ()
        {
            return "2021-10-10T13:15:21Z";
        }
    }
}