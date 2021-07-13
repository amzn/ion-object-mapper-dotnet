﻿/*
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
    using System.Text;
    using Amazon.IonDotnet;

    /// <summary>
    /// Serializer for serializing and deserializing CLOB values.
    /// </summary>
    public class IonClobSerializer : IonSerializer<string>
    {
        /// <summary>
        /// Deserialize CLOB value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        ///
        /// <returns>The deserialized CLOB value.</returns>
        public override string Deserialize(IIonReader reader)
        {
            byte[] clob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(clob);
            return Encoding.UTF8.GetString(clob);
        }

        /// <summary>
        /// Serialize CLOB value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The CLOB value to serialize.</param>
        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteClob(Encoding.UTF8.GetBytes(item));
        }
    }
}
