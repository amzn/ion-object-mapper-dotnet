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
    using Amazon.IonObjectMapper.Test;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class IgnoreReadOnlyFieldsClass 
    {
        [IonField]
        public int A = 1;
        
        [IonField]
        public readonly int B = 2;
    }

    [TestClass]
    public class IgnoreReadOnlyFieldsTest
    {
        [TestMethod]
        public void Scratch()
        {
            IgnoreReadOnlyFieldsClass IgnoreReadOnlyFieldsObject = new IgnoreReadOnlyFieldsClass();

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IgnoreReadOnlyFields = false }).Serialize(IgnoreReadOnlyFieldsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     A: 1,
            //     B: 2
            // }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IgnoreReadOnlyFields = true }).Serialize(IgnoreReadOnlyFieldsObject);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     A: 1
            // }
        }
    }
}
