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
    /// Attribute to identify a custom Ion Serializer to be used to serialize
    /// and deserialize instances of the class annotated with this attribute.
    /// </summary>
    public class IonSerializerAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the IonSerializer Attribute to be used
        /// to create IonSerializerFactory with custom context.
        /// </summary>
        public Type Factory { get; set; }

        /// <summary>
        /// Gets the name of the IonSerializer Attribute to be used
        /// to create custom IonSerializer.
        /// </summary>
        public Type Serializer { get; set; }
    }
}
