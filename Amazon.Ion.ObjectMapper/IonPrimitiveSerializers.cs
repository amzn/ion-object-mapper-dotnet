using System;
using System.Collections.Generic; 
using System.Numerics;
using System.Text;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonNullSerializer : IonSerializer<object>
    {
        public override object Deserialize(IIonReader reader)
        {
            return null;
        }

        public override void Serialize(IIonWriter writer, object item)
        {
            writer.WriteNull();
        }
    }

    public class IonByteArraySerializer : IonSerializer<byte[]>
    {
        public override byte[] Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return blob;
        }

        public override void Serialize(IIonWriter writer, byte[] item)
        {
            writer.WriteBlob(item);
        }
    }
    public class IonStringSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            return reader.StringValue();
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item);
        }
    }

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

    public class IonLongSerializer : IonSerializer<long>
    {
        internal static readonly string ANNOTATION = "numeric.int32";

        public override long Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        public override void Serialize(IIonWriter writer, long item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteInt(item);
        }
    }
    public class IonBooleanSerializer : IonSerializer<bool>
    {
        public override bool Deserialize(IIonReader reader)
        {
            return reader.BoolValue();
        }

        public override void Serialize(IIonWriter writer, bool item)
        {
            writer.WriteBool(item);
        }
    }

    public class IonDoubleSerializer : IonSerializer<double>
    {
        public override double Deserialize(IIonReader reader)
        {
            return reader.DoubleValue();
        }

        public override void Serialize(IIonWriter writer, double item)
        {
            writer.WriteFloat(item);
        }
    }

    public class IonDecimalSerializer : IonSerializer<decimal>
    {
        internal static readonly string ANNOTATION = "numeric.decimal128";

        public override decimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue().ToDecimal();
        }

        public override void Serialize(IIonWriter writer, decimal item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteDecimal(item);
        }
    }

    public class IonBigDecimalSerializer : IonSerializer<BigDecimal>
    {
        public override BigDecimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue();
        }

        public override void Serialize(IIonWriter writer, BigDecimal item)
        {
            writer.WriteDecimal(item);
        }
    }

    public class IonFloatSerializer : IonSerializer<float>
    {
        internal static readonly string ANNOTATION = "numeric.float32";

        public override float Deserialize(IIonReader reader)
        {
            
            return Convert.ToSingle(reader.DoubleValue());
        }

        public override void Serialize(IIonWriter writer, float item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteFloat(item);
        }
    }

    public class IonDateTimeSerializer : IonSerializer<DateTime>
    {
        public override DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue;
        }

        public override void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item));
        }
    }

    public class IonSymbolSerializer : IonSerializer<SymbolToken>
    {
        public override SymbolToken Deserialize(IIonReader reader)
        {
            return reader.SymbolValue();
        }

        public override void Serialize(IIonWriter writer, SymbolToken item)
        {
            writer.WriteSymbolToken(item);
        }
    }

    public class IonClobSerializer : IonSerializer<string>
    {
        public override string Deserialize(IIonReader reader)
        {
            byte[] clob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(clob);
            return Encoding.UTF8.GetString(clob);
        }

        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteClob(Encoding.UTF8.GetBytes(item));
        }
    }

    public class IonGuidSerializer : IonSerializer<Guid>
    {
        internal static readonly string ANNOTATION = "guid128";
        private IonSerializationOptions options;

        public IonGuidSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        public override Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return new Guid(blob);
        }

        public override void Serialize(IIonWriter writer, Guid item)
        {
            if (options.AnnotateGuids) {
                writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            }
            writer.WriteBlob(item.ToByteArray());
        }
    }
}