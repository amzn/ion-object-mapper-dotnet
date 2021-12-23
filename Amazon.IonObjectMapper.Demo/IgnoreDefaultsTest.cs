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
    using Amazon.IonDotnet.Tree;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class IgnoreDefaultsClass 
    {
        public int? A { get; set; }
        
        public int? B { get; set; }
    }

    [TestClass]
    public class IgnoreDefaultsTest
    {
        [TestMethod]
        public void Scratch()
        {
            IgnoreDefaultsClass ignoreDefaultsObject = new IgnoreDefaultsClass { A = 1 };

            MemoryStream stream;
            IonSerializer ionSerializer;
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { IgnoreDefaults = false });
            stream = (MemoryStream)ionSerializer.Serialize(ignoreDefaultsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     b: null
            // }

            ignoreDefaultsObject = ionSerializer.Deserialize<IgnoreDefaultsClass>(stream);
            Console.WriteLine(ignoreDefaultsObject.A);
            // 1
            Console.WriteLine(ignoreDefaultsObject.B == null);
            // True

            ionSerializer = new IonSerializer(new IonSerializationOptions { IgnoreDefaults = true });
            stream = (MemoryStream)ionSerializer.Serialize(ignoreDefaultsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1
            // }

            ignoreDefaultsObject = ionSerializer.Deserialize<IgnoreDefaultsClass>(stream);
            Console.WriteLine(ignoreDefaultsObject.A);
            // 1
            Console.WriteLine(ignoreDefaultsObject.B == null);
            // True

        }
    }
}
