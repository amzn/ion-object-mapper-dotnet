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

    public class IonSerializationOptions
    {
        public IIonPropertyNamingConvention NamingConvention { get; init; } = new CamelCaseNamingConvention();

        public IonSerializationFormat Format { get; init; } = TEXT;

        public int MaxDepth { get; init; } = 64;

        public bool AnnotateGuids { get; init; } = false;

        public bool IncludeFields { get; init; } = false;

        public bool IgnoreNulls { get; init; } = false;

        public bool IgnoreReadOnlyFields { get; init; } = false;

        public bool IgnoreReadOnlyProperties { get; init; } = false;

        public bool PropertyNameCaseInsensitive { get; init; } = false;

        public bool IgnoreDefaults { get; init; } = false;

        public bool IncludeTypeInformation { get; init; } = false;

        public ITypeAnnotationPrefix TypeAnnotationPrefix { get; init; } = new NamespaceTypeAnnotationPrefix();

        public ITypeAnnotationName TypeAnnotationName { get; init; } = new ClassNameTypeAnnotationName();

        public IAnnotationConvention AnnotationConvention { get; init; } = new DefaultAnnotationConvention();

        public ITypeAnnotator TypeAnnotator { get; init; } = new DefaultTypeAnnotator();

        public IIonReaderFactory ReaderFactory { get; init; } = new DefaultIonReaderFactory();

        public IIonWriterFactory WriterFactory { get; init; } = new DefaultIonWriterFactory();

        public IObjectFactory ObjectFactory { get; init; } = new DefaultObjectFactory();

        public IEnumerable<string> AnnotatedTypeAssemblies { get; init; }

        public Dictionary<Type, IIonSerializer> IonSerializers { get; init; }

        public Dictionary<string, object> CustomContext { get; init; }
    }
}
