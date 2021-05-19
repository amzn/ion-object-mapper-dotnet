/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace Amazon.Ion.ObjectMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Amazon.IonDotnet;

    public class IonListSerializer : IonSerializer<IList>
    {
        private readonly IonSerializer serializer;
        private readonly Type listType;
        private readonly Type elementType;
        private readonly bool isGenericList;

        public IonListSerializer(IonSerializer serializer, Type listType, Type elementType)
        {
            this.serializer = serializer;
            this.listType = listType;
            this.elementType = elementType;
            this.isGenericList = true;
        }

        public IonListSerializer(IonSerializer serializer, Type listType)
        {
            this.serializer = serializer;
            this.listType = listType;
            this.isGenericList = false;
        }

        public IList Deserialize(IIonReader reader)
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
                    typedList = (IList)Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(this.elementType));
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

            throw new NotSupportedException(
                $"Don't know how to make a list of type {this.listType} with element type {this.elementType}");
        }

        public void Serialize(IIonWriter writer, IList item)
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
