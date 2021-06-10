using Amazon.IonDotnet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Amazon.Ion.ObjectMapper
{
    public class IonDictionarySerializer : IonSerializer<IDictionary>
    {
        private IonSerializer serializer;
        private Type valueType;

        public IonDictionarySerializer(IonSerializer ionSerializer, Type valueType)
        {
            this.serializer = ionSerializer;
            this.valueType = valueType;
        }

        public override IDictionary Deserialize(IIonReader reader)
        {
            reader.StepIn();

            Type typedDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), this.valueType);
            MethodInfo methodInfo = typedDictionaryType.GetMethod("Add");
            var dictionary = Activator.CreateInstance(typedDictionaryType);
            IonType currentType;
            while ((currentType = reader.MoveNext()) != IonType.None)
            {
                object[] parameters = new object[] { 
                    reader.CurrentFieldName, 
                    Convert.ChangeType(this.serializer.Deserialize(reader, valueType, currentType), valueType) };
                methodInfo.Invoke(dictionary, parameters);

            }

            reader.StepOut();
            return (IDictionary)dictionary;
        }

        public override void Serialize(IIonWriter writer, IDictionary item)
        {
            writer.StepIn(IonType.Struct);
            foreach (DictionaryEntry nameValuePair in item) {
                writer.SetFieldName((string)nameValuePair.Key);
                this.serializer.Serialize(writer, nameValuePair.Value);
            }

            writer.StepOut();
        }
    }
}
