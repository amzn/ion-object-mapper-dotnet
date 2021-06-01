using System;
using System.Collections.Generic;
using System.Globalization;

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
    public class Motorcycle : Vehicle
    {
        public string Brand { get; init; }

        [IonField]
        public string color;

        [IonField]
        public bool canOffroad; 
        
        public override string ToString()
        {
            return "<Motorcycle>{ Brand: " + Brand + ", color: " + color + ", canOffroad: " + canOffroad + " }";
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

    [IonDoNotAnnotateType(ExcludeDescendants = true)]
    public class Ship : Boat
    {
        public string Name { get; init; }
        public int Weight { get; init; }
        public int Capacity { get; init; }
        
        public override string ToString()
        {
            return $"<Ship>{{ Name: {this.Name}, Weight: {this.Weight}, Capacity: {this.Capacity} }}";
        }
    }
    
    // For testing case insensitive deserialization.
    public class ShipWithVariedCasing : Boat
    {
        public string name { get; init; }
        public double WEIGHT { get; init; }
        public int CaPaCiTy { get; init; }
        
        public override string ToString()
        {
            return $"<Ship>{{ name: {this.name}, WEIGHT: {this.WEIGHT}, CaPaCiTy: {this.CaPaCiTy} }}";
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

        public static Registration registration = new Registration(new LicensePlate("KM045F", DateTime.Parse("2020-04-01T12:12:12Z")));

        public static Radio fmRadio = new Radio { Band = "FM" };

        public static Teacher drKyler = new Teacher("Edward", "Kyler", "Math", DateTime.ParseExact("18/08/1962", "dd/MM/yyyy", CultureInfo.InvariantCulture));
        public static Teacher drFord = new Teacher("Rachel", "Ford", "Chemistry", DateTime.ParseExact("29/04/1985", "dd/MM/yyyy", CultureInfo.InvariantCulture));
        private static Teacher[] faculty = { drKyler, drFord };
        public static School fieldAcademy = new School("1234 Fictional Ave", 150, new List<Teacher>(faculty));

        private static Politician GeorgeAdams = new Politician {FirstName = "George", LastName = "Adams" };
        private static Politician SuzanneBenson = new Politician {FirstName = "Suzanne", LastName = "Benson" };
        private static Politician SarahCasey = new Politician {FirstName = "Sarah", LastName = "Casey" };
        private static Politician CharlesRogers = new Politician {FirstName = "Charles", LastName = "Rogers" };
        private static Politician RolandCohen = new Politician {FirstName = "Roland", LastName = "Cohen" };
        private static Politician GeneHouston = new Politician {FirstName = "Gene", LastName = "Houston" };
        private static City WashingtonDC = new City {Name = "Washington D.C.", Mayor = SuzanneBenson};
        private static City Olympia = new City {Name = "Olympia", Mayor = SarahCasey};
        private static City Austin = new City {Name = "Austin", Mayor = RolandCohen};
        private static State Washington = new State {Name = "Washington", Capital = Olympia, Governor = CharlesRogers};
        private static State Texas = new State {Name = "Texas", Capital = Austin, Governor = GeneHouston};
        private static State[] states = { Washington, Texas };
        public static Country UnitedStates = new Country
        {
            Name = "United States of America",
            Capital = WashingtonDC,
            President = GeorgeAdams,
            States = new List<State>(states)
        };

        public static Ship Titanic = new Ship
        {
            Name = "Titanic",
            Weight = 52310,
            Capacity = 2230,
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

    public class Registration
    {
        [IonField]
        private LicensePlate license;

        public Registration()
        {
        }

        public Registration(LicensePlate license)
        {
            this.license = license;
        }

        public override string ToString()
        {
            return "<LicensePlate>{ license: " + license + " }";
        }
    }

    public class LicensePlate
    {
        [IonPropertyName("licenseCode")]
        [IonField]
        private readonly string code;

        [IonField]
        DateTime expires;

        public LicensePlate()
        {
        }

        public LicensePlate(string code, DateTime expires)
        {
            this.code = code;
            this.expires = expires;
        }

        public override string ToString()
        {
            return "<LicensePlate>{ code: " + code + ", expires: " + expires +  " }";
        }
    }

    public class Radio
    {
        [IonPropertyName("broadcastMethod")]
        public string Band { get; init; }

        public override string ToString()
        {
            return "<Radio>{ Band: " + Band + " }";
        }
    }

    public class Wheel
    {
        [IonConstructor]
        public Wheel([IonPropertyName("specification")] string specification)
        {

        }
    }

    public class School
    {
        private readonly string address;
        private int studentCount;
        private List<Teacher> faculty;

        public School()
        {
            this.address = null;
            this.studentCount = 0;
            this.faculty = new List<Teacher>();
        }
        
        public School(string address, int studentCount, List<Teacher> faculty)
        {
            this.address = address;
            this.studentCount = studentCount;
            this.faculty = faculty;
        }
        
        public override string ToString()
        {
            return "<School>{ address: " + address + ", studentCount: " + studentCount + ", faculty: " + faculty + " }";
        }
    }

    public class Teacher
    {
        public readonly string firstName;
        public readonly string lastName;
        public string department;
        public readonly DateTime? birthDate;

        public Teacher()
        {
            this.firstName = null;
            this.lastName = null;
            this.department = null;
            this.birthDate = null;
        }
        
        public Teacher(string firstName, string lastName, string department, DateTime birthDate)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.department = department;
            this.birthDate = birthDate;
        }
        
        public override string ToString()
        {
            return "<Teacher>{ firstName: " + firstName + ", lastName: " + lastName + ", department: " + department + ", birthDate: " + birthDate + " }";
        }
    }
    
    public class Ruler
    {
        internal int size;

        [IonPropertyGetter("size")]
        public int GetSize() 
        {
            return this.size + 10;
        }
    
        [IonPropertySetter("size")]
        public void SetSize(int input) 
        {
            this.size = input - 10;
        }
        
        public override string ToString()
        {
            return $"<Ruler>{{ size: {this.size} }}";
        }
    }

    public class Country
    {
        public string Name { get; init; }
        public City Capital { get; init; }
        public Politician President { get; init; }
        public List<State> States { get; init; }
    }

    public class State
    {
        public string Name { get; init; }
        public City Capital { get; init; }
        public Politician Governor { get; init; }
    }

    public class City
    {
        public string Name { get; init; }
        public Politician Mayor { get; init; }
    }

    public class Politician
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }
}
