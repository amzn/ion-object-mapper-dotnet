using System;
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

    public readonly struct IonSerializationOptions
    {
        public static IonSerializationOptions DEFAULT = new IonSerializationOptions
        {
            NamingConvention = new CamelCaseNamingConvention()
        };

        public readonly IonPropertyNamingConvention NamingConvention { get; init; }
        public readonly IonSerializationFormat Format;
        public readonly int MaxDepth;
        public readonly bool IncludeFields;
        public readonly bool IgnoreNulls;
        public readonly bool IgnoreReadOnlyFields;
        public readonly bool IgnoreReadOnlyProperties;
        public readonly bool PropertyNameCaseInsensitive;
        public readonly bool IgnoreDefaults;
        public readonly bool IncludeTypeInformation;
        public readonly bool PermissiveMode;
    }

    public interface IonSerializerFactory<T, TContext> where TContext : IonSerializationContext
    {
        public IonSerializer<T> create(IonSerializationOptions options, TContext context);
    }

    public class IonSerializer
    {
        private readonly IonSerializationOptions options; 

        public IonSerializer() : this(IonSerializationOptions.DEFAULT) 
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
                new IonNullSerializer().Serialize(writer, null);
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
