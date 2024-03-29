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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    [IonAnnotateType]
    public class AnnotationConventionClass 
    {
        public int A { get; set; } = 1;
        
        public int B { get; } = 2;
    }

    public class BracketAnnotationConvention : IAnnotationConvention
    {
        public string Apply(IonSerializationOptions options, IonAnnotateTypeAttribute annotateType, Type type)
        {
            annotateType.Prefix ??= options.TypeAnnotationPrefix.Apply(type);
            annotateType.Name ??= options.TypeAnnotationName.Apply(type);
            return "<" + annotateType.Prefix + "." + annotateType.Name + ">";
        }
    }

    [TestClass]
    public class AnnotationConventionTest
    {
        [TestMethod]
        public void Scratch()
        {
            AnnotationConventionClass annotationConventionClass = new AnnotationConventionClass();

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { AnnotationConvention = new DefaultAnnotationConvention() }).Serialize(annotationConventionClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // 'Amazon.IonObjectMapper.Test.AnnotationConventionClass'::{
            //     a: 1,
            //     b: 2
            // }

            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { AnnotationConvention = new BracketAnnotationConvention() }).Serialize(annotationConventionClass);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // '<Amazon.IonObjectMapper.Test.AnnotationConventionClass>'::{
            //     a: 1,
            //     b: 2
            // }

        }
    }
}
