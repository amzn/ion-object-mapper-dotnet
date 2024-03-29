﻿/*
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

    [TestClass]
    public class FormatTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            MemoryStream stream;

            ionSerializer = new IonSerializer(new IonSerializationOptions {Format = IonSerializationFormat.BINARY});
            stream = (MemoryStream)ionSerializer.Serialize(TestObjects.a90);
            Console.WriteLine(BitConverter.ToString(stream.ToArray())); 
            // E0-01-00-EA-EE-B0...
            
            ionSerializer = new IonSerializer(new IonSerializationOptions {Format = IonSerializationFormat.TEXT});
            stream = (MemoryStream)ionSerializer.Serialize(TestObjects.a90);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
            // 'Amazon.IonObjectMapper.Test.Supra'::{brand:'OEM.Manufacturer'::"Toyota"}
            
            ionSerializer = new IonSerializer(new IonSerializationOptions {Format = IonSerializationFormat.PRETTY_TEXT});
            stream = (MemoryStream)ionSerializer.Serialize(TestObjects.a90);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
            // 'Amazon.IonObjectMapper.Test.Supra'::{
            //     brand: 'OEM.Manufacturer'::"Toyota"
            // }
        }
    }
}
