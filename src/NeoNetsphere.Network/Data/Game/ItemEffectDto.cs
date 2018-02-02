using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class ItemEffectDto
    {
        public ItemEffectDto()
        {
            Unk1 = -1;
        }

        [BlubMember(0)]
        public uint Effect { get; set; }

        [BlubMember(1)]
        public int Unk1 { get; set; }

        [BlubMember(2)]
        public long Unk2 { get; set; }

        [BlubMember(3)]
        public int Unk3 { get; set; }
    }
}
