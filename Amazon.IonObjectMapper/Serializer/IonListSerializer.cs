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
    /// Ion Serializer for list types.
    /// </summary>
    public class IonListSerializer : IonSerializer<IList>
    {
        private readonly IonSerializer serializer;
        private readonly Type listType;
        private readonly Type elementType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonListSerializer"/> class.
        /// </summary>
        ///
        /// <param name="serializer">Serializer used for serializing/deserializing items in list.</param>
        /// <param name="listType">Type of list.</param>
        /// <param name="elementType">Type of items in list.</param>
        public IonListSerializer(IonSerializer serializer, Type listType, Type elementType)
        {
            this.serializer = serializer;
            this.listType = listType;
            this.elementType = elementType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonListSerializer"/> class.
        /// </summary>
        ///
        /// <param name="serializer">Serializer used for serializing/deserializing items in list.</param>
        /// <param name="listType">Type of list.</param>
        public IonListSerializer(IonSerializer serializer, Type listType)
        {
            this.serializer = serializer;
            this.listType = listType;
        }

        /// <inheritdoc/>
        public override IList Deserialize(IIonReader reader)
        {
            reader.StepIn();
            var list = new ArrayList();
            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                list.Add(this.serializer.Deserialize(reader, this.elementType, ionType));
            }

            reader.StepOut();

            if (this.listType.IsArray)
            {
                var typedArray = Array.CreateInstance(this.elementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    typedArray.SetValue(list[i], i);
                }

                return typedArray;
            }

            if (this.listType is IEnumerable || this.listType is object)
            {
                IList typedList;
                if (this.listType.IsGenericType)
                {
                    typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(this.elementType));
                }
                else
                {
                    typedList = new ArrayList();
                }

                foreach (var element in list)
                {
                    typedList.Add(element);
                }

                return typedList;
            }

            throw new NotSupportedException($"Don't know how to make a list of type {this.listType} with element type {this.elementType}");
        }

        /// <inheritdoc/>
        public override void Serialize(IIonWriter writer, IList item)
        {
            writer.StepIn(IonType.List);
            foreach (var i in item)
            {
                this.serializer.Serialize(writer, i);
            }

            writer.StepOut();
        }
    }
}
