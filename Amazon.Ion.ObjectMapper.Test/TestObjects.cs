using System;
using Amazon.Ion.ObjectMapper;

namespace Amazon.Ion.ObjectMapper.Test
{

    [IonAnnotateType]
    public abstract class Vehicle
    {

    }

    [IonAnnotateType(Prefix = "my.universal.namespace", Name="BussyMcBusface")] 
    public class Bus : Vehicle
    {
        public override string ToString()
        {
            return "<Bus>";
        }
    }

    public class Truck : Vehicle
    {
        public override string ToString()
        {
            return "<Truck>";
        }
    }

    public class Plane : Vehicle
    {
        public int MaxCapacity { get; init; }

        public override string ToString()
        {
            return "<Plane>";
        }
    }

    [IonAnnotateType]
    public class Boat : Vehicle
    {
        public override string ToString()
        {
            return "<Boat>";
        }
    }
    
    [IonDoNotAnnotateType(ExcludeDescendants = true)]
    public class Yacht : Boat
    {
        public override string ToString()
        {
            return "<Yacht>";
        }
    }
    
    public class Catamaran : Yacht
    {
        public override string ToString()
        {
            return "<Catamaran>";
        }
    }

    public class Helicopter : Vehicle
    {
        public override string ToString()
        {
            return "<Helicopter>";
        }
    }

    [IonAnnotateType(ExcludeDescendants = true)] 
    public class Jet : Plane
    {
        public override string ToString()
        {
            return "<Jet>";
        }
    }

    public class Boeing : Jet
    {
        public override string ToString()
        {
            return "<Boeing>";
        }
    }

    public static class TestObjects {
        public static Car honda = new Car 
        { 
            Make = "Honda", 
            Model = "Civic", 
            YearOfManufacture = 2010, 
            Weight = new Random().NextDouble(),
            Engine = new Engine { Cylinders = 4, ManufactureDate = DateTime.Parse("2009-10-10T13:15:21Z") } 
        };
    }

    public class Car
    {
        private string color;

        public string Make { get; init; }
        public string Model { get; init; }
        public int YearOfManufacture { get; init; }
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
            return "<Car>{ Make: " + Make + ", Model: " + Model + ", YearOfManufacture: " + YearOfManufacture + " }";
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