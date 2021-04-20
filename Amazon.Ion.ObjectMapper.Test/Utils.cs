using System.IO;
using System.Text;
using Amazon.IonDotnet.Builders;
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
            var stream = new IonSerializer().Serialize(item);
            return new IonSerializer().Deserialize<T>(stream);
        }

        public static void Check<T>(T item)
        {
            if (item == null) 
            {
                Assert.AreEqual(null, Serde((object) null));
                return;
            }
            Assert.AreEqual(item.ToString(), Serde(item).ToString());
        }
        
        public static void Check<T>(Stream actual, T expected)
        {
            Assert.AreEqual(expected.ToString(), new IonSerializer().Deserialize<string>(actual));
        }
    }
}