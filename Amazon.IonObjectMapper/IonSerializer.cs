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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Amazon.IonDotnet;

    /// <summary>
    /// Ion Serializer.
    /// </summary>
    public class IonSerializer
    {
        private readonly IonSerializationOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonSerializer"/> class.
        /// </summary>
        public IonSerializer()
            : this(new IonSerializationOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonSerializer"/> class.
        /// </summary>
        ///
        /// <param name="options">Serialization options for customizing serializer behavior.</param>
        public IonSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Serialize value to Ion.
        /// </summary>
        ///
        /// <param name="item">The value to serialize.</param>
        /// <typeparam name="T">The type of data to serialize.</typeparam>
        ///
        /// <returns>The serialized stream.</returns>
        public Stream Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            this.Serialize(stream, item);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Serialize value to Ion.
        /// </summary>
        ///
        /// <param name="stream">The stream to be written with serialized Ion.</param>
        /// <param name="item">The value to serialize.</param>
        /// <typeparam name="T">The type of data to serialize.</typeparam>
        public void Serialize<T>(Stream stream, T item)
        {
            IIonWriter writer = this.options.WriterFactory.Create(this.options, stream);
            this.Serialize(writer, item);

            // We use `IIonWriter.Flush` instead of `IIonWriter.Finish` here. Although the `Finish` method
            // documentation says "all written values will be flushed", that's only true for Ion binary writer.
            // For Ion text writer, the `Finish` method is empty and only the `Flush` method calls the implemented
            // `TextWriter.Flush` method that causes any buffered data to be written to the underlying device.
            writer.Flush();
        }

        /// <summary>
        /// Serialize value to Ion.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The value to serialize.</param>
        /// <typeparam name="T">The type of data to serialize.</typeparam>
        public void Serialize<T>(IIonWriter writer, T item)
        {
            if (item == null)
            {
                new IonNullSerializer().Serialize(writer, null);
                return;
            }

            if (item is bool)
            {
                var serializer = this.GetPrimitiveSerializer<bool>(new IonBooleanSerializer());
                serializer.Serialize(writer, Convert.ToBoolean(item));
                return;
            }

            if (item is int)
            {
                var serializer = this.GetPrimitiveSerializer<int>(new IonIntSerializer());
                serializer.Serialize(writer, Convert.ToInt32(item));
                return;
            }

            if (item is long)
            {
                var serializer = this.GetPrimitiveSerializer<long>(new IonLongSerializer());
                serializer.Serialize(writer, Convert.ToInt64(item));
                return;
            }

            if (item is float)
            {
                var serializer = this.GetPrimitiveSerializer<float>(new IonFloatSerializer());
                serializer.Serialize(writer, Convert.ToSingle(item));
                return;
            }

            if (item is double)
            {
                var serializer = this.GetPrimitiveSerializer<double>(new IonDoubleSerializer());
                serializer.Serialize(writer, Convert.ToDouble(item));
                return;
            }

            if (item is decimal)
            {
                var serializer = this.GetPrimitiveSerializer<decimal>(new IonDecimalSerializer());
                serializer.Serialize(writer, Convert.ToDecimal(item));
                return;
            }

            if (item is BigDecimal)
            {
                var serializer = this.GetPrimitiveSerializer<BigDecimal>(new IonBigDecimalSerializer());
                serializer.Serialize(writer, (BigDecimal)(object)item);
                return;
            }

            if (item is byte[])
            {
                var serializer = this.GetPrimitiveSerializer<byte[]>(new IonByteArraySerializer());
                serializer.Serialize(writer, (byte[])(object)item);
                return;
            }

            if (item is string)
            {
                var serializer = this.GetPrimitiveSerializer<string>(new IonStringSerializer());
                serializer.Serialize(writer, item as string);
                return;
            }

            if (item is SymbolToken)
            {
                var serializer = this.GetPrimitiveSerializer<SymbolToken>(new IonSymbolSerializer());
                serializer.Serialize(writer, (SymbolToken)(object)item);
                return;
            }

            if (item is DateTime)
            {
                var serializer = this.GetPrimitiveSerializer<DateTime>(new IonDateTimeSerializer());
                serializer.Serialize(writer, (DateTime)(object)item);
                return;
            }

            if (item is Guid)
            {
                var serializer = this.GetPrimitiveSerializer<Guid>(new IonGuidSerializer(this.options));
                serializer.Serialize(writer, (Guid)(object)item);
                return;
            }

            if (item is IList list)
            {
                this.NewIonListSerializer(item.GetType()).Serialize(writer, list);
                return;
            }

            Type genericDictionaryType = item.GetType().GetInterfaces().FirstOrDefault(t =>
                t.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(t.GetGenericTypeDefinition()));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();

                if (typeof(string).IsAssignableFrom(genericArguments[0]))
                {
                    new IonDictionarySerializer(this, genericArguments[1]).Serialize(writer, (IDictionary)item);
                    return;
                }
                else
                {
                    throw new NotSupportedException("Can not serialize IDictionary when key is not of type string");
                }
            }

            if (item is object)
            {
                Type itemType = item.GetType();

                // The AnnotatedIonSerializers takes precedence over IonSerializerAttribute
                var ionAnnotateTypes = (IEnumerable<IonAnnotateTypeAttribute>)itemType.GetCustomAttributes(typeof(IonAnnotateTypeAttribute), false);
                if (this.TryAnnotatedIonSerializer(writer, item, ionAnnotateTypes))
                {
                    return;
                }

                var customSerializerAttribute = itemType.GetCustomAttribute<IonSerializerAttribute>();
                if (customSerializerAttribute != null)
                {
                    var customSerializer = this.CreateCustomSerializer(itemType);
                    customSerializer.Serialize(writer, item);
                    return;
                }

                new IonObjectSerializer(this, this.options, itemType).Serialize(writer, item);
                return;
            }

            throw new NotSupportedException($"Do not know how to serialize type {typeof(T)}");
        }

        /// <summary>
        /// Deserialize value from Ion.
        /// </summary>
        ///
        /// <param name="stream">The stream to be read during deserialization.</param>
        /// <typeparam name="T">The type of data to deserialize to.</typeparam>
        ///
        /// <returns>The deserialized value.</returns>
        public T Deserialize<T>(Stream stream)
        {
            return this.Deserialize<T>(this.options.ReaderFactory.Create(stream));
        }

        /// <summary>
        /// Deserialize value from Ion.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        /// <param name="type">The target .NET type for deserialization.</param>
        ///
        /// <returns>The deserialized value.</returns>
        public object Deserialize(IIonReader reader, Type type)
        {
            return this.Deserialize(reader, type, reader.MoveNext());
        }

        /// <summary>
        /// Deserialize value from Ion.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        /// <param name="type">The target .NET type for deserialization.</param>
        /// <param name="ionType">The Ion type of the current Ion field.</param>
        ///
        /// <returns>The deserialized value.</returns>
        public object Deserialize(IIonReader reader, Type type, IonType ionType)
        {
            if (reader.CurrentDepth > this.options.MaxDepth)
            {
                return null;
            }

            var annotations = reader.GetTypeAnnotations();

            // The AnnotatedIonSerializers takes precedence over IonSerializerAttribute
            if (this.options.AnnotatedIonSerializers != null)
            {
                foreach (var ionAnnotateType in annotations)
                {
                    if (this.options.AnnotatedIonSerializers.ContainsKey(ionAnnotateType))
                    {
                        var serializer = this.options.AnnotatedIonSerializers[ionAnnotateType];
                        return serializer.Deserialize(reader);
                    }
                }
            }

            if (type != null)
            {
                var customSerializerAttribute = type.GetCustomAttribute<IonSerializerAttribute>();
                if (customSerializerAttribute != null)
                {
                    var customSerializer = this.CreateCustomSerializer(type);
                    return customSerializer.Deserialize(reader);
                }
            }

            if (ionType == IonType.None || ionType == IonType.Null)
            {
                return new IonNullSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Bool)
            {
                var serializer = this.GetPrimitiveSerializer<bool>(new IonBooleanSerializer());
                return serializer.Deserialize(reader);
            }

            if (ionType == IonType.Int)
            {
                if (annotations.Any(s => s.Equals(IonLongSerializer.ANNOTATION)))
                {
                    var serializer = this.GetPrimitiveSerializer<long>(new IonLongSerializer());
                    return serializer.Deserialize(reader);
                }
                else
                {
                    var serializer = this.GetPrimitiveSerializer<int>(new IonIntSerializer());
                    return serializer.Deserialize(reader);
                }
            }

            if (ionType == IonType.Float)
            {
                if (annotations.Any(s => s.Equals(IonFloatSerializer.ANNOTATION)))
                {
                    var serializer = this.GetPrimitiveSerializer<float>(new IonFloatSerializer());
                    return serializer.Deserialize(reader);
                }
                else
                {
                    var serializer = this.GetPrimitiveSerializer<double>(new IonDoubleSerializer());
                    return serializer.Deserialize(reader);
                }
            }

            if (ionType == IonType.Decimal)
            {
                if (annotations.Any(s => s.Equals(IonDecimalSerializer.ANNOTATION)))
                {
                    var serializer = this.GetPrimitiveSerializer<decimal>(new IonDecimalSerializer());
                    return serializer.Deserialize(reader);
                }
                else
                {
                    var serializer = this.GetPrimitiveSerializer<BigDecimal>(new IonBigDecimalSerializer());
                    return serializer.Deserialize(reader);
                }
            }

            if (ionType == IonType.Blob)
            {
                if (annotations.Any(s => s.Equals(IonGuidSerializer.ANNOTATION))
                    || typeof(Guid).IsAssignableFrom(type))
                {
                    var serializer = this.GetPrimitiveSerializer<Guid>(new IonGuidSerializer(this.options));
                    return serializer.Deserialize(reader);
                }
                else
                {
                    var serializer = this.GetPrimitiveSerializer<byte[]>(new IonByteArraySerializer());
                    return serializer.Deserialize(reader);
                }
            }

            if (ionType == IonType.String)
            {
                var serializer = this.GetPrimitiveSerializer<string>(new IonStringSerializer());
                return serializer.Deserialize(reader);
            }

            if (ionType == IonType.Symbol)
            {
                var serializer = this.GetPrimitiveSerializer<SymbolToken>(new IonSymbolSerializer());
                return serializer.Deserialize(reader);
            }

            if (ionType == IonType.Timestamp)
            {
                var serializer = this.GetPrimitiveSerializer<DateTime>(new IonDateTimeSerializer());
                return serializer.Deserialize(reader);
            }

            if (ionType == IonType.Clob)
            {
                return new IonClobSerializer().Deserialize(reader);
            }

            if (ionType == IonType.List)
            {
                return this.NewIonListSerializer(type).Deserialize(reader);
            }

            if (ionType == IonType.Struct)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                {
                    var genericArguments = type.GetGenericArguments();
                    if (typeof(string).IsAssignableFrom(genericArguments[0]))
                    {
                        var dictionarySerializer = new IonDictionarySerializer(this, genericArguments[1]);
                        var deserialized = dictionarySerializer.Deserialize(reader);

                        return deserialized;
                    }
                    else
                    {
                        throw new NotSupportedException("Can not deserialize into Dictionary when key is not of type string");
                    }
                }

                return new IonObjectSerializer(this, this.options, type).Deserialize(reader);
            }

            throw new NotSupportedException($"Data with Ion type {ionType} is not supported for deserialization");
        }

        /// <summary>
        /// Deserialize value from Ion.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        /// <typeparam name="T">The type of data to deserialize to.</typeparam>
        ///
        /// <returns>The deserialized value.</returns>
        public T Deserialize<T>(IIonReader reader)
        {
            return (T)this.Deserialize(reader, typeof(T));
        }

        /// <summary>
        /// Serialize value to Ion using serializer identified by <see cref="IonAnnotateTypeAttribute"/>.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The value to serialize.</param>
        /// <param name="annotationAttributes">The <see cref="IonAnnotateTypeAttribute"/> to identify custom serializer for serialization.</param>
        /// <typeparam name="T">The type of data to serialize.</typeparam>
        ///
        /// <returns>True if the value is serialized to Ion using AnnotatedIonSerializer. Otherwise return false.</returns>
        internal bool TryAnnotatedIonSerializer<T>(IIonWriter writer, T item, IEnumerable<IonAnnotateTypeAttribute> annotationAttributes)
        {
            if (this.options.AnnotatedIonSerializers != null)
            {
                foreach (IonAnnotateTypeAttribute annotationAttribute in annotationAttributes)
                {
                    string standardizedAnnotation = this.options.AnnotationConvention.Apply(annotationAttribute, item.GetType());
                    if (this.options.AnnotatedIonSerializers.ContainsKey(standardizedAnnotation))
                    {
                        var serializer = this.options.AnnotatedIonSerializers[standardizedAnnotation];
                        writer.AddTypeAnnotation(standardizedAnnotation);
                        serializer.Serialize(writer, item);
                        return true;
                    }
                }
            }

            return false;
        }

        private IonListSerializer NewIonListSerializer(Type listType)
        {
            if (listType.IsArray)
            {
                return new IonListSerializer(this, listType, listType.GetElementType());
            }

            if (typeof(System.Collections.IList).IsAssignableFrom(listType))
            {
                if (listType.IsGenericType)
                {
                    return new IonListSerializer(this, listType, listType.GetGenericArguments()[0]);
                }

                return new IonListSerializer(this, listType);
            }

            throw new NotSupportedException($"Encountered an Ion list but the desired deserialized type was not an IList, it was: {listType}");
        }

        private IIonSerializer GetPrimitiveSerializer<T>(IIonSerializer defaultSerializer)
        {
            if (this.options.IonSerializers != null && this.options.IonSerializers.ContainsKey(typeof(T)))
            {
                return this.options.IonSerializers[typeof(T)];
            }

            return defaultSerializer;
        }

        private IIonSerializer CreateCustomSerializer(Type targetType)
        {
            var customSerializerAttribute = targetType.GetCustomAttribute<IonSerializerAttribute>();
            if (customSerializerAttribute.Factory != null)
            {
                var customSerializerFactory = (IIonSerializerFactory)Activator.CreateInstance(customSerializerAttribute.Factory);
                return customSerializerFactory.Create(this.options, this.options.CustomContext);
            }
            else if (customSerializerAttribute.Serializer != null)
            {
                return (IIonSerializer)Activator.CreateInstance(customSerializerAttribute.Serializer);
            }

            throw new InvalidOperationException($"[IonSerializer] annotated type {targetType} should have a valid IonSerializerAttribute Factory or Serializer");
        }
    }
}