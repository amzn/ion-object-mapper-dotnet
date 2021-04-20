using System;
using System.Linq;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonObjectSerializer : IonSerializer<object>
    {
        private readonly IonSerializer ionSerializer;
        private readonly IonSerializationOptions options;
        private readonly Type targetType;

        public IonObjectSerializer(IonSerializer ionSerializer, IonSerializationOptions options, Type targetType)
        {
            this.ionSerializer = ionSerializer;
            this.options = options;
            this.targetType = targetType;
        }

        public object Deserialize(IIonReader reader)
        {
            var targetObject = Activator.CreateInstance(targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType  = reader.MoveNext()) != IonType.None)
            {
                var name = options.NamingConvention.ToProperty(reader.CurrentFieldName);
                var property = targetType.GetProperty(name);
                if (property != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
                    property.SetValue(targetObject, deserialized);
                }
            }
            reader.StepOut();
            return targetObject;
        }

        public void Serialize(IIonWriter writer, object item)
        {
            writer.StepIn(IonType.Struct);
            foreach (var property in item.GetType().GetProperties())
            {
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
                {
                    continue;
                }
                writer.SetFieldName(options.NamingConvention.FromProperty(property.Name));
                ionSerializer.Serialize(writer, property.GetValue(item));
            }
            writer.StepOut();
        }
    }
}