using System;
using System.Collections.Generic;
using System.Globalization;

namespace Amazon.IonObjectMapper.Test
{

    [IonAnnotateType]
    public class Vehicle
    {
        public override string ToString()
        {
            return "<Vehicle>";
        }
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

        public static Truck nativeTruck = new Truck();

        public static string truckIonText = "Truck:: { }";

        public static Registration registration = new Registration(new LicensePlate("KM045F", DateTime.Parse("2020-04-01T12:12:12Z")));

        public static Radio fmRadio = new Radio { Band = "FM" };

        public static Teacher drKyler = new Teacher("Edward", "Kyler", "Math", DateTime.ParseExact("18/08/1962", "dd/MM/yyyy", CultureInfo.InvariantCulture));
        public static Teacher drFord = new Teacher("Rachel", "Ford", "Chemistry", DateTime.ParseExact("29/04/1985", "dd/MM/yyyy", CultureInfo.InvariantCulture));
        private static Teacher[] faculty = { drKyler, drFord };
        public static School fieldAcademy = new School("1234 Fictional Ave", 150, new List<Teacher>(faculty));
        
        public static Student JohnGreenwood = new Student("John", "Greenwood", "Physics");
        
        public static Desk SchoolDesk = new Desk {width = 48, Depth = 24, Height = 30};
        public static Ruler Ruler = new Ruler {length = 30, unit = "cm"};
        public static Chalkboard Chalkboard = new Chalkboard {width = 48, height = 36};

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
         
        public static ClassWithProperties objectWithProperties = new ClassWithProperties("Public Value", "Protected Value",
            "Protected Internal Value", "Internal Value", "Private Value", "Private Protected Value");

        public static ClassWithReadonlyProperties objectWithReadonlyProperties = new ClassWithReadonlyProperties("Public Value", "Protected Value",
            "Protected Internal Value", "Internal Value", "Private Value", "Private Protected Value");

        public static ClassWithIonPropertyNamesAttribute objectWithIonPropertyNameAttributes = new ClassWithIonPropertyNamesAttribute("Public Value", "Protected Value",
            "Protected Internal Value", "Internal Value", "Private Value", "Private Protected Value");

        public static ClassWithMethods objectWithMethods = new ClassWithMethods("Public Value", "Protected Value",
            "Protected Internal Value", "Internal Value", "Private Value", "Private Protected Value");
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
        public string Specification { get; init; }
        public int Size { get; init; }
        
        public string Brand { get; init; }

        [IonField]
        internal string color;

        public Wheel(string specification, int size, string brand, string color)
        {
            this.Specification = specification;
            this.Size = size;
            this.Brand = brand;
            this.color = color;
        }

        [IonConstructor]
        private Wheel(
            [IonPropertyName("specification")] string specification,
            [IonPropertyName("size")] int size)
        {
            this.Specification = $"Specification: {specification}, Size: {size} inches";
            this.Size = size;
        }
    }

    public class Tire
    {
        public string specification { get; init; }
        public int size { get; init; }

        [IonConstructor]
        public Tire(string specification, int size)
        {
            this.specification = specification;
            this.size = size;
        }

        [IonConstructor]
        private Tire(
            [IonPropertyName("size")] int size,
            [IonPropertyName("specification")] string specification)
        {
            this.specification = $"Specification: {specification}, Size: {size} inches";
            this.size = size;
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

    public class Student
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Major { get; }

        public Student()
        {
            this.FirstName = default;
            this.LastName = default;
            this.Major = default;
        }
        
        public Student(string firstName, string lastName, string major)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Major = major;
        }
        
        public override string ToString()
        {
            return $"<Student>{{ FirstName: {FirstName}, LastName: {LastName}, Major: {Major} }}";
        }
    }

    public class Desk
    {
        internal int width;
        public int Depth { get; init; }
        public int Height { get; init; }

        [IonPropertyGetter("desk width")]
        public int GetWidth() 
        {
            return this.width;
        }
        
        [IonPropertySetter("desk width")]
        public void SetWidth(int width) 
        {
            this.width = width;
        }

