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

    /// <summary>
    /// Attribute to identify a .NET type to target during deserialization.
    /// </summary>
    public class IonAnnotateTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether any classes descending from the annotated class
        /// are excluded from the annotation.
        /// </summary>
        public bool ExcludeDescendants { get; set; }

        /// <summary>
        /// Gets or sets the .NET namespace of the annotated type.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the .NET class name of the annotated type.
        /// </summary>
        public string Name { get; set; }
    }
}
