/*
 * Copyright (c) Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

namespace Amazon.IonObjectMapper
{
    using System;
    using Amazon.IonDotnet;

    /// <summary>
    /// Default type annotator.
    /// </summary>
    public class DefaultTypeAnnotator : ITypeAnnotator
    {
        /// <inheritdoc/>
        public void Apply(IonSerializationOptions options, IIonWriter writer, Type type)
        {
            IonAnnotateTypeAttribute annotateType = this.ShouldAnnotate(type, type);
            if (annotateType == null && options.IncludeTypeInformation)
            {
                // the serializer insists on type information being included
                annotateType = new IonAnnotateTypeAttribute();
            }

            if (annotateType != null)
            {
                annotateType.Prefix ??= options.TypeAnnotationPrefix.Apply(type);
                annotateType.Name ??= options.TypeAnnotationName.Apply(type);
                writer.AddTypeAnnotation(options.AnnotationConvention.Apply(annotateType, type));
            }
        }

        private IonAnnotateTypeAttribute ShouldAnnotate(Type targetType, Type currentType)
        {
            if (currentType == null)
            {
                // We did not find an annotation
                return null;
            }

            var doNotAnnotateAttributes = currentType.GetCustomAttributes(typeof(IonDoNotAnnotateTypeAttribute), false);
            if (doNotAnnotateAttributes.Length > 0)
            {
                if (((IonDoNotAnnotateTypeAttribute)doNotAnnotateAttributes[0]).ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return this.ShouldAnnotate(targetType, currentType.BaseType);
                }

                // do not annotate this type
                return null;
            }

            var annotateAttributes = currentType.GetCustomAttributes(typeof(IonAnnotateTypeAttribute), false);
            if (annotateAttributes.Length > 0)
            {
                var attribute = (IonAnnotateTypeAttribute)annotateAttributes[0];
                if (attribute.ExcludeDescendants && targetType != currentType)
                {
                    // this is not the target type, it is a parent, and this annotation does not apply to children, keep going up
                    return this.ShouldAnnotate(targetType, currentType.BaseType);
                }

                // annotate this type
                return attribute;
            }

            // did not find any annotations on this type
            return this.ShouldAnnotate(targetType, currentType.BaseType);
        }
    }
}
