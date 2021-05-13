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
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                PropertyInfo property;
                FieldInfo field;
                MethodInfo method;
                
                // Check if current ion field is a .NET property
                if ((property = FindProperty(reader.CurrentFieldName)) != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
                    
                    if (options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }
                    
                    property.SetValue(targetObject, deserialized);
                }
                // Check if current ion field is a .NET field
                else if ((field = FindField(reader.CurrentFieldName)) != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, field.FieldType, ionType);
                    
                    if (options.IgnoreReadOnlyFields && field.IsInitOnly)
                    {
                        continue;
                    }
                    if (options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }

                    field.SetValue(targetObject, deserialized);
                }
                // Check if current ion field has a setter method
                else if ((method = FindSetter(reader.CurrentFieldName)) != null)
                {
                    var deserialized = ionSerializer.Deserialize(reader, method.GetParameters()[0].ParameterType, ionType);
                    method.Invoke(targetObject, new[]{ deserialized });
                }
            }
            reader.StepOut();
            return targetObject;
        }

        public void Serialize(IIonWriter writer, object item)
        {
            options.TypeAnnotator.Apply(options, writer, targetType);
            writer.StepIn(IonType.Struct);
            
            // Serialize any properties that satisfy the options/attributes
            foreach (var property in targetType.GetProperties())
            {
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
                {
                    continue;
                }
                
                var propertyValue = property.GetValue(item);
                if (options.IgnoreNulls && propertyValue == null)
                {
                    continue;
                }
                if (options.IgnoreDefaults && propertyValue == default)
                {
                    continue;
                }

                writer.SetFieldName(IonFieldNameFromProperty(property));
                ionSerializer.Serialize(writer, propertyValue);
            }

            // Serialize any fields that satisfy the options/attributes
            foreach (var field in Fields())
            {
                var fieldValue = field.GetValue(item);
                if (options.IgnoreNulls && fieldValue == null)
                {
                    continue;
                }
                if (options.IgnoreReadOnlyFields && field.IsInitOnly)
                {
                    continue;
                }
                if (options.IgnoreDefaults && fieldValue == default)
                {
                    continue;
                }

                writer.SetFieldName(GetFieldName(field));
                ionSerializer.Serialize(writer, fieldValue);
            }

            // Serialize the values returned from getter methods
            // as long as they have zero arguments and are annotated with the IonPropertyGetter attribute.
            foreach (var method in targetType.GetMethods())
            {
                var getMethod = (IonPropertyGetter)method.GetCustomAttribute(typeof(IonPropertyGetter));
                if (getMethod?.FieldName == null || method.GetParameters().Length != 0)
                {
                    continue;
                }

                writer.SetFieldName(getMethod.FieldName);
                var getValue = method.Invoke(item, Array.Empty<object>());
                ionSerializer.Serialize(writer, getValue);
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
            if (exact != null && IsField(exact))
            {
                return exact;
            }

            return Fields().FirstOrDefault(f => 
            {
                var propertyName = f.GetCustomAttribute(typeof(IonPropertyName));
                if (propertyName != null)
                {
                    return name == ((IonPropertyName)propertyName).Name;
                }
                return false;
            });
        }

        private MethodInfo FindSetter(string name)
        {
            return targetType.GetMethods().FirstOrDefault(m =>
            {
                // A setter should be a one argument void method with the IonPropertySetter attribute
                var setMethod = (IonPropertySetter)m.GetCustomAttribute(typeof(IonPropertySetter));
                return setMethod != null && 
                       setMethod.FieldName == name &&
                       m.ReturnParameter != null && 
                       m.ReturnParameter.ParameterType == typeof(void) && 
                       m.GetParameters().Length == 1;
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
        
        private bool IsField(FieldInfo field)
        {
            if (options.IncludeFields)
            {
                return true;
            }

            return IsIonField(field);
        }

        private IEnumerable<FieldInfo> Fields()
        {
            return targetType.GetFields(fieldBindings).Where(IsField);
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