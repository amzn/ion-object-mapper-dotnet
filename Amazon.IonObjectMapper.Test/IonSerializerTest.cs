﻿using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IonDotnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.IonObjectMapper.Test.Utils;

namespace Amazon.IonObjectMapper.Test
{
    [TestClass]
    public class IonSerializerTest
    {
        [TestMethod]
        public void SerializesAndDeserializesPrimitives()
        {
            Check((object) null);
            Check(false);
            Check(true);
            Check(2010); // int
            Check(20102011L); // long
            Check(3.14159f); // float
            Check(6.02214076e23d); // double
            Check(567.9876543m); // decimal
            Check(BigDecimal.Parse("2.71828"));
            Check(DateTime.Parse("2009-10-10T13:15:21Z")); 
            Check("Civic"); 
            Check(new SymbolToken("my symbol", SymbolToken.UnknownSid)); 
            Check(Encoding.UTF8.GetBytes("This is an Ion blob")); // blob
            Check(MakeIonClob("This is an Ion clob"), "This is an Ion clob"); // clob
            Check(Guid.NewGuid()); // guid
            Check(Guid.NewGuid(), new IonSerializationOptions { AnnotateGuids = true }); // guid
        }

        [TestMethod]
        public void SerializesAndDeserializesLists()
        {
            Check(new int[] { 1, 1, 2, 3, 5, 8, 11 });
        }
    }
}
