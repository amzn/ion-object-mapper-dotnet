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
    /// Attribute to identify a .NET type that should not be annotated
    /// even if a parent class is annotated.
    /// </summary>
    public class IonDoNotAnnotateTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether any classes descending from the annotated class
        /// are excluded from the IonAnnotateTypeAttribute annotation.
        /// </summary>
        public bool ExcludeDescendants { get; set; }
    }
}
