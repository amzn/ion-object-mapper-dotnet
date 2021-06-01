using System;
using System.Collections.Generic;
using System.Globalization;
using Amazon.IonDotnet;

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
  
    public class NegationBoolIonSerializer : IonSerializer<bool>
    {
        public bool Deserialize(IIonReader reader)
        {
            return !reader.BoolValue();
        }

        public void Serialize(IIonWriter writer, bool item)
        {
            writer.WriteBool(!item);
        }
    }
    
    public class ZeroByteArrayIonSerializer : IonSerializer<byte[]>
    {
        public byte[] Deserialize(IIonReader reader)
        {
            return new byte[reader.GetLobByteSize()];
        }

        public void Serialize(IIonWriter writer, byte[] item)
        {
            writer.WriteBlob(new byte[item.Length]);
        }
    }
    
    public class UpperCaseStringIonSerializer : IonSerializer<string>
    {
        public string Deserialize(IIonReader reader)
        {
            return reader.StringValue().ToUpper();
        }

        public void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item.ToUpper());
        }
    }
    
    public class NegativeIntIonSerializer : IonSerializer<int>
    {
        public int Deserialize(IIonReader reader)
        {
            return -reader.IntValue();
        }

        public void Serialize(IIonWriter writer, int item)
        {
            writer.WriteInt(-item);
        }
    }
    
    public class NegativeLongIonSerializer : IonSerializer<long>
    {
        public long Deserialize(IIonReader reader)
        {
            return -reader.LongValue();
        }

        public void Serialize(IIonWriter writer, long item)
        {
            writer.WriteInt(-item);
        }
    }
    
    public class NegativeFloatIonSerializer : IonSerializer<float>
    {
        public float Deserialize(IIonReader reader)
        {
            return -Convert.ToSingle(reader.DoubleValue());
        }

        public void Serialize(IIonWriter writer, float item)
        {
            writer.WriteFloat(-item);
        }
    }
    
    public class NegativeDoubleIonSerializer : IonSerializer<double>
    {
        public double Deserialize(IIonReader reader)
        {
            return -reader.DoubleValue();
        }

        public void Serialize(IIonWriter writer, double item)
        {
            writer.WriteFloat(-item);
        }
    }
    
    public class NegativeDecimalIonSerializer : IonSerializer<decimal>
    {
        public decimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue().ToDecimal();
        }

        public void Serialize(IIonWriter writer, decimal item)
        {
            writer.WriteDecimal(-item);
        }
    }
    
    public class NegativeBigDecimalIonSerializer : IonSerializer<BigDecimal>
    {
        public BigDecimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue();
        }

        public void Serialize(IIonWriter writer, BigDecimal item)
        {
            writer.WriteDecimal(-item);
        }
    }
    
    public class UpperCaseSymbolIonSerializer : IonSerializer<SymbolToken>
    {
        public SymbolToken Deserialize(IIonReader reader)
        {
            var token = reader.SymbolValue();
            return new SymbolToken(token.Text.ToUpper(), token.Sid);
        }

        public void Serialize(IIonWriter writer, SymbolToken item)
        {
            var token = new SymbolToken(item.Text.ToUpper(), item.Sid);
            writer.WriteSymbolToken(token);
        }
    }
    
    public class NextDayDateTimeIonSerializer : IonSerializer<DateTime>
    {
        public DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue.AddDays(1);
        }

        public void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item.AddDays(1)));
        }
    }
    
    public class ZeroGuidIonSerializer : IonSerializer<Guid>
    {
        public Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            return new Guid(blob);
        }

        public void Serialize(IIonWriter writer, Guid item)
        {
            writer.WriteBlob(new byte[item.ToByteArray().Length]);
        }
    }
}
