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
    /// Interface for Object Factory.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Create new instance of an object.
        /// </summary>
        ///
        /// <param name="options">Serialization options for customizing serializer behavior.</param>
        /// <param name="reader">The Ion reader to be used for object creation.</param>
        /// <param name="targetType">The type of the created object.</param>
        ///
        /// <returns>A new instance of a specified type.</returns>
        object Create(IonSerializationOptions options, IIonReader reader, Type targetType);
    }
}
