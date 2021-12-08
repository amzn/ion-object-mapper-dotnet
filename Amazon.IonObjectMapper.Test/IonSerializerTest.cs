/*
 * Copyright (c) Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

namespace Amazon.IonObjectMapper.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Amazon.IonDotnet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Amazon.IonObjectMapper.Test.Utils;

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

        [TestMethod]
        public void SerializesAndDeserializesDictionaries()
        {
            var dictionary = new TestDictionary
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };
            Assert.AreEqual(TestDictionary.PrettyString(dictionary),
                TestDictionary.PrettyString(Serde<Dictionary<string, int>>(dictionary)));
        }

        [TestMethod]
        public void AnnotatedIonSerializerSerialization()
        {
            var annotatedIonSerializer = new Dictionary<string, IIonSerializer>();
            annotatedIonSerializer.Add("OEM.Manufacturer", new SupraManufacturerSerializer());

            var customizedSerializer = new IonSerializer(new IonSerializationOptions { AnnotatedIonSerializers = annotatedIonSerializer });

            Assert.AreEqual("BMW", Serde(customizedSerializer, TestObjects.a90).Brand);
        }

        [TestMethod]
        public void AnnotatedIonSerializerDeserialization()
        {
            var annotatedIonDeserializer = new Dictionary<string, IIonSerializer>();
            annotatedIonDeserializer.Add("OEM.Manufacturer", new SupraManufacturerDeserializer());

            var customizedDeserializer = new IonSerializer(new IonSerializationOptions { AnnotatedIonSerializers = annotatedIonDeserializer });

            Assert.AreEqual("BMW", Serde(customizedDeserializer, TestObjects.a90).Brand);
        }
    }
}
