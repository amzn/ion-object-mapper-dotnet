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

    /// <summary>
    /// Attribute to identify a field/property's getter method to be used
    /// during serialization of that field/property.
    /// </summary>
    public class IonPropertyGetter : Attribute
    {
        /// <summary>
        /// Attribute constructor.
        /// </summary>
        public IonPropertyGetter(string fieldName)
        {
            this.FieldName = fieldName;
        }

        /// <summary>
        /// The name of the field to be serialized with the getter method.
        /// </summary>
        public string FieldName { get; }
    }

    /// <summary>
    /// Attribute to identify a field/property's setter method to be used
    /// during deserialization of that field/property.
    /// </summary>
    public class IonPropertySetter : Attribute
    {
        public IonPropertySetter(string fieldName)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// The name of the field to be deserialized with the setter method.
        /// </summary>
        public string FieldName { get; }
    }

    public class IonField : Attribute
    {
    }
}