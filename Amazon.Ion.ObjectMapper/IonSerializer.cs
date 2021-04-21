using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;

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

    public interface TypeAnnotationPrefix
    {
        public string Apply(Type type);
    }

    public class NamespaceTypeAnnotationPrefix : TypeAnnotationPrefix
    {
        public string Apply(Type type)
        {
            return type.Namespace;
        }
    }

    public class FixedTypeAnnotationPrefix : TypeAnnotationPrefix
    {
        private readonly string prefix;

        public FixedTypeAnnotationPrefix(string prefix)
        {
            this.prefix = prefix;
        }

        public string Apply(Type type)
        {
            return prefix;
        }
    }

    public interface TypeAnnotator
    {
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type);
    }

    public class DefaultTypeAnnotator : TypeAnnotator
    {
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type)
        {
            IonAnnotateType annotateType = ShouldAnnotate(type, type);
            if (annotateType == null && options.IncludeTypeInformation) 
            {
                // the serializer insists on type information being included
                annotateType = new IonAnnotateType();
            }

            if (annotateType != null)
            {
                var prefix = annotateType.Prefix != null ? annotateType.Prefix : options.TypeAnnotationPrefix.Apply(type);
                var name = annotateType.Name != null ? annotateType.Name : type.Name;
                writer.AddTypeAnnotation(prefix + "." + name);
            }
        }

        private IonAnnotateType ShouldAnnotate(Type targetType, Type currentType) 
        {
            if (currentType == null)
            {
                // We did not find an annotation
                return null;
            }
            var doNotAnnotateAttributes = currentType.GetCustomAttributes(typeof(IonDoNotAnnotateType), false);
            if (doNotAnnotateAttributes.Length > 0) 
            {
                if (((IonDoNotAnnotateType)doNotAnnotateAttributes[0]).ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return ShouldAnnotate(targetType, currentType.BaseType);
                }
                // do not annotate this type
                return null;
            }
            var annotateAttributes = currentType.GetCustomAttributes(typeof(IonAnnotateType), false);
            if (annotateAttributes.Length > 0) 
            {
                var attribute = (IonAnnotateType)annotateAttributes[0];
                if (attribute.ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return ShouldAnnotate(targetType, currentType.BaseType);
                }
                // annotate this type
                return attribute;
            }
            // did not find any annotations on this type
            return ShouldAnnotate(targetType, currentType.BaseType);
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
        public readonly IonSerializationFormat Format;
        public readonly int MaxDepth;
        public bool AnnotateGuids { get; init; } = false;
        public readonly bool IncludeFields;
        public readonly bool IgnoreNulls;
        public readonly bool IgnoreReadOnlyFields;
        public readonly bool IgnoreReadOnlyProperties;
        public readonly bool PropertyNameCaseInsensitive;
        public readonly bool IgnoreDefaults;
        public bool IncludeTypeInformation { get; init; } = false;
        public TypeAnnotationPrefix TypeAnnotationPrefix { get; init; } = new NamespaceTypeAnnotationPrefix();
        public TypeAnnotator TypeAnnotator { get; init; } = new DefaultTypeAnnotator();
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
            var sw = new StreamWriter(stream);
            var writer = IonTextWriterBuilder.Build(sw);
            Serialize(writer, item);
            writer.Finish();
            sw.Flush();
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
            return Deserialize<T>(IonReaderBuilder.Build(stream, new ReaderOptions { Format=ReaderFormat.Detect }));
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
