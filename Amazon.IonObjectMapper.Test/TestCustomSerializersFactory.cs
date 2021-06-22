using System;
using System.Collections.Generic;
using Amazon.IonDotnet;

namespace Amazon.IonObjectMapper.Test {
public class CourseSerializerFactory : IonSerializerFactory<Course>
    {
        public override IonSerializer<Course> Create(IonSerializationOptions options, Dictionary<string, object> context)
        {
            return new CourseSerializer((CustomSerializerValue)context.GetValueOrDefault("customSerializerKey", null));
        }
    }

    public class CourseSerializer : IonSerializer<Course>
    {
        private readonly CustomSerializerValue customSerializerValue;
        public CourseSerializer(CustomSerializerValue customSerializerValue)
        {
            this.customSerializerValue = customSerializerValue;
        }

        public override Course Deserialize(IIonReader reader)
        {
            return new Course { 
                Sections = customSerializerValue.AddSections(reader.IntValue()),
                MeetingTime = DateTime.Parse("2009-10-10T13:15:21Z")
            };
        }

        public override void Serialize(IIonWriter writer, Course item)
        {
            writer.StepIn(IonType.Struct);
            writer.SetFieldName("Sections");
            writer.WriteInt(customSerializerValue.RemoveSections(item.Sections));
            writer.SetFieldName("MeetingTime");
            writer.WriteString(customSerializerValue.ShowCurrentTime());
            writer.StepOut();

        }
    }

    public class CustomSerializerValue
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
            return DateTime.Now.ToString();
        }
    }
}