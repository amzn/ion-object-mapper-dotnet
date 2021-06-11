using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonObjectSerializer : IonSerializer<object>
    {
        private const BindingFlags BINDINGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        private readonly IonSerializer ionSerializer;
        private readonly IonSerializationOptions options;
        private readonly Type targetType;
        private readonly Lazy<IEnumerable<PropertyInfo>> readOnlyProperties;

        public IonObjectSerializer(IonSerializer ionSerializer, IonSerializationOptions options, Type targetType)
        {
            this.ionSerializer = ionSerializer;
            this.options = options;
            this.targetType = targetType;
            this.readOnlyProperties = new Lazy<IEnumerable<PropertyInfo>>(
                () => this.targetType.GetProperties().Where(IsReadOnlyProperty));
        }

        public override object Deserialize(IIonReader reader)
        {
            var targetObject = options.ObjectFactory.Create(options, reader, targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                MethodInfo method;
                PropertyInfo property;
                FieldInfo field;

                // Check if current Ion field has a IonPropertySetter annotated setter method.
                if ((method = FindSetter(reader.CurrentFieldName)) != null)
                {
                    // A setter should be a void method.
                    if (method.ReturnParameter?.ParameterType != typeof(void))
                    {
                        continue;
                    }

                    // A setter should have exactly one argument.
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    var deserialized = ionSerializer.Deserialize(reader, parameters[0].ParameterType, ionType);
                    if (options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }

                    method.Invoke(targetObject, new[]{ deserialized });
                }
                // Check if current Ion field is a .NET property.
                else if ((property = FindProperty(reader.CurrentFieldName)) != null)
                {
                    if (IsReadOnlyProperty(property))
                    {
                        // property.SetValue() does not work with a readonly property.
                        // logic for handling deserializing readonly properties happens during field processing
                        // when we detect backing fields for the property.
                        continue;
                    }

                    var deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
                    if (options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }

                    property.SetValue(targetObject, deserialized);
                }
                // Check if current Ion field is a .NET field.
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
            }
            reader.StepOut();
            return targetObject;
        }

        public override void Serialize(IIonWriter writer, object item)
        {
            options.TypeAnnotator.Apply(options, writer, targetType);
            writer.StepIn(IonType.Struct);

            var serializedViaMethod = new HashSet<string>();
            
            // Serialize the values returned from IonPropertyGetter annotated getter methods.
            foreach (var method in targetType.GetMethods())
            {
                var getMethod = (IonPropertyGetter)method.GetCustomAttribute(typeof(IonPropertyGetter));
                
                // A getter method should have zero parameters.
                if (getMethod?.IonPropertyName == null || method.GetParameters().Length != 0)
                {
                    continue;
                }

                var getValue = method.Invoke(item, Array.Empty<object>());
                if (options.IgnoreNulls && getValue == null)
                {
                    continue;
                }
                if (options.IgnoreDefaults && getValue == default)
                {
                    continue;
                }
                
                writer.SetFieldName(getMethod.IonPropertyName);
                ionSerializer.Serialize(writer, getValue);
                
                serializedViaMethod.Add(getMethod.IonPropertyName);
            }

            // Serialize any properties that satisfy the options/attributes.
            foreach (var property in targetType.GetProperties())
            {
                if (serializedViaMethod.Contains(property.Name))
                {
                    // This property name was already serialized using an annotated method.
                    continue;
                }
                
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
                {
                    continue;
                }

                if (this.options.IgnoreReadOnlyProperties && IsReadOnlyProperty(property))
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

            // Serialize any fields that satisfy the options/attributes.
            foreach (var field in Fields())
            {
                if (serializedViaMethod.Contains(field.Name))
                {
                    // This field name was already serialized using an annotated method.
                    continue;
                }
                
                if (options.IgnoreReadOnlyFields && field.IsInitOnly)
                {
                    continue;
                }

                var fieldValue = field.GetValue(item);
                if (options.IgnoreNulls && fieldValue == null)
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

            writer.StepOut();
        }

        private MethodInfo FindSetter(string name)
        {
            return targetType.GetMethods().FirstOrDefault(m =>
            {
                var setMethod = (IonPropertySetter)m.GetCustomAttribute(typeof(IonPropertySetter));
                return setMethod != null && setMethod.IonPropertyName == name;
            });
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

            if (options.PropertyNameCaseInsensitive)
            {
                return targetType.GetProperties().FirstOrDefault(p => String.Equals(p.Name, readName, StringComparison.OrdinalIgnoreCase));
            }

            var name = options.NamingConvention.ToProperty(readName);
            var property = targetType.GetProperty(name, BINDINGS);

            return property;
        }

        private bool IsReadOnlyProperty(PropertyInfo property)
        {
            return property.SetMethod == null;
        }
        
        private FieldInfo FindField(string name)
        {
            var exact = targetType.GetField(name, BINDINGS);
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

            if (!this.options.IgnoreReadOnlyProperties &&
                this.readOnlyProperties.Value.Any(p => field.Name == $"<{p.Name}>k__BackingField"))
            {
                return true;
            }

            return IsIonField(field);
        }

        private IEnumerable<FieldInfo> Fields()
        {
            return targetType.GetFields(BINDINGS).Where(IsField);
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
