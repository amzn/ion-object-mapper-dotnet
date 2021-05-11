/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace Amazon.Ion.ObjectMapper.Test
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using Amazon.IonDotnet.Builders;
    using Amazon.IonDotnet.Tree;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class Utils
    {
        public static Stream Copy(Stream source)
        {
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

        public static string PrettyPrint(Stream stream)
        {
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
            return StreamToIonValue(stream);
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
                Assert.AreEqual(null, Serde(ionSerializer, (object)null));
                return;
            }

            Assert.AreEqual(item.ToString(), Serde(ionSerializer, item).ToString());
        }

        public static void AssertHasAnnotation(string annotation, Stream stream)
        {
            AssertHasAnnotation(annotation, StreamToIonValue(Copy(stream)));
        }

        public static void AssertHasAnnotation(string annotation, IIonValue ionValue)
        {
            var annotations = string.Join(",", ionValue.GetTypeAnnotationSymbols().Select(a => a.Text));
            Assert.IsTrue(ionValue.HasAnnotation(annotation), $"seeking {annotation} in [{annotations}]");
        }

        public static void AssertHasNoAnnotations(Stream stream)
        {
            var count = StreamToIonValue(Copy(stream)).GetTypeAnnotationSymbols().Count;
            Assert.IsTrue(count == 0, $"Has {count} annotations");
        }

        public static void Check<T>(Stream actual, T expected)
        {
            Assert.AreEqual(expected.ToString(), new IonSerializer().Deserialize<string>(actual));
        }

        public static IIonValue StreamToIonValue(Stream stream)
        {
            return IonLoader.Default.Load(stream).GetElementAt(0);
        }
    }
}
