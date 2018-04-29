using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ItemDropDto
    {
        public ItemDropDto()
        {
            Position = new byte[6];
        }

        [BlubMember(0)]
        public int Type { get; set; }

        [BlubMember(1)]
        public int EntityId { get; set; } //guessed

        [BlubMember(2, typeof(FixedArraySerializer), 6)]
        public byte[] Position { get; set; } 

        [BlubMember(3)]
        public int Ammo { get; set; } // ammo related

        [BlubMember(4)]
        public byte Unk5 { get; set; }

        [BlubMember(5)]
        public short Unk6 { get; set; }

        [BlubMember(6)]
        public short Unk7 { get; set; }
    }
}
