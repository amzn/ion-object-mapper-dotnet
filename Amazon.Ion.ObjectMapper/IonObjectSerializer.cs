using System;
using System.Linq;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonObjectSerializer : IonSerializer<object>
    {
        private readonly IonSerializer ionSerializer;
        private readonly Type targetType;

        public IonObjectSerializer(IonSerializer ionSerializer, Type targetType)
        {
            this.ionSerializer = ionSerializer;
            this.targetType = targetType;
        }

        public object Deserialize(IIonReader reader)
        {
            var targetObject = Activator.CreateInstance(targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType  = reader.MoveNext()) != IonType.None)
            {
                var name = AsIonFieldName(reader.CurrentFieldName);
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
                writer.SetFieldName(AsPropertyName(property.Name));
                ionSerializer.Serialize(writer, property.GetValue(item));
            }
            writer.StepOut();
        }

        private String AsPropertyName(string property) 
        {
            return property.Substring(0, 1).ToLowerInvariant() + property.Substring(1);
        }

        private String AsIonFieldName(string property) 
        {
            return property.Substring(0, 1).ToUpperInvariant() + property.Substring(1);
        }
    }
}