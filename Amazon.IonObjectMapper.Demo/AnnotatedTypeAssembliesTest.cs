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
    using Amazon.IonDotnet.Builders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    public class BaseType {};

    public class DerivedType : BaseType {};

    [TestClass]
    public class AnnotatedTypeAssembliesTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            IIonReader reader;
            BaseType result;

            string ionText = "'Amazon.IonObjectMapper.Test.DerivedType'::{}";
            
            ionSerializer = new IonSerializer();
            reader = IonReaderBuilder.Build(ionText);
            result = ionSerializer.Deserialize<BaseType>(reader);
            Console.WriteLine(result.GetType());
            // Amazon.IonObjectMapper.Test.DerivedType
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { AnnotatedTypeAssemblies = new string[] { "thisAssemblyDoesNotExist" }});
            reader = IonReaderBuilder.Build(ionText);
            result = ionSerializer.Deserialize<BaseType>(reader);
            Console.WriteLine(result.GetType());
            // Amazon.IonObjectMapper.Test.BaseType
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { AnnotatedTypeAssemblies = new string[] { typeof(DerivedType).Assembly.GetName().Name }});
            reader = IonReaderBuilder.Build(ionText);
            result = ionSerializer.Deserialize<BaseType>(reader);
            Console.WriteLine(result.GetType());
            // Amazon.IonObjectMapper.Test.DerivedType

        }
    }
}
