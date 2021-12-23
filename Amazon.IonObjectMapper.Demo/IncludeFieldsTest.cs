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

    public class Foo 
    {
        [IonField]
        private int Bar;
        
        public int Gee { get; set; }

        public Foo(int Bar) 
        {
            this.Bar = Bar;
        }

        public override string ToString()
        {
            return "<Foo>{ Bar: " + Bar + ", Gee: " + Gee + " }";
        }
    }

    [TestClass]
    public class IncludeFieldsTest
    {
        [TestMethod]
        public void Scratch()
        {
            Foo foo = new Foo(1);
            foo.Gee = 2;

            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IncludeFields = false }).Serialize(foo);
            Console.WriteLine(Utils.PrettyPrint(stream));
            //  {
            //    gee: 2
            //  }
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { IncludeFields = true }).Serialize(foo);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     gee: 2,
            //     Bar: 1,
            //     '<Gee>k__BackingField': 2
            // }
        }
    }
}
