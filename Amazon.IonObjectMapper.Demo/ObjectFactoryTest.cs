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
    using Amazon.IonDotnet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    public class ObjectFactoryClass 
    {
        public int A { get; set; } = 1;
        
        public int B { get; } = 2;
    }

    public class CustomClass : ObjectFactoryClass
    {
        public int C { get; set; } = 3;
    }

    public class CustomObjectFactory : IObjectFactory
    {
        public object Create(IonSerializationOptions options, IIonReader reader, Type targetType)
        {
            return Activator.CreateInstance(typeof(CustomClass));
        }
    }

    [TestClass]
    public class ObjectFactoryTest
    {
        [TestMethod]
        public void Scratch()
        {
            ObjectFactoryClass objectFactoryClass = new ObjectFactoryClass();

            IonSerializer ionSerializer;
            object result;
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { ObjectFactory = new DefaultObjectFactory() });
            result = Utils.Serde(ionSerializer, objectFactoryClass);
            Console.WriteLine(result.GetType());
            // Amazon.IonObjectMapper.Test.ObjectFactoryClass
            
            ionSerializer = new IonSerializer(new IonSerializationOptions { ObjectFactory = new CustomObjectFactory() });
            result = Utils.Serde(ionSerializer, objectFactoryClass);
            Console.WriteLine(result.GetType());
            // Amazon.IonObjectMapper.Test.CustomClass

        }
    }
}
