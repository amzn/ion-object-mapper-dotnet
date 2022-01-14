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

namespace Amazon.IonObjectMapper.Demo
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Amazon.IonObjectMapper.Test.Utils;

    public class Club
    {
        [IonClob(encoding = "Unicode")]
        public string name { get; set; } = "homeschool club";

        public string address { get; set; } = "street corner";
    }

    [TestClass]
    public class IonClobTest
    {
        [TestMethod]    
        public void Scratch()
        {
            IonSerializer ionSerializer;
            MemoryStream stream;
            Club result;

            Club club = new Club();

            ionSerializer = new IonSerializer();
            stream = (MemoryStream)ionSerializer.Serialize(club);
            result = ionSerializer.Deserialize<Club>(stream);
            Check(result.name, "homeschool club");
            Check(result.address, "street corner");
        }
    }
}
