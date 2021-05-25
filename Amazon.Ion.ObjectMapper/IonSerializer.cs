using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using static Amazon.Ion.ObjectMapper.IonSerializationFormat;

namespace Amazon.Ion.ObjectMapper
{
    public interface IonSerializer<T>
    {
        void Serialize(IIonWriter writer, T item);
        T Deserialize(IIonReader reader);
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
                var assemblyName = options.AnnotatedTypeAssemblies.First(a => Type.GetType(FullName(typeName, a)) != null);
                return Activator.CreateInstance(Type.GetType(FullName(typeName, assemblyName)));
            }
            return Activator.CreateInstance(targetType);
        }

        private string FullName(string typeName, string assemblyName)
        {
            return typeName + ", " + assemblyName;
        }
    }

    public class IonSerializationOptions
    {
        public IonPropertyNamingConvention NamingConvention { get; init; } = new CamelCaseNamingConvention();
        public IonSerializationFormat Format { get; init; } = TEXT;
        public readonly int MaxDepth;
        public bool AnnotateGuids { get; init; } = false;

        public bool IncludeFields { get; init; } = false;
        public bool IgnoreNulls { get; init; } = false;
        public bool IgnoreReadOnlyFields { get; init; } = false;
        public readonly bool IgnoreReadOnlyProperties;
        public readonly bool PropertyNameCaseInsensitive;
        public bool IgnoreDefaults { get; init; } = false;
        public bool IncludeTypeInformation { get; init; } = false;
        public TypeAnnotationPrefix TypeAnnotationPrefix { get; init; } = new NamespaceTypeAnnotationPrefix();
        public TypeAnnotator TypeAnnotator { get; init; } = new DefaultTypeAnnotator();

        public IonReaderFactory ReaderFactory { get; init; } = new DefaultIonReaderFactory();
        public IonWriterFactory WriterFactory { get; init; } = new DefaultIonWriterFactory();

        public ObjectFactory ObjectFactory { get; init; } = new DefaultObjectFactory();
        public string[] AnnotatedTypeAssemblies { get; init; } = new string[] {};

        public readonly bool PermissiveMode;

        public Dictionary<Type, dynamic> IonSerializers { get; init; }
    }

    public interface IonSerializerFactory<T, TContext> where TContext : IonSerializationContext
    {
        public IonSerializer<T> create(IonSerializationOptions options, TContext context);
    }

    public class IonSerializer
    {
        internal readonly IonSerializationOptions options;
        private Dictionary<Type, dynamic> primitiveSerializers { get; init; }
        private IonNullSerializer nullSerializer { get; init; }
        private IonClobSerializer clobSerializer { get; init; }
        private IonListSerializer listSerializer { get; init; }
        private IonObjectSerializer objectSerializer { get; init; }

        public IonSerializer() : this(new IonSerializationOptions())
        {
        }

        public IonSerializer(IonSerializationOptions options)
        {
            this.options = options;
            this.primitiveSerializers = new Dictionary<Type, dynamic>()
            {
                {typeof(bool), new IonBooleanSerializer()},
                {typeof(string), new IonStringSerializer()},
                {typeof(byte[]), new IonByteArraySerializer()},
                {typeof(int), new IonIntSerializer()},
                {typeof(long), new IonLongSerializer()},
                {typeof(float), new IonFloatSerializer()},
                {typeof(double), new IonDoubleSerializer()},
                {typeof(decimal), new IonDecimalSerializer()},
                {typeof(BigDecimal), new IonBigDecimalSerializer()},
                {typeof(SymbolToken), new IonSymbolSerializer()},
                {typeof(DateTime), new IonDateTimeSerializer()},
                {typeof(Guid), new IonGuidSerializer(this.options.AnnotateGuids)},
            };
            this.nullSerializer = new IonNullSerializer();
            this.clobSerializer = new IonClobSerializer();
            this.listSerializer = new IonListSerializer(this);
            this.objectSerializer = new IonObjectSerializer(this);

            if (this.options.IonSerializers != null)
            {
                foreach (var serializer in this.options.IonSerializers)
                {
                    if (ValidateCustomSerializer(serializer.Key, serializer.Value))
                    {
                        this.primitiveSerializers[serializer.Key] = serializer.Value;
                    }
                    else
                    {
                        throw new NotSupportedException($"Custom serializer does not satisfy IonSerializer<{serializer.Key}> interface");
                    }
                }
            }
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
                this.nullSerializer.Serialize(writer, null);
                return;
            }

            Type type = item.GetType();
            if (this.primitiveSerializers.ContainsKey(type))
            {
                this.SerializePrimitive(type, writer, item);
                return;
            }

            switch (item)
            {
                case IList:
                    this.listSerializer.SetListType(type);
                    this.listSerializer.Serialize(writer, item);
                    break;
                case object:
                    this.objectSerializer.targetType = type;
                    this.objectSerializer.Serialize(writer, item);
                    break;
                default:
                    throw new NotSupportedException($"Do not know how to serialize type {typeof(T)}");
            }
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
            if (ionType == IonType.None || ionType == IonType.Null)
            {
                return this.nullSerializer.Deserialize(reader);
            }

            if (ionType == IonType.Bool)
            {
                return this.primitiveSerializers[typeof(bool)].Deserialize(reader);
            }

            if (ionType == IonType.Int)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonLongSerializer.ANNOTATION)))
                {
                    return this.primitiveSerializers[typeof(long)].Deserialize(reader);
                }
                return this.primitiveSerializers[typeof(int)].Deserialize(reader);
            }

            if (ionType == IonType.Float)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonFloatSerializer.ANNOTATION)))
                {
                    return this.primitiveSerializers[typeof(float)].Deserialize(reader);
                }
                return this.primitiveSerializers[typeof(double)].Deserialize(reader);
            }

            if (ionType == IonType.Decimal)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonDecimalSerializer.ANNOTATION)))
                {
                    return this.primitiveSerializers[typeof(decimal)].Deserialize(reader);
                }
                return this.primitiveSerializers[typeof(BigDecimal)].Deserialize(reader);
            }

            if (ionType == IonType.Blob) 
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonGuidSerializer.ANNOTATION))
                    || type.IsAssignableTo(typeof(Guid)))
                {
                    return this.primitiveSerializers[typeof(Guid)].Deserialize(reader);
                }
                return this.primitiveSerializers[typeof(byte[])].Deserialize(reader);
            }

            if (ionType == IonType.String) 
            {
                return this.primitiveSerializers[typeof(string)].Deserialize(reader);
            }

            if (ionType == IonType.Symbol) 
            {
                return this.primitiveSerializers[typeof(SymbolToken)].Deserialize(reader);
            }

            if (ionType == IonType.Timestamp) 
            {
                return this.primitiveSerializers[typeof(DateTime)].Deserialize(reader);
            }

            if (ionType == IonType.Clob) 
            {
                return this.clobSerializer.Deserialize(reader);
            }

            if (ionType == IonType.List) 
            {
                this.listSerializer.SetListType(type);
                return this.listSerializer.Deserialize(reader);
            }

            if (ionType == IonType.Struct) 
            {
                this.objectSerializer.targetType = type;
                return this.objectSerializer.Deserialize(reader);
            }

            throw new NotSupportedException($"Data with Ion type {ionType} is not supported for deserialization");
        }

        public T Deserialize<T>(IIonReader reader)
        {
            return (T) Deserialize(reader, typeof(T));
        }
        
        private bool ValidateCustomSerializer(Type type, dynamic serializer)
        {
            if (!this.primitiveSerializers.ContainsKey(type))
            {
                throw new NotSupportedException($"Custom serializer for {type} is not supported");
            }
            
            if (type == typeof(bool))
                return serializer is IonSerializer<bool>;

            if (type == typeof(string))
                return serializer is IonSerializer<string>;

            if (type == typeof(byte[]))
                return serializer is IonSerializer<byte[]>;

            if (type == typeof(int))
                return serializer is IonSerializer<int>;

            if (type == typeof(long))
                return serializer is IonSerializer<long>;

            if (type == typeof(float))
                return serializer is IonSerializer<float>;

            if (type == typeof(double))
                return serializer is IonSerializer<double>;
            
            if (type == typeof(decimal))
                return serializer is IonSerializer<decimal>;
            
            if (type == typeof(BigDecimal))
                return serializer is IonSerializer<BigDecimal>;

            if (type == typeof(SymbolToken))
                return serializer is IonSerializer<SymbolToken>;
            
            if (type == typeof(DateTime))
                return serializer is IonSerializer<DateTime>;

            if (type == typeof(Guid))
                return serializer is IonSerializer<Guid>;

            return false;
        }

        private void SerializePrimitive(Type type, IIonWriter writer, object item)
        {
            if (type == typeof(bool))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToBoolean(item));

            else if (type == typeof(string))
                this.primitiveSerializers[type].Serialize(writer, item as string);

            else if (type == typeof(byte[]))
                this.primitiveSerializers[type].Serialize(writer, (byte[])item);

            else if (type == typeof(int))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToInt32(item));

            else if (type == typeof(long))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToInt64(item));

            else if (type == typeof(float))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToSingle(item));

            else if (type == typeof(double))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToDouble(item));
            
            else if (type == typeof(decimal))
                this.primitiveSerializers[type].Serialize(writer, Convert.ToDecimal(item));
            
            else if (type == typeof(BigDecimal))
                this.primitiveSerializers[type].Serialize(writer, (BigDecimal)item);

            else if (type == typeof(SymbolToken))
                this.primitiveSerializers[type].Serialize(writer, (SymbolToken)item);
            
            else if (type == typeof(DateTime))
                this.primitiveSerializers[type].Serialize(writer, (DateTime)item);

            else if (type == typeof(Guid))
                this.primitiveSerializers[type].Serialize(writer, (Guid)item);
        }
    }
}
