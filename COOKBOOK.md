# Cookbook

#### What is the Ion Object Mapper

The Ion Object Mapper is a convenience library built to assist developers when dealing with data that's in the format of [Amazon Ion](https://amzn.github.io/ion-docs/). It is used to easily transform C# classes and objects into Ion format and vice-versa. There are numerous additional settings and features that can be configured for advanced users.

#### Basic Serialization and Deserialization

The mapper (IonSerializer) supports by default the types listed in the [spec](SPEC.MD#primitive-type-conversion) under the "Primitive type conversion" section. There are ways to customize the conversion but the default use case adheres to the spec.

```c#
public class MyPOCO
{
    public string ValueOne { get; init; }
    public int ValueTwo { get; init; }
}        

private static void SerializationExample()
{
    IonSerializer serializer = new IonSerializer();

    MyPOCO poco = new MyPOCO
    {
        ValueOne = "one",
        ValueTwo = 2,
    };

    Stream serialized = serializer.Serialize(poco);
    // Most AWS libraries will take in a Stream type.
    // The Ion library can build an IIonReader from a Stream for using IIonValue.
}

private static void DeserializationExample(Stream serialized)
{
    // Assume serialized is the same stream from the SerializationExample.
    IonSerializer serializer = new IonSerializer();

    // The format of the serialized data must be known and a supported output type must be supplied to the Deserialize method.
    MyPOCO poco = serializer.Deserialize<MyPOCO>(serialized);
    // This poco will have the same values as the poco from the SerializationExample.
}
```

#### Serializer Configuration

The full list of configurations available and what they do can be found in the [spec](SPEC.md) under Serialization options. In most scenarios, the default options (none passed into the constructor) are sufficient but various options can be helpful to reduce the need for transformation or mutation of objects after serialization or deserialization.

```c#
private static void CreateSerializerWithOptions()
{
    IonSerializationOptions myOptions = new IonSerializationOptions
    {
        // Enable the serialization and deserialization of all fields in classes
        IncludeFields = true,
        // Do not serialize or deserialize both C# and Ion null values
        IgnoreNulls = true
    };

    IonSerializer serializer = new IonSerializer(myOptions);
}
```

#### Attributes

Various attributes provided by this library can be used to customize the behavior of the serializer's mapping.

##### IonAnnotateType

This attribute allows deserialization into specific subtypes when they share a common parent type. For example, the following allows you to serialize any `Car` whilst keeping its type as an Ion annotation. When deserializing, the serializer uses this information to instantiate the correct class instance. `IonDoNotAnnotateType` can be used to exclude a subtype which inherits from a parent type which has `IonAnnotateType` enabled.

```c#
[IonAnnotateType]
public abstract class Car 
{    
    [IonAnnotateType]
    public Engine { get; init; }
}
public class Honda : Car { }
public class Toyota : Car { }

public class Engine { }
public class Hybrid : Engine { }

Car myCar = new Honda 
	{ 
		Engine = new Hybrid();
	};

IonSerializer serializer = new IonSerializer();

// The serialized Ion will keep track of the type as an Annotation
Stream serialized = serializer.Serialize(myCar);
// This will instantiate Honda or Toyota types with the specified engine depending on the stream contents
myCar = serializer.Deserialize<Car>(serialized);
```

#### IonPropertyName

This attribute specifies the name to use for the serialized name of property and fields, as well as mapping them back to the property or field during deserialization.

```c#
public class Car
{
    [IonPropertyName("weightInKg")]
    public double Weight { get; init; }
}

Car myCar = new Car
    {
		Weight = 1400;
    };

IonSerializer serializer = new IonSerializer();

// Ion Text of this is { "weightInKg": 1400 }
Stream serialized = serializer.Serialize(myCar);
// Deserilizing this stream into a Car would map weightInKg to Weight as well
```

#### IonConstructor

This attributes specifies a Constructor method that will be invoked during deserialization. The `IonPropertyName` can also be used to pass parameters into the Constructor.

```c#
public class Car
{
    public double WeightLB { get; init; }
    
    [IonPropertyName("weightInKg")]
    public double Weight { get; init; }
    
    public DateTime CreatedOn { get; init; }
    
    [IonConstructor]
    public Car([IonPropertyName("weightInKg")] double weight) 
    {
        this.WeightLB = weight * 2.205;
        this.CreatedOn = DateTime.Now;
    }
}

// Ion Text of serialized Stream is { "weightInKg": 1400 }
Car myCar = new IonSerializer().Deserialize<Car>(stream);
// myCar would have all 3 properties filled out accordingly
```

#### IonField

Fields are ignored by default by the serializer, but this attribute specifies that they should not be ignored.

```c#
public class Motorcycle
{
    public string Make { get; init; }

    [IonField]
    public string color;
}
```

#### IonIgnore

Any field, method, or property tagged with this attribute causes the serializer to ignore it.

```c#
public Document
{
    public string Info { get; init; }
    
    [IonIgnore]
    public string Metadata { get; init; }
}
```

#### IonPropertyGetter and IonPropertySetter

Methods can be tagged with this attribute to act as getters and setters for properties and fields provided the “get” signature is a no-argument method and the “set” signature is a one-argument `void` method.

```c#
public class Car
{
    [IonField]
    private string color;
    
    [IonPropertyGetter("color")]
    public string GetColor() 
    {
        return "#FF0000";
    }
    
    [IonPropertySetter("color")]
    public void SetColor(string input) 
    {
        this.color = input;
    }
}
```

