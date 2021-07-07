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
    using System.Collections.Generic;
    using Amazon.IonDotnet;

    /// <summary>
    /// Serializer for serializing and deserializing Guid values.
    /// </summary>
    public class IonGuidSerializer : IonSerializer<Guid>
    {
        internal static readonly string ANNOTATION = "guid128";
        private readonly IonSerializationOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonGuidSerializer"/> class.
        /// </summary>
        ///
        /// <param name="options">Serialization options for customizing serializer behavior.</param>
        public IonGuidSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Deserialize Guid value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader to be used for deserialization.</param>
        ///
        /// <returns>The deserialized Guid value.</returns>
        public override Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return new Guid(blob);
        }

        /// <summary>
        /// Serialize Guid value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer to be used for serialization.</param>
        /// <param name="item">The Guid value to serialize.</param>
        public override void Serialize(IIonWriter writer, Guid item)
        {
            if (this.options.AnnotateGuids)
            {
                writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            }

            writer.WriteBlob(item.ToByteArray());
        }
    }
}
