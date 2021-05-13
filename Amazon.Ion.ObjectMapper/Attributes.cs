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
        public IonPropertyName(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class IonPropertyGetter : Attribute
    {
        public IonPropertyGetter(string fieldName)
        {
            this.FieldName = fieldName;
        }

        public string FieldName { get; }
    }
    
    public class IonPropertySetter : Attribute
    {
        public IonPropertySetter(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }

    public class IonField : Attribute
    {
    }
}