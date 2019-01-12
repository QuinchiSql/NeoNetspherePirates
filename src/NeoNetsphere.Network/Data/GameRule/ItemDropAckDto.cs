using System;
using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;
using NeoNetsphere.Network.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ItemDropAckDto
    {
        [BlubMember(0)]
        public uint Counter { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; } //3

        [BlubMember(2)]
        public int Unk3 { get; set; } //2

        [BlubMember(3)]
        public int Unk4 { get; set; } //?

        // if Unk2 != 4 -- 6 bytes
        [BlubMember(4, typeof(FixedArraySerializer), 6)]
        public byte[] Position { get; set; }

        // if Unk3 != 1
        [BlubMember(5)]
        public long Unk6 { get; set; } //?

        // else
        [BlubMember(6)]
        public long Unk7 { get; set; } //uuid?

        [BlubMember(7)]
        public int Unk8 { get; set; } //ItemNumber?

        [BlubMember(8)]
        public int Unk9 { get; set; } //?

        [BlubMember(9)]
        public int Unk10 { get; set; } //?

        [BlubMember(10, typeof(ArrayWithIntPrefixSerializer))]
        public int[] Unk11 { get; set; } //effects

        [BlubMember(11)]
        public short Unk12 { get; set; } //?

        [BlubMember(12)]
        public short Unk13 { get; set; } //?

        public ItemDropAckDto()
        {
            Position = new byte[6];
            Unk11 = new int[0];
        }

    }
}
