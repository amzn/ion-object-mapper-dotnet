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

    public class TypeAnnotatorClass 
    {
        public int A { get; set; } = 1;
        
        public int B { get; } = 2;
    }
    
    public class CustomTypeAnnotator : ITypeAnnotator
    {
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type)
        {
            writer.AddTypeAnnotation("custom");
        }
    }

    [TestClass]
    public class TypeAnnotatorTest
    {
        [TestMethod]
        public void Scratch()
        {
            TypeAnnotatorClass typeAnnotatorClass = new TypeAnnotatorClass();

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { TypeAnnotator = new DefaultTypeAnnotator() }).Serialize(typeAnnotatorClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     a: 1,
            //     b: 2
            // }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { TypeAnnotator = new CustomTypeAnnotator() }).Serialize(typeAnnotatorClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // custom::{
            //     a: 1,
            //     b: 2
            // }

        }
    }
}
