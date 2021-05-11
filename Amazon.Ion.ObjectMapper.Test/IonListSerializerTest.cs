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
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Utils;

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
