using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.IonObjectMapper.Test.Utils;

namespace Amazon.IonObjectMapper.Test
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
    }
}
