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
    /// Attribute to identify the Ion field name to be used during serialization and/or
    /// deserialization of a .NET property annotated with this attribute.
    /// </summary>
    public class IonPropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IonPropertyNameAttribute"/> class.
        /// </summary>
        ///
        /// <param name="ionPropertyName">The name of the Ion property.</param>
        public IonPropertyNameAttribute(string ionPropertyName)
        {
            this.Name = ionPropertyName;
        }

        /// <summary>
        /// Gets the Ion field name to be used instead of the .NET property's name.
        /// </summary>
        public string Name { get; }
    }
}
