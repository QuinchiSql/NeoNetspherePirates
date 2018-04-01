using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ItemCheckDto
    {
        public ItemCheckDto()
        {
            ItemNumber = 0;
            Effects = Array.Empty<uint>();
        }

        [BlubMember(0)]
        public ulong ItemId { get; set; } //8

        [BlubMember(1)]
        public ItemNumber ItemNumber { get; set; } //12

        [BlubMember(2)]
        public uint Color { get; set; } //16

        [BlubMember(3, typeof(ArrayWithIntPrefixSerializer))]
        public uint[] Effects { get; set; } //20

        [BlubMember(4)]
        public float Power { get; set; } //24

        [BlubMember(5)]
        public float MoveSpeedRate { get; set; } //28
    }
}
