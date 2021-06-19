using System;

namespace Amazon.IonObjectMapper
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
        public Type Factory { get; init; }
        public Type Serializer { get; init; }
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
    /// Attribute to identify an Ion property getter method to be used
    /// during serialization of that Ion property.
    /// </summary>
    public class IonPropertyGetter : Attribute
    {
        /// <summary>
        /// IonPropertyGetter constructor.
        /// </summary>
        public IonPropertyGetter(string ionPropertyName)
        {
            this.IonPropertyName = ionPropertyName;
        }

        /// <summary>
        /// The name of the Ion property to be serialized with the getter method.
        /// </summary>
        public string IonPropertyName { get; }
    }

    /// <summary>
    /// Attribute to identify an Ion property setter method to be used
    /// during deserialization of that Ion property.
    /// </summary>
    public class IonPropertySetter : Attribute
    {
        /// <summary>
        /// IonPropertySetter constructor.
        /// </summary>
        public IonPropertySetter(string ionPropertyName)
        {
            IonPropertyName = ionPropertyName;
        }

        /// <summary>
        /// The name of the Ion property to be deserialized with the setter method.
        /// </summary>
        public string IonPropertyName { get; }
    }

    public class IonField : Attribute
    {
    }
}