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
    using System.IO;
    using Amazon.IonDotnet;

    /// <summary>
    /// Interface for Ion Reader Factory.
    /// </summary>
    public interface IIonReaderFactory
    {
        /// <summary>
        /// Create an Ion Reader.
        /// </summary>
        ///
        /// <param name="stream">The stream to be read by the Ion reader.</param>
        ///
        /// <returns>The created Ion reader.</returns>
        IIonReader Create(Stream stream);
    }
}
