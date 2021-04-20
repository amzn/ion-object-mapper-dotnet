using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Amazon.Ion.ObjectMapper.Test.Utils;

namespace Amazon.Ion.ObjectMapper.Test
{
    [TestClass]
    public class IonObjectSerializerTest
    {
        private Car honda = new Car 
        { 
            Make = "Honda", 
            Model = "Civic", 
            Year = 2010, 
            Weight = new Random().NextDouble(),
            Engine = new Engine { Cylinders = 4, ManufactureDate = DateTime.Parse("2009-10-10T13:15:21Z") } 
        };

        [TestMethod]
        public void SerializesAndDeserializesObjects()
        {
            Check(honda);
        }
    }
}