# Ion Object Mapping in .NET for QLDB

Ion is clumsy to use with QLDB in .NET and [customers have asked](https://amzn-aws.slack.com/archives/G01CFDRUG6R/p1612319039019200) (see Appendix A) for it to be improved. [From our own documentation](https://docs.aws.amazon.com/qldb/latest/developerguide/driver-quickstart-dotnet.html), this is how you create an Ion struct to send an Ion value to the database:

```c#
IIonValue ionPerson = valueFactory.NewEmptyStruct();
ionPerson.SetField("firstName", valueFactory.NewString("John"));
ionPerson.SetField("lastName", valueFactory.NewString("Doe"));
ionPerson.SetField("age", valueFactory.NewInt(32));
```

And this is how you read a value from the database:

```c#
foreach (IIonValue row in selectResult)
{
    var person = new Person
    {
        FirstName = row.GetField("firstName").StringValue(),
        LastName = row.GetField("lastName").StringValue();
        row.GetField("age").IntValue();
    }
}
```

This is so onerous for one customer that they wrote this code:

```c#
Execute("insert into myTable {JsonConvert(person).replace("\"", "'")}")

```

Which is arguably still clumsy, possibly incorrect, and definitely dangerous in potentially causing SQL injection attacks.

## Solution

We will enhance the QLDB .NET Driver to allow for .NET objects to be passed to it, rather than `IIonValue`. We will provide standard and comprehensive mappings from objects to Ion and back, but if we miss a case, we will provide extension points allowing for complete control over the Ion serialization processing. This solution is influenced by the new [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=net-5.0) namespace, therefore aligning ourselves with Microsoft’s vision for object serialization and providing developers with a least-surprise API.

Given a `Car` object defined as follows:

```c#
class Car
{
    public string Make { get; init; }  // "init" is new keyword meaning you set
    public string Model { get; init; } //   this property once only
    public int Year { get; init; }
}
```

SQL interactions for sending parameters should look like this:

```c#
driver.Execute(tx =>
{
    tx.Execute(tx.Query(
        "insert into Car ?", 
        new Car { Make = "Opel", Model = "Monza", Year = 1997 }));
});
```

And interactions for reading results could look like this:

```c#
var hondaYears = driver.Execute(tx =>
{
    return from car in tx.Execute(tx.Query<Car>("select * from Car"))
        where car.Make == "Honda"
        select car.Year;
});
```

(The interaction with Linq for illustration purposes only. Obviously you would do the filtering and selection in the PartiQL statement itself).

This leverages [Linq](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/) to operate on the resultant `IEnumerable` returned by the driver. With the `Query<Car>` method it leans on C# reification to deserialize the desired type from the database. By adding these two interfaces the inconvenience of converting to and from Ion can be completely avoided in the client code. 

If finer grained control over the results is required, the `IQuery<ResultT>` object passed to `Execute` can be created in whatever way the client code wants conforming to this inteface:

```c#
public interface IQuery<T>
{
    string Statement { get; }
    ValueHolder[] Parameters { get; }
    T Deserialize(ValueHolder ionValueHolder);
}1
```

So long as the users `IQuery` provides a `Statement` `string`, can supply `Parameters` as `ValueHolder`s (which is a QLDB type which contains the raw Ion text or binary), and can convert from a result `ValueHolder` into a desired `T` representing the
object to deserialize from the database, they can interpret the raw Ion binary however they see fit.

To support this new `IQuery` interface, the following two methods will be added to the driver's `TransactionExecutor` class:

```c#
IResult<T> Execute<T>(IQuery<T> query)
IQuery<T> Query<T>(string statement, params object[] parameters)
```

which allows for an `IQuery` to be created as well as executed against the current transaction.

The most common usage would be to supply the Object Mapper described in this spec to the driver when the driver is built as follows:

```c#
var driver = QldbDriver.Builder()
    .WithLedger("cars")
    .WithSerializer(new ObjectSerializer())
    .Build();
```

Where `ObjectSerializer` is an implementation of `ISerializer` which is defined as:

```c#
public interface ISerializer
{
    ValueHolder Serialize(object o);
    T Deserialize<T>(ValueHolder s);
}
```

This simply converts from `object` to `ValueHolder` (which is just a simple object which contains Ion) and back. In the normal usage, the `transactionExecutor.Query<T>` method provides a way for the driver to supply the `ISerializer` given to the driver
to each transaction statement. This means developers can easily use the default driver and default Object Mapper described here without coupling the Object Mapper to the Driver or vice versa. It also means developers can write their own if they wish.

## New Ion .NET Serialization Library

In order to convert from Ion to `object` and back, we need a new Ion mapping library. The simplest interface for this library is:

```c#
Stream stream = new IonSerializer().Serialize(car);
Car car = new IonSerializer().Deserialize<Car>(stream);
```

Using the generic type information, available at runtime thanks to C# reification, when deserializing, the library will detect that we are attempting to build a `Car` object. It will instantiate a `Car` and then set the properties on the `Car` that it finds in the `Stream`. Similarly it would be possible to deserialize a `List` of `Car` again because the generic type information is present at runtime.

```c#
List<Car> = new IonSerializer().Deserialize<List<Car>>(stream);
```

### Default behavior

We describe the default behavior of the mapping library and later the extension points to customize the defaults. The most natural way to use the library is to declare POCOs (**P**lain-**o**ld **C**LR **O**bjects):

```c#
public class Car
{
    public string Make { get; init; }
    public string Model { get; init; }
    public int Year { get; init; }
    public Engine Engine { get; init; }
}

public class Engine
{
    public int Cylinders { get; init; }
    public DateTime ManufactureDate { get; init; }
}
```

the `init` keyword is a C# modifier which indicates that the property is only settable at construction time. In reality the Intermediate Language (IL) code just contains a regular setter and so long as properties in general have a getter and setter, the library will be able to use them. By default, all public gettable properties will be serialized and all public settable properties will be deserialized, but this can be configured as specified later in this document.

### Default behavior when deserializing annotated Ion

Annotated Ion types will by default map to the C# class name in currently loaded assemblies if and only if the **first** type annotation matches the name of the class. The C# class type must also be a subtype of the type passed into the deserialize method.

```c#
public class DefaultDeserialization
{
    public class Car
    {
        public override string ToString()
        {
            return "<Car>";
        }
    }
    
    public class Truck : Car
    {
        public override string ToString()
        {
            return "<Truck>";
        }
    }
    
    public void DeserializeExample() {
        // Create Ion struct with annotation that matches Truck Class Type name.
        
        string truckIonText = "Truck:: { }";

        IIonReader reader = IonReaderBuilder.Build(truckIonText);

        IonSerializer ionSerializer = new IonSerializer();
        // This will only attempt to deserialize into a Truck if and only if Truck is a subtype of the input Type, in this case Typeof(car).
        var car = ionSerializer.Deserialize(reader, Typeof(Car));

        // This will print "<Truck>".
        string output = car.ToString();
    }
}
```



### Supported types

In addition to all `object` types these types are supported by default:

* `Dictionary<TKey, TValue>` (also non-generic)
* `List<T>` (also non-generic)
* Arrays.

```c#
new IonSerializer().Deserialize<List<Car>>(stream);
new IonSerializer().Deserialize<Map<string, object>>(stream);
```

### Ignoring properties

It is possible to ignore a property with the `Attribute` `IonIgnore`:

```c#
public class Car
{
    [IonIgnore]
    public double Speed { get; }
}
```

### Encoding types

Sometimes one will need to deserialize streams of different types which share a common subtype. Alternately, sometimes one might want to coerce the deserialized type to a specific type. For this we lean on Ion annotations and another C# `Attribute` `IonAnnotateType`. This attribute can be set on a class and it will then include their type information. Alternately it can be set on a property.

```c#
[IonAnnotateType]
public class Car 
{    
    [IonAnnotateType("my.custom.engine.type")]
    public Engine { get; init; }
}
public class Honda : Car { }
public class Toyota : Car { }

public class Engine { }
public class Hybrid : Engine { }

// this will instantiate Cars, Hondas, or Toyota types depending on the stream
//  contents
new IonSerializer().Deserialize<List<Car>>(stream);
```

The type information will be included in the Ion stream as an [Ion annotation](https://amzn.github.io/ion-docs/docs/spec.html#annot). This will be the fully qualified class and namespace of the desired type to be created. At deserialization time, if the annotation is found but the type or property specified in the annotation to be deserialized is not marked with the `IonAnnotateType` attribute the annotation is ignored and deserialization carries on as normal. For example, this Ion would be produced:

```c#
com.amazon.vehicles.Honda::{
    "year": 2010,
}
```

It is further possible to specify the exact type annotation string that will be written, although this is optional and the class name will be used as the default. This is to provide compatibility with other systems where C# may not be the same language used for serialization as well as deserialization.

Since `IonAnnotateType` affects the type and all subtypes, there is a counter `IonDoNotAnnotateType` to explicitly turn this off. On `IonAnnotateType` one can set `ExcludeDescendants` which will mean this `Attribute` does not apply to descendants and finally one can set the `Prefix` to a string other than the C# namespace.

### Naming

By default, object property names are converted by changing the first character to be lowercase (camel case):

```c#
 {
   make: "Honda",
   model: "Civic",
   year: 2010
 }
```

This can be customized by specifying an option specified later, or by setting an `Attribute` on the property itself for the exact property name to be used:

```c#
public class Car
{
     [IonPropertyName("weightInKg")]
    public double Weight { get; init; }
}
```

If the specified property name conflicts with a field name of the same class, the property will get precedence over the field during serialiation and/or deserialization.

### Methods

By default, C# methods are ignored, however, provided the “get” signature is a no-argument method and the “set” signature is a one-argument `void` method, methods can be use to get and set properties. The Ion property name must be specified and will not be inferred from the method name. In the event of a naming conflict between the specified annotated Ion property name and a property or field name of the class (such as the below example), the annotated Ion property name will get precedence over the class's property or field name during serialization and/or deserialization.

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

### Fields

By default, C# fields are ignored. However, they can be annotated with an `IonField` attribute to be included or by an option specified later. Even private fields may be specified.

```c#
public class Car
{
    [IonField]
    private string color;
}
```

The Ion property name will be inferred from the field name and **preserved as is**. This can be customized with `IonPropertyName` `Attribute` as for properties.

### Construction

By default, C# objects are created using the `Activator.CreateInstance(Type)` method which calls the default constructor. If you want to supply a constructor yourself, you can markup a constructor with the `IonConstructor` `Attribute`. Additionally, the parameters to the constructor must be specified using the `IonPropertyName` `Attribute` again:

```c#
public class Wheel
{
    private string specification;

    [IonConstructor]
    public Wheel([IonPropertyName("specification")] string specification)
    {
         this.specification = specification;
    }
}
```

### Primitive type conversion

The Ion/.NET type conversion mapping is as per the following table:

|Ion Type	|C# Type	|
|---	|---	|
|[`null`](https://amzn.github.io/ion-docs/docs/spec.html#null)	|`null`	|
|[`bool`](https://amzn.github.io/ion-docs/docs/spec.html#bool)	|`bool`	|
|[`int`](https://amzn.github.io/ion-docs/docs/spec.html#int)	|`int`	|
|[`float`](https://amzn.github.io/ion-docs/docs/spec.html#real-numbers)	|`double`	|
|[`decimal`](https://amzn.github.io/ion-docs/docs/spec.html#real-numbers)	|`decimal `	|
|[`timestamp`](https://amzn.github.io/ion-docs/docs/spec.html#timestamp)	|`DateTime`	|
|[`string`](https://amzn.github.io/ion-docs/docs/spec.html#string)	|`string`	|
|[`symbol`](https://amzn.github.io/ion-docs/docs/spec.html#symbol)	|`SymbolToken` (The Ion type)	|
|[`blob`](https://amzn.github.io/ion-docs/docs/spec.html#blob)	|`ReadOnlySpan<byte>`	|
|[`clob`](https://amzn.github.io/ion-docs/docs/spec.html#clob)	|`StreamReader`	|
|`blob` (annotated)	|`Guid`	|
|[`struct`](https://amzn.github.io/ion-docs/docs/spec.html#struct)	|`object`	|
|[`list`](https://amzn.github.io/ion-docs/docs/spec.html#list)	|`IList`	|
|[`sexp`](https://amzn.github.io/ion-docs/docs/spec.html#sexp)	|`List<object>`	|

Note that this mapping is largely provided as is by the Amazon.Ion library.

### Serialization options

Ion Serialization can be customized in several ways. In order to do this, an `IonSerializationOptions` object can be passed to the `IonSerializer` object.

|Property	|Type	|Default	|Description	|
|---	|---	|---	|---	|
|`NamingConvention`	|`IonPropertyNamingConvention`	|`CamelCase`	|How fields and property names are converted to Ion. The other supplied options are `TitleCase` and `SnakeCase`.	|
|`Format`	|`IonSerializationFormat`	|`BINARY`	|The other options are `TEXT` and `PRETTY_TEXT`.	|
|`IonWriterFactory`	|`IonWriterFactory`	|`DefaultIonWriterFactory`	|Allows complete control over the creation of the `IonWriter`.	|
|`IonReaderFactory`	|`IonReaderFactory`	|`DefaultIonReaderFactory`	|Allows complete control over the creation of the `IonReader`.	|
|`MaxDepth`	|`int`	|64	|How far down a nested Ion struct to traverse on deserialization before stopping.	|
|`IncludeFields`	|`bool`	|FALSE	|Whether of not to include fields	|
|`IgnoreNulls`	|`bool`	|FALSE	|Whether or not to serialize null fields and properties	|
|`IgnoreReadOnlyFields`	|`bool`	|FALSE	|Whether or not to serialize readonly fields	|
|`IgnoreReadOnlyProperties`	|`bool`	|FALSE	|Whether or not to serialize readonly properties	|
|`PropertyNameCaseInsensitive`	|`bool`	|FALSE	|Whether or not property names are case insensitive	|
|`IgnoreDefaults`	|`bool`	|FALSE	|Whether or not to ignore fields and properties with default values	|
|`IncludeTypeInformation`	|`bool`	|FALSE	|Whether or not to include type information on all non-primitive fields.	|
|`TypeAnnotationPrefix`	|`TypeAnnotationPrefix`	|`FixedTypeAnnotationPrefix`	|By default the type prefix will be the .NET namespace of the type	|
|`TypeAnnotationName`	|`TypeAnnotationName`	|The .NET class name	|This allows one to specify how to produce an Ion annotation from a .NET type	|
|`TypeAnnotator`	|``TypeAnnotator``	|`DefaultTypeAnnotator`	|Allows complete control over the type annotation process with full access to the writer, options, and context.	|
|`PermissiveMode`	|`bool`	|FALSE	|In `PermissiveMode` the serializer will ignore as many errors as it can to deserialize objects. This is so that working with legacy data which might not be in the correct format is parseable, even if it's not perfect.	|
|`AnnotateGuids`	|bool	|FALSE	|When true, `Guid`s will be written as blobs annotated with "guid128", otherwise we will guess the blob is a `Guid` if the desired type is a `Guid`.	|
|`AnnotationConvention`	|`IonTypeAnnotationConvention`	|Fully qualified .NET namespace and class name	|This type will map from the .NET Type name to the Ion annotation. It is therefore possible to specify whatever type or scheme convention you desire. This allows one to specify how to convert an IonAnnotateType attribute into an Ion annotation string.	|
|`ObjectFactory`	|`ObjectFactory`	|`DefaultObjectFactory`	|The object used to construct types during deserialization.	|
|`AnnotatedTypeAssemblies`	|`string[]`	|Empty	|The list of assembly names to search when creating types from annotations.	|
|`IonSerializers`	|`Dictionary<Type, IonSerializer>`	|Empty	|A `Dictionary` of `IonSerializers` which specifies, for a key `Type` a custom Ion seralizer for that type.	|
|`AnnotatedIonSerializers`	|`Dictionary<string, IonSerializer>`	|Empty	|A `Dictionary` of `IonSerializers` which specifies, for the Ion type annotation, which custom Ion seralizer to use for that type.	|
|`CustomContext`	|`Dictionary<string, object>`	|Empty	|Custom arbitrary data that can be passed to the IonSerializer at serialization time which can then be used by custom Ion seriliazer to further customize behaviour.	|

### Customizing serialization

We will provide extension points to allow for arbitrarily fine-grained control over the serialization process. There are a few ways to specify this. To the `IonSerializer`, one can pass in options including a `Dictionary` of `Type` (and Ion type annotation) onto `IonSerializer`. This mapping will take precedence over the default mapping.

```c#
new IonSerializer(new IonSerializationOptions
{
     IonSerializers = new Dictionary<Type, IonSerializer>()
    {
        {typeof(string), new MyCustomStringIonSerializer()},
        {typeof(DateTime), new MyCustomDateTimeIonSerializerFactory()}
    },
    AnnotatedIonSerializers = new Dictionary<string, IonSerializer>()
    {
        {"complex-number", new MyComplexNumberIonSerializerFactory()}
    }
});
```

Note the two possible types here. One can either specify the `IonSerializer` directly which must be an instance of the interface:

```c#
public interface IonSerializer<T>
{
    void Serialize(IIonWriter writer, T item);
    T Deserialize(IIonReader reader);
}
```

Alternately, to get access to the Ion serialization options as well as the custom context passed in at serialization time, one should supply an `IonSerializerFactory`:

```c#
public interface IonSerializerFactory<T, TContext> 
    where TContext : IonSerializationContext
{
    public IonSerializer<T> create(
        IonSerializationOptions options, 
        TContext context);
}
```

This will then be called to create your `IonSerializer` and give you full access to the options as well as any other data you wish your serializer to know about.

In addition to the `Dictionary`, classes and properties can be marked up with `Attributes` specifying either the `IonSerializer` or `IonSerializerFactory` to be used for serialization.

```c#
[IonSerializer(typeof(MyIonCarSerializer))]
public class Car
{
    [IonSerializerFactory(typeof(MyIonEngineSerializerFactory))]
    public Engine engine { get; init; }
}
```

The `IonSerializerAttribute` must specify a `IonSerializer` with a no-arguments public default constructor. The `IonSerializerFactoryAttribute` must specify a `IonSerializerFactory` with a no-arguments public default constructor.

## A Note on Lossy Conversions

This library relies on Amazon.Ion to convert from Ion to the C# primitive types. So long as you use the library to serialize and deserialize Ion data, there should be no lossy conversion since the maximum precision of the .NET primitives has been captured. However, if the system which wrote the data is different than the system reading the data, a lossy conversion is possible. In this case, is it recommended to supply a custom converter or preserve the Ion bytes themselves.





