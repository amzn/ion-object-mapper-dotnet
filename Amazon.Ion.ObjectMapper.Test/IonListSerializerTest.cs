using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class IonListSerializerTest
    {
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
            CheckPermissiveMode(new int[] { 1, 1, 2, 3, 5, 8, 11 }, true, new Teacher(), true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeListToNonListWithPermissiveModeOff()
        {
            CheckPermissiveMode(new int[] { 1, 1, 2, 3, 5, 8, 11 }, false, new Teacher());
        }

        [TestMethod]
        public void DeserializeNonGenericListsToNonListWithPermissiveModeOn()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, true, new Teacher(), true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeNonGenericListsToNonListWithPermissiveModeOff()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, false, new Teacher());
        }

        [TestMethod]
        public void DeserializeArrayToNonArrayWithPermissiveModeOn()
        {
            CheckPermissiveMode(new int[] { 1, 1, 2, 3, 5, 8, 11 }, true, new Teacher(), true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeArrayToNonArrayWithPermissiveModeOff()
        {
            CheckPermissiveMode(new int[] { 1, 1, 2, 3, 5, 8, 11 }, false, new Teacher());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void DeserializeNonGenericListToArrayWithPermissiveModeOff()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, false, new int[] { });
        }

        [TestMethod]
        public void DeserializeNonGenericListToArrayWithPermissiveModeOn()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, true, new int[] {1, 0, 0 });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeserializeNonGenericListToGenericListWithPermissiveModeOff()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, false, new List<int> { });
        }

        [TestMethod]
        public void DeserializeNonGenericListToGenericListWithPermissiveModeOn()
        {
            CheckPermissiveMode(new ArrayList() { 1, "two", 3.14d }, true, new List<int> { 1, 0, 0});
        }
    }
}
