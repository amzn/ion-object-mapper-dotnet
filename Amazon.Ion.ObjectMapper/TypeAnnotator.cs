using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;

namespace Amazon.Ion.ObjectMapper
{
    public interface TypeAnnotationPrefix
    {
        public string Apply(Type type);
    }

    public class NamespaceTypeAnnotationPrefix : TypeAnnotationPrefix
    {
        public string Apply(Type type)
        {
            return type.Namespace;
        }
    }

    public class FixedTypeAnnotationPrefix : TypeAnnotationPrefix
    {
        private readonly string prefix;

        public FixedTypeAnnotationPrefix(string prefix)
        {
            this.prefix = prefix;
        }

        public string Apply(Type type)
        {
            return prefix;
        }
    }

    public interface TypeAnnotator
    {
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type);
    }

    public class DefaultTypeAnnotator : TypeAnnotator
    {
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type)
        {
            IonAnnotateType annotateType = ShouldAnnotate(type, type);
            if (annotateType == null && options.IncludeTypeInformation) 
            {
                // the serializer insists on type information being included
                annotateType = new IonAnnotateType();
            }

            if (annotateType != null)
            {
                var prefix = annotateType.Prefix != null ? annotateType.Prefix : options.TypeAnnotationPrefix.Apply(type);
                var name = annotateType.Name != null ? annotateType.Name : type.Name;
                writer.AddTypeAnnotation(prefix + "." + name);
            }
        }

        private IonAnnotateType ShouldAnnotate(Type targetType, Type currentType) 
        {
            if (currentType == null)
            {
                // We did not find an annotation
                return null;
            }
            var doNotAnnotateAttributes = currentType.GetCustomAttributes(typeof(IonDoNotAnnotateType), false);
            if (doNotAnnotateAttributes.Length > 0) 
            {
                if (((IonDoNotAnnotateType)doNotAnnotateAttributes[0]).ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return ShouldAnnotate(targetType, currentType.BaseType);
                }
                // do not annotate this type
                return null;
            }
            var annotateAttributes = currentType.GetCustomAttributes(typeof(IonAnnotateType), false);
            if (annotateAttributes.Length > 0) 
            {
                var attribute = (IonAnnotateType)annotateAttributes[0];
                if (attribute.ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return ShouldAnnotate(targetType, currentType.BaseType);
                }
                // annotate this type
                return attribute;
            }
            // did not find any annotations on this type
            return ShouldAnnotate(targetType, currentType.BaseType);
        }
    }
}
