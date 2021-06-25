using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.IonDotnet;

namespace Amazon.IonObjectMapper
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
                () => GetValidProperties(true).Where(IsReadOnlyProperty));
        }

        public override object Deserialize(IIonReader reader)
        {
            object targetObject = null;
            ConstructorInfo ionConstructor = null;
            ParameterInfo[] parameters = null;
            object[] constructorArgs = null;
            Dictionary<string, int> constructorArgIndexMap = null;

            // Determine if we are using an annotated constructor.
            var ionConstructors = targetType.GetConstructors(BINDINGS).Where(IsIonConstructor).Take(2);
            if (ionConstructors.Any())
            {
                if (ionConstructors.Count() > 1)
                {
                    throw new InvalidOperationException(
                        $"Only one constructor in class {targetType.Name} may be annotated " +
                        "with the [IonConstructor] attribute and more than one was detected");
                }

                ionConstructor = ionConstructors.First();
                parameters = ionConstructor.GetParameters();
                constructorArgs = new object[parameters.Length];
                constructorArgIndexMap = this.BuildConstructorArgIndexMap(parameters);
            }
            else
            {
                // If we are not using an annotated constructor, then we need to construct the object before stepping
                // into the reader in case we need to read the type annotations during construction.
                targetObject = options.ObjectFactory.Create(options, reader, targetType);
            }

            reader.StepIn();

            // Read the Ion and organize deserialized results into three categories:
            // 1. Values to be set via annotated methods.
            // 2. Properties to be set.
            // 3. Fields to be set.
            // Any of these deserialized results may also be used for the annotated constructor (if there is one). 
            var deserializedMethods = new List<(MethodInfo, object)>();
            var deserializedProperties = new List<(PropertyInfo, object)>();
            var deserializedFields = new List<(FieldInfo, object)>();
            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                MethodInfo method;
                PropertyInfo property;
                FieldInfo field;
                object deserialized = null;
                bool currentIonFieldProcessed = false;

                // Check if current Ion field has an annotated method.
                if ((method = FindSetter(reader.CurrentFieldName)) != null)
                {
                    if (this.TryDeserializeMethod(method, reader, ionType, out deserialized))
                    {
                        deserializedMethods.Add((method, deserialized));
                    }
                    currentIonFieldProcessed = true;
                }
                // Check if current Ion field is a .NET property.
                else if ((property = FindProperty(reader.CurrentFieldName)) != null)
                {
                    if (this.TryDeserializeProperty(property, reader, ionType, out deserialized))
                    {
                        deserializedProperties.Add((property, deserialized));
                    }
                    currentIonFieldProcessed = true;
                }
                // Check if current Ion field is a .NET field.
                else if ((field = FindField(reader.CurrentFieldName)) != null)
                {
                    if (this.TryDeserializeField(field, reader, ionType, out deserialized))
                    {
                        deserializedFields.Add((field, deserialized));
                    }
                    currentIonFieldProcessed = true;
                }
                
                // Check if current Ion field is also an argument for an annotated constructor.
                if (constructorArgIndexMap != null && constructorArgIndexMap.ContainsKey(reader.CurrentFieldName))
                {
                    var index = constructorArgIndexMap[reader.CurrentFieldName];
                    
                    // Deserialize current Ion field only if it was not already
                    // processed by the above method/property/field logic.
                    if (!currentIonFieldProcessed)
                    {
                        deserialized = ionSerializer.Deserialize(reader, parameters[index].ParameterType, ionType);
                    }
                    
                    constructorArgs[index] = deserialized;
                }
            }

            reader.StepOut();

            // Construct object with annotated constructor if we have one.
            if (ionConstructor != null)
            {
                targetObject = ionConstructor.Invoke(constructorArgs);
            }

            // Set values with annotated methods.
            foreach (var (method, deserialized) in deserializedMethods)
            {
                method.Invoke(targetObject, new[]{ deserialized });
            }
            
            // Set properties.
            foreach (var (property, deserialized) in deserializedProperties)
            {
                property.SetValue(targetObject, deserialized);
            }
            
            // Set fields.
            foreach (var (field, deserialized) in deserializedFields)
            {
                field.SetValue(targetObject, deserialized);
            }

            return targetObject;
        }

        public override void Serialize(IIonWriter writer, object item)
        {
            options.TypeAnnotator.Apply(options, writer, targetType);
            writer.StepIn(IonType.Struct);

            var serializedIonFields = new HashSet<string>();
            
            // Serialize the values returned from IonPropertyGetter annotated getter methods.
            foreach (var (method, ionPropertyName) in this.GetGetters())
            {
                var getValue = method.Invoke(item, Array.Empty<object>());
                
                writer.SetFieldName(ionPropertyName);
                ionSerializer.Serialize(writer, getValue);
                
                serializedIonFields.Add(ionPropertyName);
            }

            // Serialize any properties that satisfy the options/attributes.
            foreach (var property in GetValidProperties(true))
            {
                var ionPropertyName = IonFieldNameFromProperty(property);
                if (serializedIonFields.Contains(ionPropertyName))
                {
                    // This Ion property name was already serialized.
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

                writer.SetFieldName(ionPropertyName);
                ionSerializer.Serialize(writer, propertyValue);
            }

            // Serialize any fields that satisfy the options/attributes.
            foreach (var field in Fields())
            {
                var ionFieldName = GetFieldName(field);
                if (serializedIonFields.Contains(ionFieldName))
                {
                    // This Ion field name was already serialized.
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

                writer.SetFieldName(ionFieldName);
                ionSerializer.Serialize(writer, fieldValue);
            }

            writer.StepOut();
        }

        // Deserialize the given method and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeMethod(MethodInfo method, IIonReader reader, IonType ionType, out object deserialized)
        {
            // A setter should have exactly one argument.
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                throw new InvalidOperationException(
                    "An [IonPropertySetter] annotated method should have exactly one argument " +
                    $"but {method.Name} has {parameters.Length} arguments");
            }

            deserialized = ionSerializer.Deserialize(reader, parameters[0].ParameterType, ionType);

            return true;
        }
        
        // Deserialize the given property and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeProperty(
            PropertyInfo property, IIonReader reader, IonType ionType, out object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
            
            if (IsReadOnlyProperty(property))
            {
                // property.SetValue() does not work with a readonly property.
                // logic for handling deserializing readonly properties happens during field processing
                // when we detect backing fields for the property.
                return false;
            }

            return !options.IgnoreDefaults || deserialized != default;
        }
        
        // Deserialize the given field and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeField(FieldInfo field, IIonReader reader, IonType ionType, out object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = ionSerializer.Deserialize(reader, field.FieldType, ionType);
            
            if (options.IgnoreReadOnlyFields && field.IsInitOnly)
            {
                return false;
            }

            return !options.IgnoreDefaults || deserialized != default;
        }

        // Compute mapping between parameter names and index in parameter array so we can figure out the
        // correct order of the constructor arguments.
        private Dictionary<string, int> BuildConstructorArgIndexMap(ParameterInfo[] parameters)
        {
            var constructorArgIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var ionPropertyName = (IonPropertyName)parameters[i].GetCustomAttribute(typeof(IonPropertyName));
                if (ionPropertyName == null)
                {
                    throw new InvalidOperationException(
                        $"Parameter '{parameters[i].Name}' is not specified with the [IonPropertyName] attribute " +
                        $"for {targetType.Name}'s IonConstructor. All constructor arguments must be annotated " +
                        "so we know which parameters to set at construction time.");
                }

                constructorArgIndexMap.Add(ionPropertyName.Name, i);
            }

            return constructorArgIndexMap;
        }

        private IEnumerable<(MethodInfo, string)> GetGetters()
        {
            var getters = new List<(MethodInfo, string)>();
            foreach (var method in targetType.GetMethods(BINDINGS))
            {
                var getMethod = (IonPropertyGetter)method.GetCustomAttribute(typeof(IonPropertyGetter));
                
                // A getter method should have zero parameters.
                if (getMethod?.IonPropertyName == null || method.GetParameters().Length != 0)
                {
                    continue;
                }
                
                getters.Add((method, getMethod.IonPropertyName));
            }

            return getters;
        }
        
        private MethodInfo FindSetter(string name)
        {
            return targetType.GetMethods(BINDINGS).FirstOrDefault(m =>
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
            var exact = GetValidProperties(false).Where(IsIonNamedProperty).FirstOrDefault(p => 
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
                return GetValidProperties(false).FirstOrDefault(p => String.Equals(p.Name, readName, StringComparison.OrdinalIgnoreCase));
            }

            var name = options.NamingConvention.ToProperty(readName);
            return GetValidProperties(false).FirstOrDefault(p => String.Equals(p.Name, name));
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

        private static bool IsIonConstructor(ConstructorInfo constructor)
        {
            return constructor.GetCustomAttribute(typeof(IonConstructor)) != null;
        }

        private static bool IsIonField(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IonField)) != null;
        }

        private static bool IsIonNamedProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IonPropertyName)) != null;
        }

        /// <summary>
        /// We only serialize public, internal, and protected internal properties or properties with IonPropertyName annotation.
        /// </summary>
        /// <param name="isGetter">Specify if we are looking for getters or setters in the property.</param>
        private static bool HasValidAccessModifier(PropertyInfo propertyInfo, bool isGetter)
        {
            var methodInfo = isGetter ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);

            return methodInfo != null && (methodInfo.IsPublic || methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly
                || propertyInfo.GetCustomAttribute(typeof(IonPropertyName)) != null);
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

        private string GetFieldName(FieldInfo field)
        {
            var propertyName = field.GetCustomAttribute(typeof(IonPropertyName));
            if (propertyName != null)
            {
                return ((IonPropertyName)propertyName).Name;
            }
            return field.Name;
        }

        private IEnumerable<PropertyInfo> GetValidProperties(bool isGetter)
        {
            return targetType.GetRuntimeProperties().Where(x => HasValidAccessModifier(x, isGetter));
        }
    }
}
