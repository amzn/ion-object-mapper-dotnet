using System;
using System.IO;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;

namespace Amazon.Ion.ObjectMapper
{
    public interface IonSerializer<T>
    {
        void Serialize(IIonWriter writer, T item);
        T Deserialize(IIonReader reader);
    }

    public interface IonPropertyNamingConvention
    {

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

    public class IonSerializationOptions
    {
        public IonPropertyNamingConvention NamingConvention;
        public IonSerializationFormat Format;
        public int MaxDepth;
        public bool IncludeFields;
        public bool IgnoreNulls;
        public bool IgnoreReadOnlyFields;
        public bool IgnoreReadOnlyProperties;
        public bool PropertyNameCaseInsensitive;
        public bool IgnoreDefaults;
        public bool IncludeTypeInformation;
        public bool PermissiveMode;
    }

    public interface IonSerializerFactory<T, TContext> where TContext : IonSerializationContext
    {
        public IonSerializer<T> create(IonSerializationOptions options, TContext context);
    }

    public class IonSerializer
    {
        public Stream Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            stream.Position = 0;
            return stream;
        }

        public T Deserialize<T>(Stream stream)
        {
            return (T) (object) null;
        }
    }
}
