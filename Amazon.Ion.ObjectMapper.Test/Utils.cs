using System.IO;
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

        public static string PrettyPrint(Stream stream) {
            return IonLoader.Default.Load(Copy(stream)).ToPrettyString();
        }
        public static T Serde<T>(T item) 
        {
            var stream = new IonSerializer().Serialize(item);
            return new IonSerializer().Deserialize<T>(stream);
        }

        public static void Check<T>(T item) => Assert.AreEqual(item.ToString(), Serde(item).ToString());
    }
}