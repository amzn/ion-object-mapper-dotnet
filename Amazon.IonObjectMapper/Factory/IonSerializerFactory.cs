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

    /// <summary>
    /// This abstract class is to implement IIonSerializerFactory interface.
    /// </summary>
    public abstract class IonSerializerFactory<T> : IIonSerializerFactory
    {
        /// <summary>
        /// Create custom IonSerializer with customContext option.
        /// </summary>
        /// <param name="options">
        /// The IonSerializationOptions is an object that can be passed to the IonSerializer object
        /// and determined the way to customize the IonSerializer.
        /// </param>
        /// <param name="customContext">
        /// The customContext is one option to create IonSerializer with custom arbitrary data.
        /// A Dictionary of Key Type string is to map to any customized objects
        /// and Value Type object is to custom any serialize/deserialize logic.
        /// </param>
        /// <returns>
        /// Customized IonSerializer.
        /// </returns>
        public abstract IonSerializer<T> Create(IonSerializationOptions options, Dictionary<string, object> customContext);

        IIonSerializer IIonSerializerFactory.Create(IonSerializationOptions options, Dictionary<string, object> customContext)
        {
            return this.Create(options, customContext);
        }
    }
}
