using System;
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
        public bool PropertyNameCaseInsensitive { get; init; } = false;
        public bool IgnoreReadOnlyProperties { get; init; } = false;
        public bool IgnoreDefaults { get; init; } = false;
        public bool IncludeTypeInformation { get; init; } = false;
        public TypeAnnotationPrefix TypeAnnotationPrefix { get; init; } = new NamespaceTypeAnnotationPrefix();

        public TypeAnnotationName TypeAnnotationName { get; init; } = new ClassNameTypeAnnotationName();

        public AnnotationConvention AnnotationConvention { get; init; } = new DefaultAnnotationConvention();

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
                new IonNullSerializer().Serialize(writer, (object)null);
                return;
            }

            if (item is bool)
            {
                new IonBooleanSerializer().Serialize(writer, Convert.ToBoolean(item));
                return;
            }

            if (item is int)
            {
                new IonIntSerializer().Serialize(writer, Convert.ToInt32(item));
                return;
            }

            if (item is long)
            {
                new IonLongSerializer().Serialize(writer, Convert.ToInt64(item));
                return;
            }

            if (item is float)
            {
                new IonFloatSerializer().Serialize(writer, Convert.ToSingle(item));
                return;
            }

            if (item is double)
            {
                new IonDoubleSerializer().Serialize(writer, Convert.ToDouble(item));
                return;
            }

            if (item is decimal)
            {
                new IonDecimalSerializer().Serialize(writer, Convert.ToDecimal(item));
                return;
            }

            if (item is BigDecimal)
            {
                new IonBigDecimalSerializer().Serialize(writer, (BigDecimal)(object)item);
                return;
            }

            if (item is byte[])
            {
                new IonByteArraySerializer().Serialize(writer, (byte[])(object)item);
                return;
            }

            if (item is string)
            {
                new IonStringSerializer().Serialize(writer, item as string);
                return;
            }

            if (item is SymbolToken)
            {
                new IonSymbolSerializer().Serialize(writer, (SymbolToken)(object)item);
                return;
            }

            if (item is DateTime)
            {
                new IonDateTimeSerializer().Serialize(writer, (DateTime)(object)item);
                return;
            }

            if (item is System.Collections.IList) 
            {
                NewIonListSerializer(item.GetType()).Serialize(writer, (System.Collections.IList)(object)item);
                return;
            }

            if (item is Guid) 
            {
                new IonGuidSerializer(options).Serialize(writer, (Guid)(object)item);
                return;
            }

            if (item is object) 
            {
                new IonObjectSerializer(this, options, item.GetType()).Serialize(writer, item);
                return;
            }

            throw new NotSupportedException("Do not know how to serialize type " + typeof(T));
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
            if (ionType == IonType.None || ionType == IonType.Null)
            {
                return new IonNullSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Bool)
            {
                return new IonBooleanSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Int)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonLongSerializer.ANNOTATION)))
                {
                    return new IonLongSerializer().Deserialize(reader);
                }
                return new IonIntSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Float)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonFloatSerializer.ANNOTATION)))
                {
                    return new IonFloatSerializer().Deserialize(reader);
                }
                return new IonDoubleSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Decimal)
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonDecimalSerializer.ANNOTATION)))
                {
                    return new IonDecimalSerializer().Deserialize(reader);
                }
                return new IonBigDecimalSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Blob) 
            {
                if (reader.GetTypeAnnotations().Any(s => s.Equals(IonGuidSerializer.ANNOTATION))
                    || type.IsAssignableTo(typeof(Guid)))
                {
                    return new IonGuidSerializer(options).Deserialize(reader);
                }
                return new IonByteArraySerializer().Deserialize(reader);
            }

            if (ionType == IonType.String) 
            {
                return new IonStringSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Symbol) 
            {
                return new IonSymbolSerializer().Deserialize(reader);
            }

            if (ionType == IonType.Timestamp) 
            {
                return new IonDateTimeSerializer().Deserialize(reader);
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

            throw new NotSupportedException("Don't know how to Deserialize this Ion data. Last IonType was: " + ionType);
        }

        public T Deserialize<T>(IIonReader reader)
        {
            return (T) Deserialize(reader, typeof(T));
        }
    }
}
