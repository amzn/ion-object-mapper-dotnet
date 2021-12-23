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

namespace Amazon.IonObjectMapper.Test
{
    using System;
    using System.IO;
    using Amazon.IonDotnet.Tree;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NamingConventionTest
    {
        [TestMethod]
        public void CanConvertToCamelCase()
        {
            Assert.AreEqual("camelCaseWords", new CamelCaseNamingConvention().FromProperty("CamelCaseWords"));
            Assert.AreEqual("NonCamelCaseWords", new CamelCaseNamingConvention().ToProperty("nonCamelCaseWords"));
        }
        
        [TestMethod]
        public void CanConvertToTitleCase()
        {
            Assert.AreEqual("TitleCaseWords", new TitleCaseNamingConvention().FromProperty("TitleCaseWords"));
            Assert.AreEqual("NonTitleCaseWords", new TitleCaseNamingConvention().ToProperty("nonTitleCaseWords"));
        }

        [TestMethod]
        public void CanConvertToSnakeCase()
        {
            Assert.AreEqual("snake_case_words", new SnakeCaseNamingConvention().FromProperty("SnakeCaseWords"));
            Assert.AreEqual("NonSnakeCaseWords", new SnakeCaseNamingConvention().ToProperty("non_snake_case_words"));
            Assert.AreEqual("weird_i", new SnakeCaseNamingConvention().FromProperty("weirdI"));
            Assert.AreEqual("weird_i", new SnakeCaseNamingConvention().FromProperty("WeirdI"));
            Assert.AreEqual("lots_o_f_capitals", new SnakeCaseNamingConvention().FromProperty("LotsOFCapitals"));
            Assert.AreEqual("EndsInAnUnderscore", new SnakeCaseNamingConvention().ToProperty("ends_in_an_underscore_"));
            Assert.AreEqual("StartsWithAnUnderscore", new SnakeCaseNamingConvention().ToProperty("_starts_with_an_underscore"));
        }

        [TestMethod]
        public void CanUseTheNamingConventionOnAnObject()
        {
            var stream = new IonSerializer(new IonSerializationOptions { NamingConvention = new SnakeCaseNamingConvention() }).Serialize(TestObjects.honda);
            var serialized = Utils.StreamToIonValue(stream);
            Assert.AreEqual(2010, serialized.GetField("year_of_manufacture").IntValue);
        }
    }
}
