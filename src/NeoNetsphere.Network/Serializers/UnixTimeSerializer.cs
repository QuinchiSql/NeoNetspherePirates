using System;
using System.IO;
using BlubLib.Serialization;

namespace NeoNetsphere.Network.Serializers
{
    public class UnixTimeSerializer : ISerializer<DateTimeOffset>
    {
        public bool CanHandle(Type type)
        {
            throw new NotImplementedException();
        }

        public void Serialize(BinaryWriter writer, DateTimeOffset value)
        {
            writer.Write(value.ToUnixTimeSeconds());
        }

        public DateTimeOffset Deserialize(BinaryReader reader)
        {
            return DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt64());
        }
    }
}
