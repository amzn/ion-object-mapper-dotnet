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

    public class PluralCaseClass 
    {
        public int? A { get; set; }

        public int? a { get; set; }
        
        public int? B { get; set; }

        public int? b { get; set; }

        [IonField]
        public int C;

        [IonField]
        public int c;
    }

    [TestClass]
    public class CaseInsensitiveBug
    {
        [TestMethod]
        public void Scratch()
        {
            PluralCaseClass pluralCaseObject = new PluralCaseClass { A = 1, b = 2, C = 3 };

            MemoryStream stream;
            IonSerializer ionSerializer;
            PluralCaseClass result;
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = false });
            stream = (MemoryStream)ionSerializer.Serialize(pluralCaseObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     a: null,
            //     b: null,
            //     b: 2,
            //     C: 3,
            //     c: 0
            // }

            result = ionSerializer.Deserialize<PluralCaseClass>(stream);
            Console.WriteLine(result.A);
            Console.WriteLine(result.a);
            Console.WriteLine(result.B);
            Console.WriteLine(result.b);
            Console.WriteLine(result.C);
            Console.WriteLine(result.c);
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = true });
            stream = (MemoryStream)ionSerializer.Serialize(pluralCaseObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     a: null,
            //     b: null,
            //     b: 2,
            //     C: 3,
            //     c: 0
            // }

            result = ionSerializer.Deserialize<PluralCaseClass>(stream);
            Console.WriteLine(result.A);
            Console.WriteLine(result.a);
            Console.WriteLine(result.B);
            Console.WriteLine(result.b);
            Console.WriteLine(result.C);
            Console.WriteLine(result.c);

        }
    }
}
