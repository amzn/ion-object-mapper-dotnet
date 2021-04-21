using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonObjectSerializer : IonSerializer<object>
    {
        private const BindingFlags fieldBindings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
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
            var targetObject = options.ObjectFactory.Create(options, reader, targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType  = reader.MoveNext()) != IonType.None)
            {
                
                var property = FindProperty(reader.CurrentFieldName);
                FieldInfo field;
                if (property != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
                    property.SetValue(targetObject, deserialized);
                }
                else if ((field = FindField(reader.CurrentFieldName)) != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, field.FieldType, ionType);
                    field.SetValue(targetObject, deserialized);
                }
            }
            reader.StepOut();
            return targetObject;
        }

        public void Serialize(IIonWriter writer, object item)
        {
            options.TypeAnnotator.Apply(options, writer, targetType);
            writer.StepIn(IonType.Struct);
            foreach (var property in targetType.GetProperties())
            {
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
                {
                    continue;
                }
                writer.SetFieldName(IonFieldNameFromProperty(property));
                ionSerializer.Serialize(writer, property.GetValue(item));
            }

            foreach (var field in IonFields())
            {
                writer.SetFieldName(GetFieldName(field));
                ionSerializer.Serialize(writer, field.GetValue(item));
            }
            writer.StepOut();
        }

        private string IonFieldNameFromProperty(PropertyInfo property)
        {
            var ionPropertyName = property.GetCustomAttribute(typeof(IonPropertyName));
            if (ionPropertyName != null) 
            {
                return ((IonPropertyName)ionPropertyName).Name;
            }
            return options.NamingConvention.FromProperty(property.Name);
        }

        private PropertyInfo FindProperty(string readName)
        {
            var exact = IonNamedProperties().FirstOrDefault(p => 
                {
                    var ionPropertyName = p.GetCustomAttribute<IonPropertyName>();
                    if (ionPropertyName != null)
                    {
                        return p.GetCustomAttribute<IonPropertyName>().Name == readName;
                    }
                    return false;
                });
            if (exact != null)
            {
                return exact;
            }

            var name = options.NamingConvention.ToProperty(readName);
            return targetType.GetProperty(name);
        }
        private FieldInfo FindField(string name)
        {
            var exact = targetType.GetField(name, fieldBindings);
            if (exact != null && IsIonField(exact))
            {
                return exact;
            }
            return IonFields().FirstOrDefault(f => 
            {
                var propertyName = f.GetCustomAttribute(typeof(IonPropertyName));
                if (propertyName != null)
                {
                    return name == ((IonPropertyName)propertyName).Name;
                }
                return false;
            });
        }

        private static bool IsIonField(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IonField)) != null;
        }

        private static bool IsIonNamedProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IonPropertyName)) != null;
        }

        private IEnumerable<FieldInfo> IonFields()
        {
            return targetType.GetFields(fieldBindings).Where(IsIonField);
        }

        private IEnumerable<PropertyInfo> IonNamedProperties()
        {
            return targetType.GetProperties().Where(IsIonNamedProperty);
        }

        private string GetFieldName(FieldInfo field)
        {
            var propertyName = field.GetCustomAttribute(typeof(IonPropertyName));
            if (propertyName != null)
            {
                return ((IonPropertyName)propertyName).Name;
            }
            return field.Name;
        }
    }
}