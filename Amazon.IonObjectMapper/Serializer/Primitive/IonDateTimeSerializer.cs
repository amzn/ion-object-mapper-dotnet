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
    /// Serializer for serializing and deserializing date time values.
    /// </summary>
    public class IonDateTimeSerializer : IonSerializer<DateTime>
    {
        /// <summary>
        /// Deserialize date time value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized date time value.</returns>
        public override DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue;
        }

        /// <summary>
        /// Serialize date time value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The date time value to serialize.</param>
        public override void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item));
        }
    }
}
