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
    /// Generic abstract class for an Ion Serializer.
    /// </summary>
    ///
    /// <typeparam name="T">The data type being serialized/deserialized.</typeparam>
    public abstract class IonSerializer<T> : IIonSerializer
    {
        /// <summary>
        /// Serialize value of type T.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The value to serialize.</param>
        /// <typeparam name="T">The data type being serialized.</typeparam>
        public abstract void Serialize(IIonWriter writer, T item);

        /// <summary>
        /// Deserialize value of type T.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        /// <typeparam name="T">The data type being deserialized.</typeparam>
        ///
        /// <returns>The deserialized value.</returns>
        public abstract T Deserialize(IIonReader reader);

        /// <inheritdoc/>
        void IIonSerializer.Serialize(IIonWriter writer, object item)
        {
            this.Serialize(writer, (T)item);
        }

        /// <inheritdoc/>
        object IIonSerializer.Deserialize(IIonReader reader)
        {
            return this.Deserialize(reader);
        }
    }
}
