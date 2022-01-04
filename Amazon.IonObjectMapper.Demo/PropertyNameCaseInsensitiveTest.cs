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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class NormalCaseClass 
    {
        public int? A { get; set; }
        
        public int? B { get; set; }

        [IonField]
        public int C;
    }

    public class AbnormalCaseClass 
    {
        public int? a { get; set; }
        
        public int? B { get; set; }

        [IonField]
        public int c;
    }

    [TestClass]
    public class PropertyNameCaseInsensitiveTest
    {
        [TestMethod]
        public void Scratch()
        {
            NormalCaseClass normalCaseObject = new NormalCaseClass { A = 1, C = 3 };

            AbnormalCaseClass abnormalCaseObject;
            MemoryStream stream;
            IonSerializer ionSerializer;
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = false });
            stream = (MemoryStream)ionSerializer.Serialize(normalCaseObject);

            abnormalCaseObject = ionSerializer.Deserialize<AbnormalCaseClass>(stream);
            Console.WriteLine(abnormalCaseObject.a == null); // could not deserialize normalCaseObject.A into abnormalCaseObject.a
            // True
            Console.WriteLine(abnormalCaseObject.B == null);
            // True
            Console.WriteLine(abnormalCaseObject.c); // PropertyNameCaseInsensitive does not apply to fields
            // 0

            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = true });
            stream = (MemoryStream)ionSerializer.Serialize(normalCaseObject);

            abnormalCaseObject = ionSerializer.Deserialize<AbnormalCaseClass>(stream);
            Console.WriteLine(abnormalCaseObject.a); // successfully deserializes normalCaseObject.A into abnormalCaseObject.a
            // 1
            Console.WriteLine(abnormalCaseObject.B == null);
            // True
            Console.WriteLine(abnormalCaseObject.c); // PropertyNameCaseInsensitive does not apply to fields
            // 0

        }
    }
}