        public override string ToString()
        {
            return $"<Desk>{{ width: {width},  Depth: {Depth}, Height: {Height} }}";
        }
    }

    public class Ruler
    {
        public int length { get; set; }
        
        [IonField]
        internal string unit;

        [IonPropertyGetter("length")]
        public int GetLength() 
        {
            return this.length;
        }

        [IonPropertySetter("length")]
        public void SetLength(int length) 
        {
            this.length = length;
        }
        
        [IonPropertyGetter("unit")]
        public string GetUnit()
        {
            return this.unit;
        }

        [IonPropertySetter("unit")]
        public void SetUnit(string unit)
        {
            this.unit = unit;
        }
    }

    public class Chalkboard
    {
        public int width { get; set; }
        public int height { get; set; }

        [IonPropertySetter("width")]
        public void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;
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

    public class TestDictionary : Dictionary<string, int>
    {
        public static string PrettyString(IDictionary<string, int> dictionary)
        {
            return string.Join(Environment.NewLine, dictionary);
        }
    }

    public class ClassWithOnlySetProperty
    {
        private string val;

        public ClassWithOnlySetProperty(string input)
        {
            val = input;
        }

        public string SetOnlyProperty
        {
            set { val = value; }
        }
    }

    public class ClassWithProperties
    {
        public ClassWithProperties() { }

        public ClassWithProperties(string publicValue, string protectedValue, string protectedInternalValue,
            string internalValue, string privateValue, string privateProtectedValue)
        {
            this.PublicProperty = publicValue;
            this.ProtectedProperty = protectedValue;
            this.ProtectedInternalProperty = protectedInternalValue;
            this.InternalProperty = internalValue;
            this.PrivateProperty = privateValue;
            this.PrivateProtectedProperty = privateProtectedValue;
        }

        public string PublicProperty { get; set; }
        protected string ProtectedProperty { get; set; }
        protected internal string ProtectedInternalProperty { get; set; }
        internal string InternalProperty { get; set; }
        private string PrivateProperty { get; set; }
        private protected string PrivateProtectedProperty { get; set; }

        public override string ToString()
        {
            return $"<ClassWithProperties>{{ PublicProperty: {PublicProperty}, ProtectedProperty: {ProtectedProperty}, ProtectedInternalProperty: {ProtectedInternalProperty}, " +
                   $"InternalProperty: {InternalProperty}, PrivateProperty: {PrivateProperty}, PrivateProtectedProperty: {PrivateProtectedProperty} }}";
        }
    }

    public class ClassWithReadonlyProperties
    {
        public ClassWithReadonlyProperties() { }

        public ClassWithReadonlyProperties(string publicValue, string protectedValue, string protectedInternalValue,
            string internalValue, string privateValue, string privateProtectedValue)
        {
            this.PublicProperty = publicValue;
            this.ProtectedProperty = protectedValue;
            this.ProtectedInternalProperty = protectedInternalValue;
            this.InternalProperty = internalValue;
            this.PrivateProperty = privateValue;
            this.PrivateProtectedProperty = privateProtectedValue;
        }

        public string PublicProperty { get; }
        protected string ProtectedProperty { get; }
        protected internal string ProtectedInternalProperty { get; }
        internal string InternalProperty { get; }
        private string PrivateProperty { get; }
        private protected string PrivateProtectedProperty { get; }

        public override string ToString()
        {
            return $"<ClassWithReadonlyProperties>{{ PublicProperty: {PublicProperty}, ProtectedProperty: {ProtectedProperty}, ProtectedInternalProperty: {ProtectedInternalProperty}, " +
                   $"InternalProperty: {InternalProperty}, PrivateProperty: {PrivateProperty}, PrivateProtectedProperty: {PrivateProtectedProperty} }}";
        }
    }

    public class ClassWithIonPropertyNamesAttribute
    {
        public ClassWithIonPropertyNamesAttribute() { }

