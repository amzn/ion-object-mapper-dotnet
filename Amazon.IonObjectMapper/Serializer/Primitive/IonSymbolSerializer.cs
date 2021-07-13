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
    /// Serializer for serializing and deserializing Symbol Token values.
    /// </summary>
    public class IonSymbolSerializer : IonSerializer<SymbolToken>
    {
        /// <summary>
        /// Deserialize Symbol Token value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        ///
        /// <returns>The deserialized Symbol Token value.</returns>
        public override SymbolToken Deserialize(IIonReader reader)
        {
            return reader.SymbolValue();
        }

        /// <summary>
        /// Serialize Symbol Token value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The Symbol Token value to serialize.</param>
        public override void Serialize(IIonWriter writer, SymbolToken item)
        {
            writer.WriteSymbolToken(item);
        }
    }
}
