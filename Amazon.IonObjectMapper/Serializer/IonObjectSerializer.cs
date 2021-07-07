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

namespace Amazon.IonObjectMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Amazon.IonDotnet;

    /// <summary>
    /// Ion Serializer for non-primitive object types.
    /// </summary>
    public class IonObjectSerializer : IonSerializer<object>
    {
        private const BindingFlags BINDINGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        private readonly IonSerializer ionSerializer;
        private readonly IonSerializationOptions options;
        private readonly Type targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonObjectSerializer"/> class.
        /// </summary>
        ///
        /// <param name="ionSerializer">Serializer to be used for serializing/deserializing non-primitive objects.</param>
        /// <param name="options">Serialization options for customizing serializer behavior.</param>
        /// <param name="targetType">The type of data being serialized/deserialized.</param>
        public IonObjectSerializer(IonSerializer ionSerializer, IonSerializationOptions options, Type targetType)
        {
            this.ionSerializer = ionSerializer;
            this.options = options;
            this.targetType = targetType;
        }

        /// <inheritdoc/>
        public override object Deserialize(IIonReader reader)
        {
            object targetObject = null;
            ConstructorInfo ionConstructor = null;
            ParameterInfo[] parameters = null;
            object[] constructorArgs = null;
            Dictionary<string, int> constructorArgIndexMap = null;

            // Determine if we are using an annotated constructor.
            var ionConstructors = this.targetType.GetConstructors(BINDINGS).Where(IsIonConstructor).Take(2);
            if (ionConstructors.Any())
            {
                if (ionConstructors.Count() > 1)
                {
                    throw new InvalidOperationException(
                        $"Only one constructor in class {this.targetType.Name} may be annotated with the [IonConstructor] attribute and more than one was detected");
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
                targetObject = this.options.ObjectFactory.Create(this.options, reader, this.targetType);
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
                if ((method = this.FindSetter(reader.CurrentFieldName)) != null)
                {
                    if (this.TryDeserializeMethod(method, reader, ionType, out deserialized))
                    {
                        deserializedMethods.Add((method, deserialized));
                    }

                    currentIonFieldProcessed = true;
                }

                // Check if current Ion field is a .NET property.
                else if ((property = this.FindProperty(reader.CurrentFieldName)) != null)
                {
                    if (this.TryDeserializeProperty(property, reader, ionType, out deserialized))
                    {
                        deserializedProperties.Add((property, deserialized));
                    }

                    currentIonFieldProcessed = true;
                }

                // Check if current Ion field is a .NET field.
                else if ((field = this.FindField(reader.CurrentFieldName)) != null)
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
                        deserialized = this.ionSerializer.Deserialize(reader, parameters[index].ParameterType, ionType);
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
                method.Invoke(targetObject, new[] { deserialized });
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

        /// <inheritdoc/>
        public override void Serialize(IIonWriter writer, object item)
        {
            this.options.TypeAnnotator.Apply(this.options, writer, this.targetType);
            writer.StepIn(IonType.Struct);

            var serializedIonFields = new HashSet<string>();

            // Serialize the values returned from IonPropertyGetter annotated getter methods.
            foreach (var (method, ionPropertyName) in this.GetGetters())
            {
                var getValue = method.Invoke(item, Array.Empty<object>());

                writer.SetFieldName(ionPropertyName);
                this.ionSerializer.Serialize(writer, getValue);

                serializedIonFields.Add(ionPropertyName);
            }

            // Serialize any properties that satisfy the options/attributes.
            foreach (var property in this.GetValidProperties(true))
            {
                var ionPropertyName = this.IonFieldNameFromProperty(property);
                if (serializedIonFields.Contains(ionPropertyName))
                {
                    // This Ion property name was already serialized.
                    continue;
                }

                if (this.options.IgnoreReadOnlyProperties && IsReadOnlyProperty(property))
                {
                    continue;
                }

                var propertyValue = property.GetValue(item);
                if (this.options.IgnoreNulls && propertyValue == null)
                {
                    continue;
                }

                if (this.options.IgnoreDefaults && propertyValue == default)
                {
                    continue;
                }

                writer.SetFieldName(ionPropertyName);
                this.ionSerializer.Serialize(writer, propertyValue);
            }

            // Serialize any fields that satisfy the options/attributes.
            foreach (var field in this.Fields())
            {
                var ionFieldName = GetFieldName(field);
                if (serializedIonFields.Contains(ionFieldName))
                {
                    // This Ion field name was already serialized.
                    continue;
                }

                if (this.options.IgnoreReadOnlyFields && field.IsInitOnly)
                {
                    continue;
                }

                var fieldValue = field.GetValue(item);
                if (this.options.IgnoreNulls && fieldValue == null)
                {
                    continue;
                }

                if (this.options.IgnoreDefaults && fieldValue == default)
                {
                    continue;
                }

                writer.SetFieldName(ionFieldName);
                this.ionSerializer.Serialize(writer, fieldValue);
            }

            writer.StepOut();
        }

        private static bool IsIonConstructor(ConstructorInfo constructor)
        {
            return constructor.GetCustomAttribute(typeof(IonConstructorAttribute)) != null;
        }

        private static bool IsIonField(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IonFieldAttribute)) != null;
        }

        private static bool IsIonNamedProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IonPropertyNameAttribute)) != null;
        }

        private static bool IsIonIgnore(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IonIgnoreAttribute)) != null;
        }

        /// <summary>
        /// We only serialize public, internal, and protected internal properties or properties with IonPropertyName annotation.
        /// </summary>
        /// <param name="isGetter">Specify if we are looking for getters or setters in the property.</param>
        private static bool HasValidAccessModifier(PropertyInfo propertyInfo, bool isGetter)
        {
            var methodInfo = isGetter ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);

            return methodInfo != null && (methodInfo.IsPublic || methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly
                || propertyInfo.GetCustomAttribute(typeof(IonPropertyNameAttribute)) != null);
        }

        private static bool IsReadOnlyProperty(PropertyInfo property)
        {
            return property.SetMethod == null;
        }

        private static string GetFieldName(FieldInfo field)
        {
            var propertyName = (IonPropertyNameAttribute)field.GetCustomAttribute(typeof(IonPropertyNameAttribute));
            if (propertyName != null)
            {
                return propertyName.Name;
            }

            return field.Name;
        }

        // Deserialize the given method and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeMethod(MethodInfo method, IIonReader reader, IonType ionType, out object deserialized)
        {
            // A setter should have exactly one argument.
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                throw new InvalidOperationException(
                    $"An [IonPropertySetter] annotated method should have exactly one argument but {method.Name} has {parameters.Length} arguments");
            }

            deserialized = this.ionSerializer.Deserialize(reader, parameters[0].ParameterType, ionType);

            return true;
        }

        // Deserialize the given property and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeProperty(
            PropertyInfo property, IIonReader reader, IonType ionType, out object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = this.ionSerializer.Deserialize(reader, property.PropertyType, ionType);

            return !this.IgnoreDeserializedProperty(deserialized);
        }

        // Deserialize the given field and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeField(FieldInfo field, IIonReader reader, IonType ionType, out object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = this.ionSerializer.Deserialize(reader, field.FieldType, ionType);

            return !this.IgnoreDeserializedField(field, deserialized);
        }

        private bool IgnoreDeserializedProperty(object deserialized)
        {
            return this.options.IgnoreDefaults && deserialized == default;
        }

        private bool IgnoreDeserializedField(FieldInfo field, object deserialized)
        {
            // Check if this field is really a backing field for a readonly property and
            // if so, apply the ignore property logic instead of the ignore field logic.
            if (!this.options.IgnoreReadOnlyProperties && this.IsBackingFieldForReadonlyProperty(field))
            {
                return this.IgnoreDeserializedProperty(deserialized);
            }

            return (this.options.IgnoreReadOnlyFields && field.IsInitOnly) ||
                   (this.options.IgnoreDefaults && deserialized == default);
        }

        private bool IsBackingFieldForReadonlyProperty(FieldInfo field)
        {
            var readonlyProperties = this.GetValidProperties(true).Where(IsReadOnlyProperty);
            return readonlyProperties.Any(p => field.Name == $"<{p.Name}>k__BackingField");
        }

        // Compute mapping between parameter names and index in parameter array so we can figure out the
        // correct order of the constructor arguments.
        private Dictionary<string, int> BuildConstructorArgIndexMap(ParameterInfo[] parameters)
        {
            var constructorArgIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var ionPropertyName = (IonPropertyNameAttribute)parameters[i].GetCustomAttribute(typeof(IonPropertyNameAttribute));
                if (ionPropertyName == null)
                {
                    throw new InvalidOperationException(
                        $"Parameter '{parameters[i].Name}' is not specified with the [IonPropertyName] attribute for {this.targetType.Name}'s IonConstructor. All constructor arguments must be annotated so we know which parameters to set at construction time.");
                }

                constructorArgIndexMap.Add(ionPropertyName.Name, i);
            }

            return constructorArgIndexMap;
        }

        private IEnumerable<(MethodInfo, string)> GetGetters()
        {
            var getters = new List<(MethodInfo, string)>();
            foreach (var method in this.targetType.GetMethods(BINDINGS))
            {
                var getMethod = (IonPropertyGetterAttribute)method.GetCustomAttribute(typeof(IonPropertyGetterAttribute));

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
            return this.targetType.GetMethods(BINDINGS).FirstOrDefault(m =>
            {
                var setMethod = (IonPropertySetterAttribute)m.GetCustomAttribute(typeof(IonPropertySetterAttribute));
                return setMethod != null && setMethod.IonPropertyName == name;
            });
        }

        private string IonFieldNameFromProperty(PropertyInfo property)
        {
            var ionPropertyName = (IonPropertyNameAttribute)property.GetCustomAttribute(typeof(IonPropertyNameAttribute));
            if (ionPropertyName != null)
            {
                return ionPropertyName.Name;
            }

            return this.options.NamingConvention.FromProperty(property.Name);
        }

        private PropertyInfo FindProperty(string readName)
        {
            var exact = this.GetValidProperties(false).Where(IsIonNamedProperty).FirstOrDefault(p =>
                {
                    var ionPropertyName = p.GetCustomAttribute<IonPropertyNameAttribute>();
                    if (ionPropertyName != null)
                    {
                        return ionPropertyName.Name == readName;
                    }

                    return false;
                });
            if (exact != null)
            {
                return exact;
            }

            if (this.options.PropertyNameCaseInsensitive)
            {
                return this.GetValidProperties(false).FirstOrDefault(p => string.Equals(p.Name, readName, StringComparison.OrdinalIgnoreCase));
            }

            var name = this.options.NamingConvention.ToProperty(readName);
            return this.GetValidProperties(false).FirstOrDefault(p => string.Equals(p.Name, name));
        }

        private FieldInfo FindField(string name)
        {
            var exact = this.targetType.GetField(name, BINDINGS);
            if (exact != null)
            {
                if (this.IsField(exact))
                {
                    return exact;
                }
            }
            else if (!this.options.IgnoreReadOnlyProperties)
            {
                // Check if this field is a backing field for a readonly property.
                var propertyName = this.options.NamingConvention.ToProperty(name);
                var readonlyProperties = this.GetValidProperties(true).Where(IsReadOnlyProperty);
                if (readonlyProperties.Any(p => p.Name == propertyName))
                {
                    var backingField = this.targetType.GetField($"<{propertyName}>k__BackingField", BINDINGS);
                    if (backingField != null)
                    {
                        return backingField;
                    }
                }
            }

            return this.Fields().FirstOrDefault(f =>
            {
                var propertyName = (IonPropertyNameAttribute)f.GetCustomAttribute(typeof(IonPropertyNameAttribute));
                if (propertyName != null)
                {
                    return name == propertyName.Name;
                }

                return false;
            });
        }

        private bool IsField(FieldInfo field)
        {
            return this.options.IncludeFields || IsIonField(field);
        }

        private IEnumerable<FieldInfo> Fields()
        {
            return this.targetType.GetFields(BINDINGS).Where(this.IsField);
        }

        private IEnumerable<PropertyInfo> GetValidProperties(bool isGetter)
        {
            return this.targetType.GetRuntimeProperties().Where(p => !IsIonIgnore(p) && HasValidAccessModifier(p, isGetter));
        }
    }
}
