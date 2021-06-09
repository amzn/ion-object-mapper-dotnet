using System;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper.Test
{
    public class NegationBoolIonSerializer : IonSerializer<bool>
    {
        public bool Deserialize(IIonReader reader)
        {
            return !reader.BoolValue();
        }

        public void Serialize(IIonWriter writer, bool item)
        {
            writer.WriteBool(!item);
        }
    }

    public class ZeroByteArrayIonSerializer : IonSerializer<byte[]>
    {
        public byte[] Deserialize(IIonReader reader)
        {
            return new byte[reader.GetLobByteSize()];
        }

        public void Serialize(IIonWriter writer, byte[] item)
        {
            writer.WriteBlob(new byte[item.Length]);
        }
    }

    public class UpperCaseStringIonSerializer : IonSerializer<string>
        {
        public string Deserialize(IIonReader reader)
        {
            return reader.StringValue().ToUpper();
        }

        public void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item.ToUpper());
        }
    }

    public class NegativeIntIonSerializer : IonSerializer<int>
    {
        public int Deserialize(IIonReader reader)
        {
            return -reader.IntValue();
        }

        public void Serialize(IIonWriter writer, int item)
        {
            writer.WriteInt(-item);
        }
    }

    public class NegativeLongIonSerializer : IonSerializer<long>
    {
        public long Deserialize(IIonReader reader)
        {
            return -reader.LongValue();
        }

        public void Serialize(IIonWriter writer, long item)
        {
            writer.WriteInt(-item);
        }
    }

    public class NegativeFloatIonSerializer : IonSerializer<float>
    {
        public float Deserialize(IIonReader reader)
        {
            return -Convert.ToSingle(reader.DoubleValue());
        }

        public void Serialize(IIonWriter writer, float item)
        {
            writer.WriteFloat(-item);
        }
    }

    public class NegativeDoubleIonSerializer : IonSerializer<double>
    {
        public double Deserialize(IIonReader reader)
        {
            return -reader.DoubleValue();
        }

        public void Serialize(IIonWriter writer, double item)
        {
            writer.WriteFloat(-item);
        }
    }

    public class NegativeDecimalIonSerializer : IonSerializer<decimal>
    {
        public decimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue().ToDecimal();
        }

        public void Serialize(IIonWriter writer, decimal item)
        {
            writer.WriteDecimal(-item);
        }
    }

    public class NegativeBigDecimalIonSerializer : IonSerializer<BigDecimal>
    {
        public BigDecimal Deserialize(IIonReader reader)
        {
            return -reader.DecimalValue();
        }

        public void Serialize(IIonWriter writer, BigDecimal item)
        {
            writer.WriteDecimal(-item);
        }
    }

    public class UpperCaseSymbolIonSerializer : IonSerializer<SymbolToken>
    {
        public SymbolToken Deserialize(IIonReader reader)
        {
            var token = reader.SymbolValue();
            return new SymbolToken(token.Text.ToUpper(), token.Sid);
        }

        public void Serialize(IIonWriter writer, SymbolToken item)
        {
            var token = new SymbolToken(item.Text.ToUpper(), item.Sid);
            writer.WriteSymbolToken(token);
        }
    }

    public class NextDayDateTimeIonSerializer : IonSerializer<DateTime>
    {
        public DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue.AddDays(1);
        }

        public void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item.AddDays(1)));
        }
    }

    public class ZeroGuidIonSerializer : IonSerializer<Guid>
    {
        public Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            return new Guid(blob);
        }

        public void Serialize(IIonWriter writer, Guid item)
        {
            writer.WriteBlob(new byte[item.ToByteArray().Length]);
        }
    }
}
