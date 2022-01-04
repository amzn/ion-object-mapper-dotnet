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
    
    [IonSerializer(Factory = typeof(UnicornSerializerFactory))]
    public class Unicorn
    {
        public string Status { get; set; } = "Mythical";

        public string Drop { get; set; }
        
        public bool IsHorned { get; } = true;


        public override string ToString()
        {
            return "<Unicorn>{ " + Status + ", " + Drop + ", " + IsHorned + " }";
        }
    }

    public class UnicornSerializerFactory : IonSerializerFactory<Unicorn>
    {
        public override IonSerializer<Unicorn> Create(IonSerializationOptions options, Dictionary<string, object> context)
        {
            return new UnicornSerializer((string)context.GetValueOrDefault("drop", "Alicorn"));
        }
    }

    public class UnicornSerializer : IonSerializer<Unicorn>
    {
        private readonly string drop;
        public UnicornSerializer(string drop)
        {
            this.drop = drop;
        }

        public override Unicorn Deserialize(IIonReader reader)
        {
            return new Unicorn {
                Drop = this.drop
            };
        }

        public override void Serialize(IIonWriter writer, Unicorn item)
        {
            writer.StepIn(IonType.Struct);
            writer.SetFieldName("Status");
            writer.WriteString(item.Status);
            writer.SetFieldName("Drop");
            writer.WriteString(this.drop);
            writer.SetFieldName("IsHorned");
            writer.WriteBool(item.IsHorned);
            writer.StepOut();
        }
    }

    [TestClass]
    public class CustomContextTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            Unicorn result;
            MemoryStream stream;

            Unicorn unicorn = new Unicorn();
            
            ionSerializer = new IonSerializer();            
            stream = (MemoryStream)ionSerializer.Serialize(unicorn);
            // will throw a System.ArgumentNullException error

            ionSerializer = new IonSerializer(new IonSerializationOptions 
            { 
                CustomContext = new Dictionary<string, object>() 
                {
                    {"drop", "Dentine"}
                }
            });            
            stream = (MemoryStream)ionSerializer.Serialize(unicorn);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {  
            //     Status: "Mythical",
            //     Drop: "Dentine",
            //     IsHorned: true
            // }
            result = ionSerializer.Deserialize<Unicorn>(stream);
            Console.WriteLine(result);
            //  <Unicorn>{ Mythical, Dentine, True }

            ionSerializer = new IonSerializer(new IonSerializationOptions 
            { 
                CustomContext = new Dictionary<string, object>() 
                {
                    {"father", "Poseidon"}
                }
            });            
            stream = (MemoryStream)ionSerializer.Serialize(unicorn);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {  
            //     Status: "Mythical",
            //     Drop: "Alicorn",
            //     IsHorned: true
            // }
            result = ionSerializer.Deserialize<Unicorn>(stream);
            Console.WriteLine(result);
            //  <Unicorn>{ Mythical, Alicorn, True }

        }
    }
}
