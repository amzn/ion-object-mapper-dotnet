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

    public class IonIntSerializer : IonSerializer<int>
    {
        public override int Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        public override void Serialize(IIonWriter writer, int item)
        {
            writer.WriteInt(item);
        }
    }
}
