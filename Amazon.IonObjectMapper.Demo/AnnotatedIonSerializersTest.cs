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
    
    public class Trilobyte
    {
        [IonAnnotateType(Name = "Cambrian", Prefix = "Paleozoic")]
        public string Group { get; set; } = "Arthropods";

        public bool IsVertebrate { get; } = false;

        public override string ToString()
        {
            return "<Trilobyte>{ " + Group + ", " + IsVertebrate + " }";
        }
    }

    public class CambrianIonSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            return "Springgina";
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString("Springgina");
        }
    }

    [TestClass]
    public class AnnotatedIonSerializersTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            Trilobyte result;
            MemoryStream stream;

            Trilobyte trilobyte = new Trilobyte();
            
            ionSerializer = new IonSerializer();            
            stream = (MemoryStream)ionSerializer.Serialize(trilobyte);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {  
            //     group: 'Paleozoic.Cambrian'::"Arthropods",
            //     isVertebrate: false
            // },

            result = ionSerializer.Deserialize<Trilobyte>(stream);
            Console.WriteLine(result);
            //  <Trilobyte>{ Arthropods, False }
            
            ionSerializer = new IonSerializer(new IonSerializationOptions 
            { 
                AnnotatedIonSerializers = new Dictionary<string, IIonSerializer>() 
                {
                    {"Paleozoic.Cambrian", new CambrianIonSerializer()}
                }
            });            
            stream = (MemoryStream)ionSerializer.Serialize(trilobyte);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {  
            //     group: 'Paleozoic.Cambrian'::"Springgina",
            //     isVertebrate: false
            // },
            result = ionSerializer.Deserialize<Trilobyte>(stream);
            Console.WriteLine(result);
            //  <Trilobyte>{ Springgina, False }

        }
    }
}
