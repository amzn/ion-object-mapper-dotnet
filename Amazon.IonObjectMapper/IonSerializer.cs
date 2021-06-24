using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using static Amazon.IonObjectMapper.IonSerializationFormat;

namespace Amazon.IonObjectMapper
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
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        typeToCreate = assembly.GetType(assembly.GetName().Name + "." + typeName);
                        if (typeToCreate != null)
                        {
                            break;
                        }
                    }
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
        public Dictionary<string, object> CustomContext { get; init; }
    }

    /// <summary>
    /// This interface is to create a custom IonSerializer to be used to serialize
    /// and deserialize instances of the class annotated with Factory IonSerializerAttribute.
    /// </summary>
    public interface IIonSerializerFactory
    {
        /// <summary>
        /// Create custom IonSerializer with customContext option.
        /// </summary>
        /// <param name="options">
        /// The IonSerializationOptions is an object that can be passed to the IonSerializer object
        /// and determined the way to customize the IonSerializer.
        /// </param>
        /// <param name="customContext">
        /// The Dictionary<string, object> customContext is one option to create IonSerializer with custom arbitrary data.
        /// A Dictionary of Key Type string is to map to any customized objects
        /// and Value Type object is to custom any serialize/deserialize logic.
        /// </param>
        /// <returns>
        /// Customized IonSerializer.
        /// </returns>
        IIonSerializer Create(IonSerializationOptions options, Dictionary<string, object> customContext);
    }

    /// <summary>
    /// This abstract class is to implement IIonSerializerFactory interface.
    /// </summary>
    public abstract class IonSerializerFactory<T> : IIonSerializerFactory
    {
        /// <summary>
        /// Create custom IonSerializer with customContext option.
        /// </summary>
        /// <param name="options">
        /// The IonSerializationOptions is an object that can be passed to the IonSerializer object
        /// and determined the way to customize the IonSerializer.
        /// </param>
        /// <param name="customContext">
        /// The Dictionary<string, object> customContext is one option to create IonSerializer with custom arbitrary data.
        /// A Dictionary of Key Type string is to map to any customized objects
        /// and Value Type object is to custom any serialize/deserialize logic.
        /// </param>
        /// <returns>
        /// Customized IonSerializer.
        /// </returns>
        public abstract IonSerializer<T> Create(IonSerializationOptions options, Dictionary<string, object> customContext);

        IIonSerializer IIonSerializerFactory.Create(IonSerializationOptions options, Dictionary<string, object> customContext)
        {
            return Create(options, customContext);
        }
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
                if (customSerializerAttribute != null) {
                    var customSerializer = CreateCustomSerializer(item.GetType());
                    customSerializer.Serialize(writer, item);
                    return;
                }

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

            if (type != null) 
            {
                var customSerializerAttribute = type.GetCustomAttribute<IonSerializerAttribute>();
                if (customSerializerAttribute != null) {
                    var customSerializer = CreateCustomSerializer(type);
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

        private IIonSerializer CreateCustomSerializer (Type targetType)
        {
            var customSerializerAttribute = targetType.GetCustomAttribute<IonSerializerAttribute>();
            if (customSerializerAttribute.Factory != null) 
            {
                return ((IIonSerializerFactory)Activator.CreateInstance(customSerializerAttribute.Factory)).Create(options, options.CustomContext);
            } 
            else if (customSerializerAttribute.Serializer != null) 
            {
                return (IIonSerializer)Activator.CreateInstance(customSerializerAttribute.Serializer);
            } 

            throw new InvalidOperationException($"[IonSerializer] annotated type {targetType} should have a valid IonSerializerAttribute Factory or Serializer");
        }
    }
}
