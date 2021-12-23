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
    using Amazon.IonDotnet;
    using Amazon.IonDotnet.Tree;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    public class AnnotateGuidsClass 
    {
        public int A { get; set; } = 1;
        
        public Guid B { get; } = Guid.NewGuid();
    }

    [TestClass]
    public class AnnotateGuidsTest
    {
        [TestMethod]
        public void Scratch()
        {
            AnnotateGuidsClass annotateGuidsClass = new AnnotateGuidsClass();

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { AnnotateGuids = true }).Serialize(annotateGuidsClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     b: guid128::{{ lkjNEjlW0kmrb2M2lA+jGg== }}
            // }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { AnnotateGuids = false }).Serialize(annotateGuidsClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     b: {{ lkjNEjlW0kmrb2M2lA+jGg== }}
            // }

        }
    }
}
