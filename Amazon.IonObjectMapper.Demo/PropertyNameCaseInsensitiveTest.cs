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
    using Amazon.IonDotnet;
    using Amazon.IonDotnet.Builders;
    using Amazon.IonObjectMapper.Test;

    public class NormalCaseClass 
    {
        public int? ironicCase { get; set; }
    }

    [TestClass]
    public class PropertyNameCaseInsensitiveTest
    {
        [TestMethod]
        public void Scratch()
        {
            string ionText = "{iRoNiCcAsE: 3}";

            MemoryStream stream;
            IonSerializer ionSerializer;
            IIonReader reader;
            NormalCaseClass result;
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = false });            
            reader = IonReaderBuilder.Build(ionText);
            result = ionSerializer.Deserialize<NormalCaseClass>(reader);
            Console.WriteLine(result.ironicCase == null); // could not deserialize differently-cased text
            // True

            ionSerializer = new IonSerializer(new IonSerializationOptions { PropertyNameCaseInsensitive = true });            
            reader = IonReaderBuilder.Build(ionText);
            result = ionSerializer.Deserialize<NormalCaseClass>(reader);
            Console.WriteLine(result.ironicCase); // successfully deserializes differently-cased text
            // 3
        }
    }
}
