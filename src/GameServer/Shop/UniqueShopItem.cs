using NeoNetsphere.Database.Game;
using NeoNetsphere.Resource;

namespace NeoNetsphere.Shop
{
    internal class UniqueShopItem
    {
        public UniqueShopItem(ShopItemDto dto, ShopResources shopResources)
        {
        }

        public ItemNumber ItemNumber { get; set; }
        public int ShopId { get; set; }
        public int Discount { get; set; }
        public ItemPeriodType PeriodType { get; set; }
        public int Period { get; set; }
        public int Color { get; set; }
        public bool Enabled { get; set; }

        //public UniqueShopItemInfo GetItemInfo(int id)
        //{
        //    return ItemInfos.FirstOrDefault(i => i.Id == id);
        //}
        //
        //public UniqueShopItemInfo GetItemInfo(ItemPriceType priceType)
        //{
        //    return ItemInfos.FirstOrDefault(i => i.PriceGroup.PriceType == priceType);
        //}
    }

    internal class UniqueShopItemInfo
    {
        public UniqueShopItemInfo(ShopItem shopItem, ShopItemInfoDto dto, ShopResources shopResources)
        {
        }

        public ItemNumber ItemNumber { get; set; }
        public int ShopId { get; set; }
        public int Discount { get; set; }
        public ItemPeriodType PeriodType { get; set; }
        public int Period { get; set; }
        public int Color { get; set; }
        public bool Enabled { get; set; }
    }
}
