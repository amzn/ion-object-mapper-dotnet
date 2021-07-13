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
    using System.Collections.Generic;

    /// <inheritdoc />
    public abstract class IonSerializerFactory<T> : IIonSerializerFactory
    {
        /// <summary>
        /// Create custom IonSerializer with customContext option.
        /// </summary>
        ///
        /// <param name="options">Serialization options for customizing serializer behavior.</param>
        /// <param name="customContext">
        /// A mapping between strings and arbitrary data that can be used to
        /// create a serializer with custom serialization/deserialization logic.
        /// </param>
        ///
        /// <returns>The customized IonSerializer.</returns>
        public abstract IonSerializer<T> Create(IonSerializationOptions options, Dictionary<string, object> customContext);

        /// <inheritdoc/>
        IIonSerializer IIonSerializerFactory.Create(IonSerializationOptions options, Dictionary<string, object> customContext)
        {
            return this.Create(options, customContext);
        }
    }
}
