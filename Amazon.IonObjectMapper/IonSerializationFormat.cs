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
    /// <summary>
    /// Format for serializing/deserializing Ion.
    /// </summary>
    public enum IonSerializationFormat
    {
        /// <summary>
        /// Binary format for serializing/deserializing Ion.
        /// </summary>
        BINARY,

        /// <summary>
        /// Text format for serializing/deserializing Ion.
        /// </summary>
        TEXT,

        /// <summary>
        /// Pretty text format for serializing/deserializing Ion.
        /// Pretty printing aids human readability.
        /// </summary>
        ///
        /// <example>
        /// Consider the following un-formatted text Ion:
        /// {level1: {level2: {level3: "foo"}, x: 2}, y: [a,b,c]}
        ///
        /// Pretty text would write that as the following:
        /// {
        ///     level1: {
        ///         level2: {
        ///             level3: "foo"
        ///         },
        ///         x: 2
        ///     },
        ///     y: [
        ///         a,
        ///         b,
        ///         c
        ///     ]
        /// }
        /// </example>
        PRETTY_TEXT,
    }
}
