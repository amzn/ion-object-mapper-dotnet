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
    using System;
    using System.IO;
    using Amazon.IonObjectMapper.Test;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class IgnoreNullsClass 
    {
        public int? A { get; set; } = null;
        
        public int? B { get; set; } = null;
    }

    [TestClass]
    public class IgnoreNullsTest
    {
        [TestMethod]
        public void Scratch()
        {
            IgnoreNullsClass ignoreNullsObject = new IgnoreNullsClass { A = 1 };

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IgnoreNulls = false }).Serialize(ignoreNullsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     A: 1,
            //     B: null
            // }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IgnoreNulls = true }).Serialize(ignoreNullsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     A: 1
            // }
        }
    }
}
