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
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    [IonAnnotateType]
    public class TypeAnnotationNameClass 
    {
        public int A { get; set; } = 1;
        
        public int B { get; } = 2;
    }

    [IonAnnotateType]
    public class CustomTypeAnnotationName : ITypeAnnotationName
    {
        public string Apply(Type type)
        {
            return "custom";
        }
    }

    [TestClass]
    public class TypeAnnotationNameTest
    {
        [TestMethod]
        public void Scratch()
        {
            TypeAnnotationNameClass typeAnnotationNameClass = new TypeAnnotationNameClass();

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { TypeAnnotationName = new ClassNameTypeAnnotationName() }).Serialize(typeAnnotationNameClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // 'Amazon.IonObjectMapper.Test.TypeAnnotationNameClass'::{
            //     a: 1,
            //     b: 2
            // }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { TypeAnnotationName = new CustomTypeAnnotationName() }).Serialize(typeAnnotationNameClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // 'Amazon.IonObjectMapper.Test.custom'::{
            //     a: 1,
            //     b: 2
            // }

        }
    }
}
