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
    using System.Collections.Generic;
    using static Amazon.IonObjectMapper.IonSerializationFormat;

    /// <summary>
    /// Serialization options.
    /// </summary>
    public class IonSerializationOptions
    {
        /// <summary>
        /// Gets option to specify the .NET property naming convention.
        /// </summary>
        public IIonPropertyNamingConvention NamingConvention { get; set; } = new CamelCaseNamingConvention();

        /// <summary>
        /// Gets option to specify the serialization format.
        /// </summary>
        public IonSerializationFormat Format { get; set; } = TEXT;

        /// <summary>
        /// Gets option to specify the maximum deserialization depth.
        /// </summary>
        public int MaxDepth { get; set; } = 64;

        /// <summary>
        /// Gets a value indicating whether to annotate Guids.
        /// </summary>
        public bool AnnotateGuids { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether all .NET fields should be included
        /// during serialization and/or deserialization.
        /// </summary>
        public bool IncludeFields { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether null values should be ignored during serialization/deserialization.
        /// </summary>
        public bool IgnoreNulls { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether readonly fields should be ignored during serialization/deserialization.
        /// </summary>
        public bool IgnoreReadOnlyFields { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether readonly properties should be ignored during serialization/deserialization.
        /// </summary>
        public bool IgnoreReadOnlyProperties { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether property names should be treated as case insensitive.
        /// </summary>
        public bool PropertyNameCaseInsensitive { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether default values should be ignored during serialization/deserialization.
        /// </summary>
        public bool IgnoreDefaults { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether type information on all non-primitive fields should be included.
        /// </summary>
        public bool IncludeTypeInformation { get; set; } = false;

        /// <summary>
        /// Gets option to specify the type annotation prefix.
        /// </summary>
        public ITypeAnnotationPrefix TypeAnnotationPrefix { get; set; } = new NamespaceTypeAnnotationPrefix();

        /// <summary>
        /// Gets option to specify the type annotation .NET class name.
        /// </summary>
        public ITypeAnnotationName TypeAnnotationName { get; set; } = new ClassNameTypeAnnotationName();

        /// <summary>
        /// Gets option to specify the mapping between the .NET type name and the Ion annotation.
        /// </summary>
        public IAnnotationConvention AnnotationConvention { get; set; } = new DefaultAnnotationConvention();

        /// <summary>
        /// Gets option to specify how a type is annotated during serialization.
        /// </summary>
        public ITypeAnnotator TypeAnnotator { get; set; } = new DefaultTypeAnnotator();

        /// <summary>
        /// Gets option to specify how Ion Readers should be built.
        /// </summary>
        public IIonReaderFactory ReaderFactory { get; set; } = new DefaultIonReaderFactory();

        /// <summary>
        /// Gets option to specify how Ion Writers should be built.
        /// </summary>
        public IIonWriterFactory WriterFactory { get; set; } = new DefaultIonWriterFactory();

        /// <summary>
        /// Gets option to specify how objects should be built during deserialization.
        /// </summary>
        public IObjectFactory ObjectFactory { get; set; } = new DefaultObjectFactory();

        /// <summary>
        /// Gets option to specify the list of assembly names to search when creating types from annotations.
        /// </summary>
        public IEnumerable<string> AnnotatedTypeAssemblies { get; set; }

        /// <summary>
        /// Gets option to specify custom serializers for any given type.
        /// </summary>
        public Dictionary<Type, IIonSerializer> IonSerializers { get; set; }

        /// <summary>
        /// Gets option to specify a custom context object to be used by custom serializers
        /// to further customize behavior.
        /// </summary>
        public Dictionary<string, object> CustomContext { get; set; }
    }
}
