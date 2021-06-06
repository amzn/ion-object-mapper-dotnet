using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class IonListSerializerTest
    {
        IonSerializer serializerWithPermissiveModeOn = new IonSerializer(new IonSerializationOptions { PermissiveMode = true });
        IonSerializer serializerWithPermissiveModeOff = new IonSerializer(new IonSerializationOptions { PermissiveMode = false });

        [TestMethod]
        public void SerializesAndDeserializesLists()
        {
            Check(new List<int>() { 1, 1, 2, 3, 5, 8, 13 });
        }

        [TestMethod]
        public void SerializesAndDeserializesNonGenericLists()
        {
            Check(new ArrayList() { 1, "two", 3.14d });
        }

        [TestMethod]
        public void SerializesAndDeserializesArrays()
        {
            Check(new int[] { 1, 1, 2, 3, 5, 8, 11 });
        }

        [TestMethod]
        public void DeserializeListToNonListWithPermissiveModeOn()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new int[] { 1, 1, 2, 3, 5, 8, 11 });

            var deserialized = serializerWithPermissiveModeOn.Deserialize<Teacher>(stream);
            Assert.IsNull(deserialized);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeListToNonListWithPermissiveModeOff()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new int[] { 1, 1, 2, 3, 5, 8, 11 });

            serializerWithPermissiveModeOff.Deserialize<Teacher>(stream);
        }

        [TestMethod]
        public void DeserializeNonGenericListsToNonListWithPermissiveModeOn()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            var deserialized = serializerWithPermissiveModeOn.Deserialize<Teacher>(stream);
            Assert.IsNull(deserialized);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeNonGenericListsToNonListWithPermissiveModeOff()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            serializerWithPermissiveModeOff.Deserialize<Teacher>(stream);
        }

        [TestMethod]
        public void DeserializeArrayToNonArrayWithPermissiveModeOn()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new int[] { 1, 1, 2, 3, 5, 8, 11 });

            var deserialized = serializerWithPermissiveModeOn.Deserialize<Teacher>(stream);
            Assert.IsNull(deserialized);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeArrayToNonArrayWithPermissiveModeOff()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new int[] { 1, 1, 2, 3, 5, 8, 11 });

            serializerWithPermissiveModeOff.Deserialize<Teacher>(stream);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void DeserializeNonGenericListToArrayWithPermissiveModeOff()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            serializerWithPermissiveModeOff.Deserialize<int[]>(stream);
        }

        [TestMethod]
        public void DeserializeNonGenericListToArrayWithPermissiveModeOn()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            var deserialized = serializerWithPermissiveModeOn.Deserialize<int[]>(stream);
            Assert.AreEqual(new int[] { 1, 0, 0 }.ToString(), deserialized.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeserializeNonGenericListToGenericListWithPermissiveModeOff()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            serializerWithPermissiveModeOff.Deserialize<List<int>>(stream);
        }

        [TestMethod]
        public void DeserializeNonGenericListToGenericListWithPermissiveModeOn()
        {
            var stream = serializerWithPermissiveModeOff.Serialize(new ArrayList() { 1, "two", 3.14d });

            var deserialized = serializerWithPermissiveModeOn.Deserialize<List<int>>(stream);
            Assert.AreEqual(new List<int>() { 1, 0, 0 }.ToString(), deserialized.ToString());
        }
    }
}