        public ClassWithIonPropertyNamesAttribute(string publicValue, string protectedValue, string protectedInternalValue,
            string internalValue, string privateValue, string privateProtectedValue)
        {
            this.PublicProperty = publicValue;
            this.ProtectedProperty = protectedValue;
            this.ProtectedInternalProperty = protectedInternalValue;
            this.InternalProperty = internalValue;
            this.PrivateProperty = privateValue;
            this.PrivateProtectedProperty = privateProtectedValue;
        }

        [IonPropertyName("Public Property")]
        public string PublicProperty { get; set; }

        [IonPropertyName("Protected Property")]
        protected string ProtectedProperty { get; set; }

        [IonPropertyName("Protected Internal Property")]
        protected internal string ProtectedInternalProperty { get; set; }

        [IonPropertyName("Internal Property")]
        internal string InternalProperty { get; set; }

        [IonPropertyName("Private Property")]
        private string PrivateProperty { get; set; }

        [IonPropertyName("Private Protected Property")]
        private protected string PrivateProtectedProperty { get; set; }

        public override string ToString()
        {
            return $"<ClassWithIonPropertyNamesAttribute>{{ PublicProperty: {PublicProperty}, ProtectedProperty: {ProtectedProperty}, ProtectedInternalProperty: {ProtectedInternalProperty}, " +
                   $"InternalProperty: {InternalProperty}, PrivateProperty: {PrivateProperty}, PrivateProtectedProperty: {PrivateProtectedProperty} }}";
        }
    }

    public class ClassWithMethods
    {
        public string publicValue;
        public string protectedValue;
        public string protectedInternalValue;
        public string internalValue;
        public string privateValue;
        public string privateProtectedValue;

        public ClassWithMethods() { }

        public ClassWithMethods(string publicValue, string protectedValue, string protectedInternalValue,
            string internalValue, string privateValue, string privateProtectedValue)
        {
            this.publicValue = publicValue;
            this.protectedValue = protectedValue;
            this.protectedInternalValue = protectedInternalValue;
            this.internalValue = internalValue;
            this.privateValue = privateValue;
            this.privateProtectedValue = privateProtectedValue;
        }

        [IonPropertyGetter("public value")]
        public string GetPublicValue()
        {
            return this.publicValue;
        }

        [IonPropertySetter("public value")]
        public void SetPublicValue(string value)
        {
            this.publicValue = value;
        }

        [IonPropertyGetter("protected value")]
        protected string GetProtectedValue()
        { 
            return this.protectedValue;
        }

        [IonPropertySetter("protected value")]
        protected void SetProtectedValue(string value)
        {
            this.protectedValue = value;
        }

        [IonPropertyGetter("protected internal value")]
        protected internal string GetProtectedInternalValue()
        {
            return this.protectedInternalValue;
        }

        [IonPropertySetter("protected internal value")]
        protected internal void SetProtectedInternalValue(string value)
        {
            this.protectedInternalValue = value;
        }

        [IonPropertyGetter("internal value")]
        internal string GetInternalValue()
        {
            return this.internalValue;
        }

        [IonPropertySetter("internal value")]
        internal void SetInternalValue(string value)
        {
            this.internalValue = value;
        }

        [IonPropertyGetter("private value")]
        private string GetPrivateValue()
        {
            return this.privateValue;
        }

        [IonPropertySetter("private value")]
        private void SetPrivateValue(string value)
        {
            this.privateValue = value;
        }

        [IonPropertyGetter("private protected value")]
        private protected string GetPrivateProtectedValue()
        {
            return this.privateProtectedValue;
        }

        [IonPropertySetter("private protected value")]
        private protected void SetPrivateProtectedValue(string value)
        {
            this.privateProtectedValue = value;
        }

        public override string ToString()
        {
            return $"<ClassWithMethods>{{ PublicValue: {publicValue}, ProtectedValue: {protectedValue}, ProtectedInternalValue: {protectedInternalValue}, " +
                   $"InternalValue: {internalValue}, PrivateValue: {privateValue}, PrivateProtectedValue: {privateProtectedValue} }}";
        }
    }

    public class ObjectWithPublicGetter
    {
        public string Property { init; get; }
    }

    public class ObjectWithPrivateSetter
    {
        public string val;

        private string Property
        {
            set { val = value; }
        }
    }
}
