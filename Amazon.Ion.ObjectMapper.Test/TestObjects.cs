/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace Amazon.Ion.ObjectMapper.Test
{
    using System;
    using System.Collections.Generic;

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
            return $"<Motorcycle>{{ Brand: {this.Brand}, color: {this.color}, canOffroad: {this.canOffroad} }}";
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

    public static class TestObjects
    {
        public static readonly Car Honda = new Car
        {
            Make = "Honda",
            Model = "Civic",
            YearOfManufacture = 2010,
            Weight = new Random().NextDouble(),
            Engine = new Engine { Cylinders = 4, ManufactureDate = DateTime.Parse("2009-10-10T13:15:21Z") },
        };

        public static readonly Registration Registration = new Registration(
            new LicensePlate("KM045F", DateTime.Parse("2020-04-01T12:12:12Z")));

        public static readonly Radio FmRadio = new Radio { Band = "FM" };

        public static readonly Teacher DrKyler = new Teacher("Edward", "Kyler", "Math", DateTime.Parse("08/18/1962"));
        public static readonly Teacher DrFord = new Teacher("Rachel", "Ford", "Chemistry", DateTime.Parse("04/29/1985"));
        private static readonly Teacher[] faculty = { DrKyler, DrFord };
        public static readonly School FieldAcademy = new School("1234 Fictional Ave", 150, new List<Teacher>(faculty));
    }

    public class Car
    {
        private string color;

        public string Make { get; init; }

        public string Model { get; init; }

        public int YearOfManufacture { get; init; }

        public Engine Engine { get; init; }

        [IonIgnore]
        public double Speed => new Random().NextDouble();

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
            return $"<Car>{{ Make: {this.Make}, Model: {this.Model}, YearOfManufacture: {this.YearOfManufacture} }}";
        }
    }

    public class Engine
    {
        public int Cylinders { get; init; }

        public DateTime ManufactureDate { get; init; }

        public override string ToString()
        {
            return $"<Engine>{{ Cylinders: {this.Cylinders}, ManufactureDate: {this.ManufactureDate} }}";
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
            return $"<LicensePlate>{{ license: {this.license} }}";
        }
    }

    public class LicensePlate
    {
        [IonPropertyName("licenseCode")]
        [IonField]
        private readonly string code;

        [IonField]
        private readonly DateTime expires;

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
            return $"<LicensePlate>{{ code: {this.code}, expires: {this.expires} }}";
        }
    }

    public class Radio
    {
        [IonPropertyName("broadcastMethod")]
        public string Band { get; init; }

        public override string ToString()
        {
            return $"<Radio>{{ Band: {this.Band} }}";
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
            return $"<School>{{ address: {this.address}, studentCount: {this.studentCount}, faculty: {this.faculty} }}";
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
            return $"<Teacher>{{ firstName: {this.firstName}, lastName: {this.lastName}, " +
                   $"department: {this.department}, birthDate: {this.birthDate} }}";
        }
    }
}
