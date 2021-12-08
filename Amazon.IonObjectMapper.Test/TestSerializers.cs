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

namespace Amazon.IonObjectMapper.Test
{
    using System;
    using Amazon.IonDotnet;

    public class NegationBoolIonSerializer : IonSerializer<bool>
    {
        public override bool Deserialize(IIonReader reader)
        {
            return !reader.BoolValue();
        }

        public override void Serialize(IIonWriter writer, bool item)
        {
            writer.WriteBool(!item);
        }
    }

    public class ZeroByteArrayIonSerializer : IonSerializer<byte[]>
    {
        public override byte[] Deserialize(IIonReader reader)
        {
            return new byte[reader.GetLobByteSize()];
        }

        public override void Serialize(IIonWriter writer, byte[] item)
        {
            writer.WriteBlob(new byte[item.Length]);
        }
    }

    public class UpperCaseStringIonSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            return reader.StringValue().ToUpper();
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item.ToUpper());
        }
    }

    public class NegativeIntIonSerializer : IonSerializer<int>
    {
        public override int Deserialize(IIonReader reader)
        {
            return -reader.IntValue();
        }

        public override void Serialize(IIonWriter writer, int item)
        {
            writer.WriteInt(-item);
        }
    }

    public class NegativeLongIonSerializer : IonSerializer<long>
    {
        public override long Deserialize(IIonReader reader)
        {
            return -reader.LongValue();
        }

        public override void Serialize(IIonWriter writer, long item)
        {
            writer.WriteInt(-item);
        }
    }

    public class NegativeFloatIonSerializer : IonSerializer<float>
    {
        public override float Deserialize(IIonReader reader)
        {
            return -Convert.ToSingle(reader.DoubleValue());
        }

        public override void Serialize(IIonWriter writer, float item)
        {
            writer.WriteFloat(-item);
        }
    }

    public class NegativeDoubleIonSerializer : IonSerializer<double>
    {
        public override double Deserialize(IIonReader reader)
        {
            return -reader.DoubleValue();
        }

        public override void Serialize(IIonWriter writer, double item)
        {
            writer.WriteFloat(-item);
        }
    }

    public class NegativeDecimalIonSerializer : IonSerializer<decimal>
    {
        public override decimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue().ToDecimal();
        }

        public override void Serialize(IIonWriter writer, decimal item)
        {
            writer.WriteDecimal(-item);
        }
    }

    public class NegativeBigDecimalIonSerializer : IonSerializer<BigDecimal>
    {
        public override BigDecimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue();
        }

        public override void Serialize(IIonWriter writer, BigDecimal item)
        {
            writer.WriteDecimal(-item);
        }
    }

    public class UpperCaseSymbolIonSerializer : IonSerializer<SymbolToken>
    {
        public override SymbolToken Deserialize(IIonReader reader)
        {
            var token = reader.SymbolValue();
            return new SymbolToken(token.Text.ToUpper(), token.Sid);
        }

        public override void Serialize(IIonWriter writer, SymbolToken item)
        {
            var token = new SymbolToken(item.Text.ToUpper(), item.Sid);
            writer.WriteSymbolToken(token);
        }
    }

    public class NextDayDateTimeIonSerializer : IonSerializer<DateTime>
    {
        public override DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue.AddDays(1);
        }

        public override void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item.AddDays(1)));
        }
    }

    public class ZeroGuidIonSerializer : IonSerializer<Guid>
    {
        public override Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            return new Guid(blob);
        }

        public override void Serialize(IIonWriter writer, Guid item)
        {
            writer.WriteBlob(new byte[item.ToByteArray().Length]);
        }
    }

    public class SupraManufacturerDeserializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            return "BMW";
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item);
        }
    }

    public class SupraManufacturerSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            return reader.StringValue();
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString("BMW");
        }
    }
}
