using System;

namespace Amazon.Ion.ObjectMapper
{
    public class IonIgnore : Attribute
    {
    }

    public class IonAnnotateType : Attribute
    {
        public bool ExcludeDescendants { get; init; }
        public string Prefix { get; set; }
        public string Name { get; set; }
    }

    public class IonDoNotAnnotateType : Attribute
    {
        public bool ExcludeDescendants { get; init; }
    }

    public class IonSerializerAttribute : Attribute
    {
    }

    public class IonConstructor : Attribute
    {
    }

    public class IonPropertyName : Attribute
    {
        public IonPropertyName(string v)
        {
            V = v;
        }

        public string V { get; }
    }
}