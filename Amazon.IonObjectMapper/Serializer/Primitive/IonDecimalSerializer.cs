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
    using Amazon.IonDotnet;

    /// <summary>
    /// Serializer for serializing and deserializing decimal values.
    /// </summary>
    public class IonDecimalSerializer : IonSerializer<decimal>
    {
        /// <summary>
        /// Ion annotation to distinguish decimals from big decimals.
        /// </summary>
        internal static readonly string ANNOTATION = "numeric.decimal128";

        /// <summary>
        /// Deserialize decimal value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        ///
        /// <returns>The deserialized decimal value.</returns>
        public override decimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue().ToDecimal();
        }

        /// <summary>
        /// Serialize decimal value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The decimal value to serialize.</param>
        public override void Serialize(IIonWriter writer, decimal item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteDecimal(item);
        }
    }
}
