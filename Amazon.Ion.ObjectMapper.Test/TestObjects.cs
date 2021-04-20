using System;
using Amazon.Ion.ObjectMapper;

namespace Amazon.Ion.ObjectMapper.Test
{
    public class Car
    {
        private string color;

        public string Make { get; init; }
        public string Model { get; init; }
        public int Year { get; init; }
        public Engine Engine { get; init; }
        
        [IonIgnore]
        public double Speed { get { return new Random().NextDouble(); } }

        [IonPropertyName("weightInKg")]
        public double Weight { get; init; }

        public string GetColor() 
        {
            return "#FF0000";
        }

        public void SetColor(string input) 
        {
            this.color = input;
        }

        public override string ToString()
        {
            return "<Car>{ Make: " + Make + ", Model: " + Model + ", Year: " + Year + " }";
        }
    }

    public class Engine
    {
        public int Cylinders { get; init; }
        public DateTime ManufactureDate { get; init; }

        public override string ToString()
        {
            return "<Engine>{ Cylinders: " + Cylinders + ", ManufactureDate: " + ManufactureDate + " }";
        }
    }

    public class Wheel
    {
        [IonConstructor]
        public Wheel([IonPropertyName("specification")] string specification)
        {

        }
    }
}