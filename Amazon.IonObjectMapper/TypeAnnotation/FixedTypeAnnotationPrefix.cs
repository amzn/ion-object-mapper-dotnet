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

    /// <summary>
    /// Fixed type annotation prefix.
    /// </summary>
    public class FixedTypeAnnotationPrefix : ITypeAnnotationPrefix
    {
        private readonly string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedTypeAnnotationPrefix"/> class.
        /// </summary>
        ///
        /// <param name="prefix">The prefix used for annotation.</param>
        public FixedTypeAnnotationPrefix(string prefix)
        {
            this.prefix = prefix;
        }

        /// <inheritdoc/>
        public string Apply(Type type)
        {
            return this.prefix;
        }
    }
}
