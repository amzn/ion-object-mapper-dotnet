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
                this.primitiveSerializers[type].Serialize(writer, item);
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
            dynamic serializer;
            switch (ionType)
            {
                case IonType.None:
                case IonType.Null:
                    serializer = this.nullSerializer;
                    break;
                case IonType.Bool:
                    serializer = this.primitiveSerializers[typeof(bool)];
                    break;
                case IonType.Int when reader.GetTypeAnnotations().Any(s => s.Equals(IonLongSerializer.ANNOTATION)):
                    serializer = this.primitiveSerializers[typeof(long)];
                    break;
                case IonType.Int:
                    serializer = this.primitiveSerializers[typeof(int)];
                    break;
                case IonType.Float when reader.GetTypeAnnotations().Any(s => s.Equals(IonFloatSerializer.ANNOTATION)):
                    serializer = this.primitiveSerializers[typeof(float)];
                    break;
                case IonType.Float:
                    serializer = this.primitiveSerializers[typeof(double)];
                    break;
                case IonType.Decimal when reader.GetTypeAnnotations().Any(s => s.Equals(IonDecimalSerializer.ANNOTATION)):
                    serializer = this.primitiveSerializers[typeof(decimal)];
                    break;
                case IonType.Decimal:
                    serializer = this.primitiveSerializers[typeof(BigDecimal)];
                    break;
                case IonType.Blob when reader.GetTypeAnnotations().Any(s => s.Equals(IonGuidSerializer.ANNOTATION))
                                       || type.IsAssignableTo(typeof(Guid)):
                    serializer = this.primitiveSerializers[typeof(Guid)];
                    break;
                case IonType.Blob:
                    serializer = this.primitiveSerializers[typeof(byte[])];
                    break;
                case IonType.String:
                    serializer = this.primitiveSerializers[typeof(string)];
                    break;
                case IonType.Symbol:
                    serializer = this.primitiveSerializers[typeof(SymbolToken)];
                    break;
                case IonType.Timestamp:
                    serializer = this.primitiveSerializers[typeof(DateTime)];
                    break;
                case IonType.Clob:
                    serializer = this.clobSerializer;
                    break;
                case IonType.List:
                    this.listSerializer.SetListType(type);
                    serializer = this.listSerializer;
                    break;
                case IonType.Struct:
                    this.objectSerializer.targetType = type;
                    serializer = this.objectSerializer;
                    break;
                default:
                    throw new NotSupportedException(
                        $"Don't know how to Deserialize this Ion data. Last IonType was: {ionType}");
            }
            
            return serializer.Deserialize(reader);
        }

        public T Deserialize<T>(IIonReader reader)
        {
            return (T) Deserialize(reader, typeof(T));
        }
    }
}
