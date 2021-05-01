using System.IO;
using System.Linq;
using System.Text;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Amazon.Ion.ObjectMapper.Test
{
    public static class Utils
    {
        public static Stream Copy(Stream source) {
            var copy = new MemoryStream();
            source.CopyTo(copy);
            source.Position = 0;
            copy.Position = 0;
            return copy;
        }

        public static Stream MakeIonClob(string clob)
        {
            var stream = new MemoryStream();
            var writer = IonTextWriterBuilder.Build(new StreamWriter(stream));
            writer.WriteClob(Encoding.ASCII.GetBytes(clob));
            writer.Flush();
            writer.Finish();
            stream.Position = 0;
            return stream;
        }

        public static string PrettyPrint(Stream stream) {
            return IonLoader.Default.Load(Copy(stream)).ToPrettyString();
        }
        public static T Serde<T>(T item) 
        {
            return Serde(new IonSerializer(), item);
        }

        public static T Serde<T>(IonSerializer ionSerializer, T item) 
        {
            var stream = ionSerializer.Serialize(item);
            return ionSerializer.Deserialize<T>(stream);
        }

        public static IIonValue ToIonValue<T>(IonSerializer ionSerializer, T item) 
        {
            var stream = ionSerializer.Serialize(item);
            return IonLoader.Default.Load(stream).GetElementAt(0);
        }

        public static void Check<T>(T item)
        {
            Check(new IonSerializer(), item);
        }
        
        public static void Check<T>(T item, IonSerializationOptions options)
        {
            Check(new IonSerializer(options), item);
        }

        public static void Check<T>(IonSerializer ionSerializer, T item)
        {
            if (item == null) 
            {
                Assert.AreEqual(null, Serde(ionSerializer, (object) null));
                return;
            }
            Assert.AreEqual(item.ToString(), Serde(ionSerializer, item).ToString());
        }

        public static void AssertHasAnnotation(string annotation, Stream stream)
        {
            AssertHasAnnotation(annotation, IonLoader.Default.Load(Copy(stream)).GetElementAt(0));
        }

        public static void AssertHasAnnotation(string annotation, IIonValue ionValue)
        {
            var annotations = string.Join(",", ionValue.GetTypeAnnotationSymbols().Select(a => a.Text));
            Assert.IsTrue(ionValue.HasAnnotation(annotation), "seeking " + annotation + " in [" + annotations + "]");
        }

        public static void AssertHasNoAnnotations(Stream stream)
        {
            var count = IonLoader.Default.Load(Copy(stream)).GetElementAt(0).GetTypeAnnotationSymbols().Count;
            Assert.IsTrue(count == 0, "Has " + count + " annotations");
        }

        private static bool SerializesField<T>(T item, IonSerializer serializer, string fieldName)
        {
            var stream = serializer.Serialize(item);

            IIonReader reader = IonReaderBuilder.Build(stream, new ReaderOptions {Format = ReaderFormat.Detect});
            reader.MoveNext();
            reader.StepIn();
            
            while (reader.MoveNext() != IonType.None)
            {
                if (fieldName == reader.CurrentFieldName)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static void AssertSerializesField<T>(T item, IonSerializer serializer, string fieldName)
        {
            Assert.IsTrue(SerializesField(item, serializer, fieldName));
        }
        
        public static void AssertDoesNotSerializeField<T>(T item, IonSerializer serializer, string fieldName)
        {
            Assert.IsFalse(SerializesField(item, serializer, fieldName));
        }
        
        public static void Check<T>(Stream actual, T expected)
        {
            Assert.AreEqual(expected.ToString(), new IonSerializer().Deserialize<string>(actual));
        }
    }
}
