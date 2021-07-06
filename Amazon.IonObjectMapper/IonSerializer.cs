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

    public class IonSerializer
    {
        private readonly IonSerializationOptions options;

        public IonSerializer()
            : this(new IonSerializationOptions())
        {
        }

        public IonSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        public Stream Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            this.Serialize(stream, item);
            stream.Position = 0;
            return stream;
        }

        public void Serialize<T>(Stream stream, T item)
        {
            IIonWriter writer = this.options.WriterFactory.Create(stream);
            this.Serialize(writer, item);
            writer.Finish();
            writer.Flush();
        }

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

            Type genericDictionaryType = item.GetType().GetInterfaces().FirstOrDefault(t => t.GetGenericTypeDefinition().IsAssignableTo(typeof(IDictionary<,>)));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();

                if (genericArguments[0].IsAssignableTo(typeof(string)))
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
                var customSerializerAttribute = item.GetType().GetCustomAttribute<IonSerializerAttribute>();
                if (customSerializerAttribute != null)
                {
                    var customSerializer = this.CreateCustomSerializer(item.GetType());
                    customSerializer.Serialize(writer, item);
                    return;
                }

                new IonObjectSerializer(this, this.options, item.GetType()).Serialize(writer, item);
                return;
            }

            throw new NotSupportedException($"Do not know how to serialize type {typeof(T)}");
        }

        public T Deserialize<T>(Stream stream)
        {
            return this.Deserialize<T>(this.options.ReaderFactory.Create(stream));
        }

        public object Deserialize(IIonReader reader, Type type)
        {
            return this.Deserialize(reader, type, reader.MoveNext());
        }

        public object Deserialize(IIonReader reader, Type type, IonType ionType)
        {
            if (reader.CurrentDepth > this.options.MaxDepth)
            {
                return null;
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
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonLongSerializer.ANNOTATION)))
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
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonFloatSerializer.ANNOTATION)))
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
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonDecimalSerializer.ANNOTATION)))
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
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonGuidSerializer.ANNOTATION))
                    || type.IsAssignableTo(typeof(Guid)))
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
                    if (genericArguments[0].IsAssignableTo(typeof(string)))
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

        public T Deserialize<T>(IIonReader reader)
        {
            return (T)this.Deserialize(reader, typeof(T));
        }

        private IonListSerializer NewIonListSerializer(Type listType)
        {
            if (listType.IsArray)
            {
                return new IonListSerializer(this, listType, listType.GetElementType());
            }

            if (listType.IsAssignableTo(typeof(System.Collections.IList)))
            {
                if (listType.IsGenericType)
                {
                    return new IonListSerializer(this, listType, listType.GetGenericArguments()[0]);
                }

                return new IonListSerializer(this, listType);
            }

            throw new NotSupportedException("Encountered an Ion list but the desired deserialized type was not an IList, it was: " + listType);
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
