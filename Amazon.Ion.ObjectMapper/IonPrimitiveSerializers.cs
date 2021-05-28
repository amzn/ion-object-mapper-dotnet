using System;
using System.Collections.Generic;
using System.Text;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonNullSerializer : IonSerializer<object>
    {
        public object Deserialize(IIonReader reader)
        {
            return null;
        }

        public void Serialize(IIonWriter writer, object item)
        {
            writer.WriteNull();
        }
    }

    public class IonByteArraySerializer : IonSerializer<byte[]>
    {
        public byte[] Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return blob;
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteBlob(item);
        }
    }
    public class IonStringSerializer : IonSerializer<string>
    {
        public string Deserialize(IIonReader reader)
        {
            return reader.StringValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteString(item);
        }
    }

    public class IonIntSerializer : IonSerializer<int>
    {
        public int Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteInt(item);
        }
    }

    public class IonLongSerializer : IonSerializer<long>
    {
        internal static readonly string ANNOTATION = "numeric.int32";

        public long Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteInt(item);
        }
    }
    public class IonBooleanSerializer : IonSerializer<bool>
    {
        public bool Deserialize(IIonReader reader)
        {
            return reader.BoolValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteBool(item);
        }
    }

    public class IonDoubleSerializer : IonSerializer<double>
    {
        public double Deserialize(IIonReader reader)
        {
            return reader.DoubleValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteFloat(item);
        }
    }

    public class IonDecimalSerializer : IonSerializer<decimal>
    {
        internal static readonly string ANNOTATION = "numeric.decimal128";

        public decimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue().ToDecimal();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteDecimal(item);
        }
    }

    public class IonBigDecimalSerializer : IonSerializer<BigDecimal>
    {
        public BigDecimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteDecimal(item);
        }
    }

    public class IonFloatSerializer : IonSerializer<float>
    {
        internal static readonly string ANNOTATION = "numeric.float32";

        public float Deserialize(IIonReader reader)
        {
            return Convert.ToSingle(reader.DoubleValue());
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteFloat(item);
        }
    }

    public class IonDateTimeSerializer : IonSerializer<DateTime>
    {
        public DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue;
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteTimestamp(new Timestamp(item));
        }
    }

    public class IonSymbolSerializer : IonSerializer<SymbolToken>
    {
        public SymbolToken Deserialize(IIonReader reader)
        {
            return reader.SymbolValue();
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteSymbolToken(item);
        }
    }

    public class IonClobSerializer : IonSerializer<string>
    {
        public string Deserialize(IIonReader reader)
        {
            byte[] clob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(clob);
            return Encoding.UTF8.GetString(clob);
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            writer.WriteClob(Encoding.UTF8.GetBytes(item));
        }
    }

    public class IonGuidSerializer : IonSerializer<Guid>
    {
        internal static readonly string ANNOTATION = "guid128";
        private readonly bool annotateGuids;

        public IonGuidSerializer(bool annotateGuids)
        {
            this.annotateGuids = annotateGuids;
        }

        public Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return new Guid(blob);
        }

        public void Serialize(IIonWriter writer, dynamic item)
        {
            if (annotateGuids) {
                writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            }
            writer.WriteBlob(item.ToByteArray());
        }
    }
}