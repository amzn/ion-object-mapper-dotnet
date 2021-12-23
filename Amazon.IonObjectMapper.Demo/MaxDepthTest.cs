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

    [TestClass]
    public class MaxDepthTest
    {
        [TestMethod]
        public void Scratch()
        {
            IonSerializer ionSerializer;
            MemoryStream stream;
            Country country;

            
            stream =(MemoryStream)new IonSerializer().Serialize(TestObjects.UnitedStates);

            ionSerializer = new IonSerializer();
            country = ionSerializer.Deserialize<Country>(stream);

            Console.WriteLine(country.States[0].Capital.Mayor);
            // Amazon.IonObjectMapper.Test.Politician
            Console.WriteLine(country.States[0].Capital.Mayor.FirstName);
            // Sarah

            
            stream =(MemoryStream)new IonSerializer().Serialize(TestObjects.UnitedStates);

            ionSerializer = new IonSerializer(new IonSerializationOptions {MaxDepth = 4});
            country = ionSerializer.Deserialize<Country>(stream);

            Console.WriteLine(country.States[0].Capital.Mayor);
            // Amazon.IonObjectMapper.Test.Politician
            Console.WriteLine(country.States[0].Capital.Mayor.FirstName == null);
            // True

        }
    }
}
