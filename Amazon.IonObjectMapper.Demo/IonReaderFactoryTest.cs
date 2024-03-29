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
    using System.Text;
    using Amazon.IonDotnet;
    using Amazon.IonDotnet.Builders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper.Test;

    public class UnicodeIonReaderFactory : IIonReaderFactory
    {
        public IIonReader Create(Stream stream)
        {
            return IonReaderBuilder.Build(stream, new ReaderOptions { Format = ReaderFormat.Detect, Encoding = Encoding.Unicode });
        }
    }

    [TestClass]
    public class IonReaderFactoryTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            MemoryStream stream;
            Supra result;

            
            ionSerializer = new IonSerializer(new IonSerializationOptions {ReaderFactory = new UnicodeIonReaderFactory()});
            stream =(MemoryStream)ionSerializer.Serialize(TestObjects.a90);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // 'Amazon.IonObjectMapper.Test.Supra'::{
            //     brand: 'OEM.Manufacturer'::"Toyota"
            // }
            result = ionSerializer.Deserialize<Supra>(stream);
            Console.WriteLine(result);
            // <Supra>{ Toyota }

        }
    }
}
