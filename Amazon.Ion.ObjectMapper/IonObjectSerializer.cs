/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace Amazon.Ion.ObjectMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Amazon.IonDotnet;

    public class IonObjectSerializer : IonSerializer<object>
    {
        private const BindingFlags FieldBindings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
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
            var targetObject = this.options.ObjectFactory.Create(this.options, reader, this.targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                var property = this.FindProperty(reader.CurrentFieldName);
                FieldInfo field;
                if (property != null)
                {
                    var deserialized = this.ionSerializer.Deserialize(reader, property.PropertyType, ionType);

                    if (this.options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }

                    property.SetValue(targetObject, deserialized);
                }
                else if ((field = this.FindField(reader.CurrentFieldName)) != null)
                {
                    var deserialized = this.ionSerializer.Deserialize(reader, field.FieldType, ionType);

                    if (this.options.IgnoreReadOnlyFields && field.IsInitOnly)
                    {
                        continue;
                    }

                    if (this.options.IgnoreDefaults && deserialized == default)
                    {
                        continue;
                    }

                    field.SetValue(targetObject, deserialized);
                }
            }

            reader.StepOut();
            return targetObject;
        }

        public void Serialize(IIonWriter writer, object item)
        {
            this.options.TypeAnnotator.Apply(this.options, writer, this.targetType);
            writer.StepIn(IonType.Struct);
            foreach (var property in this.targetType.GetProperties())
            {
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
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

                writer.SetFieldName(this.IonFieldNameFromProperty(property));
                this.ionSerializer.Serialize(writer, propertyValue);
            }

            foreach (var field in this.Fields())
            {
                var fieldValue = field.GetValue(item);
                if (this.options.IgnoreNulls && fieldValue == null)
                {
                    continue;
                }

                if (this.options.IgnoreReadOnlyFields && field.IsInitOnly)
                {
                    continue;
                }

                if (this.options.IgnoreDefaults && fieldValue == default)
                {
                    continue;
                }

                writer.SetFieldName(this.GetFieldName(field));
                this.ionSerializer.Serialize(writer, fieldValue);
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

            return this.options.NamingConvention.FromProperty(property.Name);
        }

        private PropertyInfo FindProperty(string readName)
        {
            var exact = this.IonNamedProperties().FirstOrDefault(p =>
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

            var name = this.options.NamingConvention.ToProperty(readName);
            return this.targetType.GetProperty(name);
        }

        private FieldInfo FindField(string name)
        {
            var exact = this.targetType.GetField(name, FieldBindings);
            if (exact != null && this.IsField(exact))
            {
                return exact;
            }

            return this.Fields().FirstOrDefault(f =>
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
            if (this.options.IncludeFields)
            {
                return true;
            }

            return IsIonField(field);
        }

        private IEnumerable<FieldInfo> Fields()
        {
            return this.targetType.GetFields(FieldBindings).Where(this.IsField);
        }

        private IEnumerable<PropertyInfo> IonNamedProperties()
        {
            return this.targetType.GetProperties().Where(IsIonNamedProperty);
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
