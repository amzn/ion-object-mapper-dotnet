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
    /// <summary>
    /// Interface for property naming convention.
    /// </summary>
    public interface IIonPropertyNamingConvention
    {
        /// <summary>
        /// Convert name back to original .NET property name.
        /// </summary>
        ///
        /// <param name="s">The specified naming convention version of the .NET property name.</param>
        ///
        /// <returns>The original .NET property name.</returns>
        public string ToProperty(string s);

        /// <summary>
        /// Convert .NET property name to some naming convention.
        /// </summary>
        ///
        /// <param name="s">The original .NET property name.</param>
        ///
        /// <returns>The specified naming convention version of the .NET property name.</returns>
        public string FromProperty(string s);
    }
}
