using System.Collections.Generic;
using System.Linq;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Resource;

namespace NeoNetsphere.Shop
{
    internal class ShopItem
    {
        public ShopItem(ShopItemDto dto, ShopResources shopResources)
        {
            ItemNumber = dto.Id;
            Gender = (Gender) dto.RequiredGender;
            License = (ItemLicense) dto.RequiredLicense;
            ColorGroup = dto.Colors;
            UniqueColorGroup = dto.UniqueColors;
            MinLevel = dto.RequiredLevel;
            MaxLevel = dto.LevelLimit;
            MasterLevel = dto.RequiredMasterLevel;
            //RepairCost = dto.repair_cost;
            IsOneTimeUse = dto.IsOneTimeUse;
            IsDestroyable = dto.IsDestroyable;
            MainTab = dto.MainTab;
            SubTab = dto.SubTab;
            ItemInfos = dto.ItemInfos.Select(i => new ShopItemInfo(this, i, shopResources)).ToList();
        }

        public ItemNumber ItemNumber { get; set; }
        public Gender Gender { get; set; }
        public ItemLicense License { get; set; }
        public int ColorGroup { get; set; }
        public int UniqueColorGroup { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int MasterLevel { get; set; }

        //public int RepairCost { get; set; }
        public bool IsOneTimeUse { get; set; }

        public bool IsDestroyable { get; set; }
        public byte MainTab { get; set; }
        public byte SubTab { get; set; }
        public IList<ShopItemInfo> ItemInfos { get; set; }

        public ShopItemInfo GetItemInfo(int id)
        {
            return ItemInfos.FirstOrDefault(i => i.Id == id);
        }

        public ShopItemInfo GetItemInfo(ItemPriceType priceType)
        {
            return ItemInfos.FirstOrDefault(i => i.PriceGroup.PriceType == priceType);
        }
    }

    internal class ShopItemInfo
    {
        public ShopItemInfo(ShopItem shopItem, ShopItemInfoDto dto, ShopResources shopResources)
        {
            Id = dto.Id;
            PriceGroup = shopResources.Prices[dto.PriceGroupId];
            EffectGroup = shopResources.Effects[dto.EffectGroupId];
            IsEnabled = dto.IsEnabled;
            Discount = dto.DiscountPercentage;

            ShopItem = shopItem;
        }

        public int Id { get; set; }
        public ShopPriceGroup PriceGroup { get; set; }
        public ShopEffectGroup EffectGroup { get; set; }
        public bool IsEnabled { get; set; }
        public int Discount { get; set; }

        public ShopItem ShopItem { get; }
    }
}
