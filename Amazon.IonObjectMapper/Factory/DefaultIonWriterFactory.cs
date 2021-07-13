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
    using System.IO;
    using Amazon.IonDotnet;
    using Amazon.IonDotnet.Builders;
    using static Amazon.IonObjectMapper.IonSerializationFormat;

    /// <summary>
    /// Default Ion Writer Factory.
    /// </summary>
    public class DefaultIonWriterFactory : IIonWriterFactory
    {
        private readonly IonSerializationFormat format = TEXT;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIonWriterFactory"/> class.
        /// </summary>
        public DefaultIonWriterFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIonWriterFactory"/> class.
        /// </summary>
        ///
        /// <param name="format">Serialization format.</param>
        public DefaultIonWriterFactory(IonSerializationFormat format)
        {
            this.format = format;
        }

        /// <inheritdoc/>
        public IIonWriter Create(Stream stream)
        {
            return this.format switch
            {
                BINARY => IonBinaryWriterBuilder.Build(stream),
                TEXT => IonTextWriterBuilder.Build(new StreamWriter(stream)),
                PRETTY_TEXT => IonTextWriterBuilder.Build(
                    new StreamWriter(stream),
                    new IonTextOptions { PrettyPrint = true }),
                _ => throw new InvalidOperationException($"Format {this.format} not supported"),
            };
        }
    }
}
