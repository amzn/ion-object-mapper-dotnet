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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Amazon.IonObjectMapper.Test;

    [TestClass]
    public class NamingConventionTest
    {
        [TestMethod]
        public void Scratch()
        {
            MemoryStream stream;
            
            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { NamingConvention = new CamelCaseNamingConvention() }).Serialize(TestObjects.honda);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     make: "Honda",
            //     model: "Civic",
            //     yearOfManufacture: 2010,
            //     engine: 'Amazon.IonObjectMapper.Test.my.custom.engine.type'::'Amazon.IonObjectMapper.Test.Engine'::{
            //         cylinders: 4,
            //         manufactureDate: 2009-10-10T06:15:21.0000000-00:00
            //     },
            //     weightInKg: 0.19718574136364542e0
            // }

            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { NamingConvention = new TitleCaseNamingConvention() }).Serialize(TestObjects.honda);
            Console.WriteLine(Utils.PrettyPrint(stream));
            // {
            //     Make: "Honda",
            //     Model: "Civic",
            //     YearOfManufacture: 2010,
            //     Engine: 'Amazon.IonObjectMapper.Test.my.custom.engine.type'::'Amazon.IonObjectMapper.Test.Engine'::{
            //         Cylinders: 4,
            //         ManufactureDate: 2009-10-10T06:15:21.0000000-00:00
            //     },
            //     weightInKg: 0.19718574136364542e0
            // }

            stream = (MemoryStream)new IonSerializer(new IonSerializationOptions { NamingConvention = new SnakeCaseNamingConvention() }).Serialize(TestObjects.honda);
            Console.WriteLine(Utils.PrettyPrint(stream));            
            // {
            //     make: "Honda",
            //     model: "Civic",
            //     year_of_manufacture: 2010,
            //     engine: 'Amazon.IonObjectMapper.Test.my.custom.engine.type'::'Amazon.IonObjectMapper.Test.Engine'::{
            //         cylinders: 4,
            //         manufacture_date: 2009-10-10T06:15:21.0000000-00:00
            //     },
            //     weightInKg: 0.19718574136364542e0
            // }
        }
    }
}
