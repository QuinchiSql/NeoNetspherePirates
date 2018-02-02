using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class ItemDurabilityInfoDto
    {
        public ItemDurabilityInfoDto()
        {
            Durabilityloss = 0;
            Unk1 = 1;
        }

        public ItemDurabilityInfoDto(ulong itemId, int durabilityloss, int unk1)
        {
            ItemId = itemId;
            Durabilityloss = durabilityloss;
            Unk1 = unk1;
        }

        [BlubMember(0)]
        public ulong ItemId { get; set; }

        [BlubMember(1)]
        public int Durabilityloss { get; set; }

        [BlubMember(2)]
        public int Unk1 { get; set; }
    }
}
