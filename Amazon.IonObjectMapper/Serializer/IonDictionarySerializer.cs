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
    using System.Collections;
    using System.Collections.Generic;
    using Amazon.IonDotnet;

    /// <summary>
    /// Ion Serializer for dictionary types.
    /// </summary>
    public class IonDictionarySerializer : IonSerializer<IDictionary>
    {
        private readonly IonSerializer serializer;
        private readonly Type valueType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonDictionarySerializer"/> class.
        /// </summary>
        ///
        /// <param name="ionSerializer">
        /// The Ion serializer to use for serializing and deserializing the values of the IDictionary.
        /// </param>
        /// <param name="valueType">The Type of the Value of the IDictionary.</param>
        public IonDictionarySerializer(IonSerializer ionSerializer, Type valueType)
        {
            this.serializer = ionSerializer;
            this.valueType = valueType;
        }

        /// <summary>
        /// Deserialize an Ion Struct into an IDictionary.
        /// </summary>
        ///
        /// <returns>A Dictionary of Key Type string and Value Type valueType.</returns>
        public override IDictionary Deserialize(IIonReader reader)
        {
            reader.StepIn();

            Type typedDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), this.valueType);
            var dictionary = (IDictionary)Activator.CreateInstance(typedDictionaryType);
            IonType currentType;
            while ((currentType = reader.MoveNext()) != IonType.None)
            {
                dictionary.Add(reader.CurrentFieldName, this.serializer.Deserialize(reader, this.valueType, currentType));
            }

            reader.StepOut();
            return dictionary;
        }

        /// <summary>
        /// Serializes an IDictionary into an Ion Struct where the Key is the struct field name
        /// and the Value is the struct field value.
        /// </summary>
        ///
        /// <param name="writer">The IIonWriter to use to write the Ion Struct.</param>
        /// <param name="item">The IDictionary to serialize into an Ion Struct.</param>
        public override void Serialize(IIonWriter writer, IDictionary item)
        {
            writer.StepIn(IonType.Struct);
            foreach (DictionaryEntry nameValuePair in item)
            {
                writer.SetFieldName((string)nameValuePair.Key);
                this.serializer.Serialize(writer, nameValuePair.Value);
            }

            writer.StepOut();
        }
    }
}
