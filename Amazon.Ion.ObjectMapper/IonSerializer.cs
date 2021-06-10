using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using static Amazon.Ion.ObjectMapper.IonSerializationFormat;

namespace Amazon.Ion.ObjectMapper
{
    public interface IIonSerializer
    {
        void Serialize(IIonWriter writer, object item);
        object Deserialize(IIonReader reader);
    }

    public abstract class IonSerializer<T> : IIonSerializer
    {
        public abstract void Serialize(IIonWriter writer, T item);
        public abstract T Deserialize(IIonReader reader);

        void IIonSerializer.Serialize(IIonWriter writer, object item)
        {
            Serialize(writer, (T)item);
        }

        object IIonSerializer.Deserialize(IIonReader reader)
        {
            return Deserialize(reader);
        }
    }

    public interface IonSerializationContext
    {

    }

    public enum IonSerializationFormat 
    {
        BINARY,
        TEXT, 
        PRETTY_TEXT
    }

    public interface IonReaderFactory
    {
        IIonReader Create(Stream stream);
    }

    public class DefaultIonReaderFactory : IonReaderFactory
    {
        public IIonReader Create(Stream stream)
        {
            return IonReaderBuilder.Build(stream, new ReaderOptions {Format = ReaderFormat.Detect});
        }
    }
    
    public interface IonWriterFactory
    {
        IIonWriter Create(Stream stream);
    }

    public class DefaultIonWriterFactory : IonWriterFactory
    {
        private IonSerializationFormat format = TEXT;

        public DefaultIonWriterFactory()
        {
        }

        public DefaultIonWriterFactory(IonSerializationFormat format)
        {
            this.format = format;
        }

        public IIonWriter Create(Stream stream)
        {
            switch (format)
            {
                case BINARY:
                    return IonBinaryWriterBuilder.Build(stream);
                case TEXT:
                    return IonTextWriterBuilder.Build(new StreamWriter(stream));
                case PRETTY_TEXT:
                    return IonTextWriterBuilder.Build(new StreamWriter(stream),
                        new IonTextOptions {PrettyPrint = true});
                default:
                    throw new InvalidOperationException($"Format {format} not supported");
            }
        }
    }

    public interface ObjectFactory
    {
        object Create(IonSerializationOptions options, IIonReader reader, Type targetType);
    }

    public class DefaultObjectFactory : ObjectFactory
    {
        public object Create(IonSerializationOptions options, IIonReader reader, Type targetType)
        {
            var annotations = reader.GetTypeAnnotations();
            if (annotations.Length > 0)
            {
                var typeName = annotations[0];
                Type typeToCreate = null;

                if (options.AnnotatedTypeAssemblies != null)
                {
                    foreach (string assemblyName in options.AnnotatedTypeAssemblies)
                    {
                        if ((typeToCreate = Type.GetType(FullName(typeName, assemblyName))) != null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    typeToCreate = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .FirstOrDefault(type => string.Equals(type.Name, typeName, StringComparison.OrdinalIgnoreCase));
                }

                if (typeToCreate != null && targetType.IsAssignableFrom(typeToCreate))
                {
                    return Activator.CreateInstance(typeToCreate);
                }
            }
            return Activator.CreateInstance(targetType);
        }

        private string FullName(string typeName, string assemblyName)
        {
            return assemblyName + "." + typeName + "," + assemblyName;
        }
    }

    public class IonSerializationOptions
    {
        public IonPropertyNamingConvention NamingConvention { get; init; } = new CamelCaseNamingConvention();
        public IonSerializationFormat Format { get; init; } = TEXT;
        public int MaxDepth { get; init; } = 64;
        public bool AnnotateGuids { get; init; } = false;

        public bool IncludeFields { get; init; } = false;
        public bool IgnoreNulls { get; init; } = false;
        public bool IgnoreReadOnlyFields { get; init; } = false;
        public bool IgnoreReadOnlyProperties { get; init; } = false;
        public bool PropertyNameCaseInsensitive { get; init; } = false;
        public bool IgnoreDefaults { get; init; } = false;
        public bool IncludeTypeInformation { get; init; } = false;
        public TypeAnnotationPrefix TypeAnnotationPrefix { get; init; } = new NamespaceTypeAnnotationPrefix();

        public TypeAnnotationName TypeAnnotationName { get; init; } = new ClassNameTypeAnnotationName();

        public AnnotationConvention AnnotationConvention { get; init; } = new DefaultAnnotationConvention();

        public TypeAnnotator TypeAnnotator { get; init; } = new DefaultTypeAnnotator();

        public IonReaderFactory ReaderFactory { get; init; } = new DefaultIonReaderFactory();
        public IonWriterFactory WriterFactory { get; init; } = new DefaultIonWriterFactory();

        public ObjectFactory ObjectFactory { get; init; } = new DefaultObjectFactory();
        public IEnumerable<string> AnnotatedTypeAssemblies { get; init; }

        public readonly bool PermissiveMode;
        
        public Dictionary<Type, IIonSerializer> IonSerializers { get; init; }
    }

    public interface IonSerializerFactory<T, TContext> where TContext : IonSerializationContext
    {
        public IonSerializer<T> create(IonSerializationOptions options, TContext context);
    }

    public class IonSerializer
    {
        private readonly IonSerializationOptions options;

        public IonSerializer() : this(new IonSerializationOptions())
        {

        }

        public IonSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        public Stream Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            Serialize(stream, item);
            stream.Position = 0;
            return stream;
        }

        public void Serialize<T>(Stream stream, T item)
        {
            IIonWriter writer = options.WriterFactory.Create(stream);
            Serialize(writer, item);
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
                var serializer = this.GetPrimitiveSerializer<Guid>(new IonGuidSerializer(options));
                serializer.Serialize(writer, (Guid)(object)item);
                return;
            }

            if (item is System.Collections.IList)
            {
                NewIonListSerializer(item.GetType()).Serialize(writer, (System.Collections.IList)item);
                return;
            }

            if (item is object)
            {
                new IonObjectSerializer(this, options, item.GetType()).Serialize(writer, item);
                return;
            }

            throw new NotSupportedException($"Do not know how to serialize type {typeof(T)}");
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


        public T Deserialize<T>(Stream stream)
        {
            return Deserialize<T>(options.ReaderFactory.Create(stream));
        }

        public object Deserialize(IIonReader reader, Type type)
        {
            return Deserialize(reader, type, reader.MoveNext());
        }

        public object Deserialize(IIonReader reader, Type type, IonType ionType)
        {
            if (reader.CurrentDepth > this.options.MaxDepth)
            {
                return null;
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
                    var serializer = this.GetPrimitiveSerializer<Guid>(new IonGuidSerializer(options));
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
                return NewIonListSerializer(type).Deserialize(reader);
            }

            if (ionType == IonType.Struct) 
            {
                return new IonObjectSerializer(this, options, type).Deserialize(reader);
            }

            throw new NotSupportedException($"Data with Ion type {ionType} is not supported for deserialization");
        }

        public T Deserialize<T>(IIonReader reader)
        {
            return (T) Deserialize(reader, typeof(T));
        }

        private IIonSerializer GetPrimitiveSerializer<T>(IIonSerializer defaultSerializer)
        {
            if (this.options.IonSerializers != null && this.options.IonSerializers.ContainsKey(typeof(T)))
            {
                return this.options.IonSerializers[typeof(T)];
            }

            return defaultSerializer;
        }
    }
}
