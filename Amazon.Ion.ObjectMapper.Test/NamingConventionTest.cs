using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Amazon.Ion.ObjectMapper.Test
{
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
            Dictionary<string, object> customContext = new Dictionary<string, object>() 
            {
                { "customSerializerKey", new CustomSerializerValue()}
            };
            
            var stream = new IonSerializer(new IonSerializationOptions { NamingConvention = new SnakeCaseNamingConvention(), CustomContext =  customContext}).Serialize(TestObjects.honda);
            var serialized = Utils.StreamToIonValue(stream);
            Assert.AreEqual(2010, serialized.GetField("year_of_manufacture").IntValue);
        }
    }
}
