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
    using System.Collections.Generic;
    using Amazon.IonDotnet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper;
    using Amazon.IonObjectMapper.Test;

    public class BracketStringIonSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            string result = reader.StringValue();
            return result.Substring(1, result.Length-2);
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString($"<{item}>");
        }
    }

    [TestClass]
    public class IonSerializersTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            List<object> result;
            MemoryStream stream;

            List<object> testList = new List<object>(new object[] { "test", 5 });
            
            ionSerializer = new IonSerializer();            
            stream = (MemoryStream)ionSerializer.Serialize(testList);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // [
            //     "test",
            //     5
            // ]
            result = ionSerializer.Deserialize<List<object>>(stream);
            Console.WriteLine(result[0]);
            // test
            Console.WriteLine(result[1]);
            // 5
            
            ionSerializer = new IonSerializer(new IonSerializationOptions 
            { 
                IonSerializers = new Dictionary<Type, IIonSerializer>() 
                { 
                    {typeof(string), new BracketStringIonSerializer()}
                }
            });            
            stream = (MemoryStream)ionSerializer.Serialize(testList);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // [
            //     "<test>",
            //     5
            // ]
            result = ionSerializer.Deserialize<List<object>>(stream);
            Console.WriteLine(result[0]);
            // test
            Console.WriteLine(result[1]);
            // 5

        }
    }
}
