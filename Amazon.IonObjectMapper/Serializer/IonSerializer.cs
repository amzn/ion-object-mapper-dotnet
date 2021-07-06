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
    using Amazon.IonDotnet;

    public abstract class IonSerializer<T> : IIonSerializer
    {
        public abstract void Serialize(IIonWriter writer, T item);

        public abstract T Deserialize(IIonReader reader);

        void IIonSerializer.Serialize(IIonWriter writer, object item)
        {
            this.Serialize(writer, (T)item);
        }

        object IIonSerializer.Deserialize(IIonReader reader)
        {
            return this.Deserialize(reader);
        }
    }
}
