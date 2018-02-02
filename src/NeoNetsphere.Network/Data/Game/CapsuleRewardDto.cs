using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class CapsuleRewardDto
    {
        public CapsuleRewardDto()
        {
        }

        public CapsuleRewardDto(CapsuleRewardType _type, uint _pen, ItemNumber _itemnumber, ItemPriceType _priceType,
            ItemPeriodType _periodType, uint _period)
        {
            RewardType = _type;
            PEN = _pen;
            ItemNumber = _itemnumber;
            PriceType = _priceType;
            PeriodType = _periodType;
            Period = _period;
        }

        [BlubMember(0)]
        public CapsuleRewardType RewardType { get; set; }

        [BlubMember(1)]
        public uint PEN { get; set; }

        [BlubMember(2)]
        public ItemNumber ItemNumber { get; set; }

        [BlubMember(3)]
        public ItemPriceType PriceType { get; set; }

        [BlubMember(4)]
        public ItemPeriodType PeriodType { get; set; }

        [BlubMember(5)]
        public uint Period { get; set; }

        [BlubMember(6)]
        public byte Unk1 { get; set; }

        [BlubMember(7)]
        public uint Effect { get; set; }

        [BlubMember(8)]
        public byte Unk2 { get; set; }
    }
}
