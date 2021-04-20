using System;

namespace Amazon.Ion.ObjectMapper
{
    public class IonSerdeIgnore : Attribute
    {
    }

    public class IonSerializerAttribute : Attribute
    {

    }

    public class IonConstructor : Attribute
    {
    }

    public class IonPropertyName : Attribute
    {
        public IonPropertyName(string? v)
        {
            V = v;
        }

        public string V { get; }
    }
}