using System;
using System.Collections.Generic;
using Amazon.IonDotnet;

namespace Amazon.IonObjectMapper.Test {
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
            writer.WriteString(updateCourseSections.ShowCurrentTime());
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

        public string ShowCurrentTime ()
        {
            string newTime = "2021-10-10T13:15:21Z";
            return newTime;
        }
    }
}