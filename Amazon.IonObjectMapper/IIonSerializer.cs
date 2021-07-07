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
    using Amazon.IonDotnet;

    /// <summary>
    /// Interface for Ion Serializer.
    /// </summary>
    public interface IIonSerializer
    {
        /// <summary>
        /// Serialize value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The value to serialize.</param>
        void Serialize(IIonWriter writer, object item);

        /// <summary>
        /// Deserialize value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        ///
        /// <returns>The deserialized value.</returns>
        object Deserialize(IIonReader reader);
    }
}
