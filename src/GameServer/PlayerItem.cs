using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Resource;
using NeoNetsphere.Shop;

namespace NeoNetsphere
{
    internal class PlayerItem
    {
        private uint _count;
        private int _durability = 2400;

        internal PlayerItem(Inventory inventory, PlayerItemDto dto)
        {
            var shop = GameServer.Instance.ResourceCache.GetShop();
            ExistsInDatabase = true;
            Inventory = inventory;
            Id = (ulong) dto.Id;

            var itemInfo = shop.Items.Values.First(group => group.GetItemInfo(dto.ShopItemInfoId) != null);
            ItemNumber = itemInfo.ItemNumber;

            var priceGroup = shop.Prices.Values.First(group => group.GetPrice(dto.ShopPriceId) != null);
            var price = priceGroup.GetPrice(dto.ShopPriceId);

            PriceType = priceGroup.PriceType;
            PeriodType = price.PeriodType;
            Period = price.Period;
            Color = dto.Color;

            var raweffects = new List<uint>();
            var effects_text = dto.Effects.Split(",").ToList();
            effects_text.ForEach(eff => { raweffects.Add(uint.Parse(eff)); });
            Effects = raweffects.ToArray();
            if (Effects.Length == 0)
                Effects = new uint[] {0};
            _durability = dto.Durability;
            _count = (uint) dto.Count;
            if (_count == 0)
                _count = 1;
            PurchaseDate = DateTimeOffset.FromUnixTimeSeconds(dto.PurchaseDate);
        }

        internal PlayerItem(Inventory inventory, ShopItemInfo itemInfo, ShopPrice price, byte color, uint[] effects,
            DateTimeOffset purchaseDate, uint count)
        {
            Inventory = inventory;
            Id = ItemIdGenerator.GetNextId();
            ItemNumber = itemInfo.ShopItem.ItemNumber;
            PriceType = itemInfo.PriceGroup.PriceType;
            PeriodType = price.PeriodType;
            Period = price.Period;
            Color = color;
            Effects = effects;
            PurchaseDate = purchaseDate;
            _durability = price.Durability;
            _count = count;
        }

        internal bool ExistsInDatabase { get; set; }
        internal bool NeedsToSave { get; set; }

        public Inventory Inventory { get; }

        public ulong Id { get; }
        public ItemNumber ItemNumber { get; }
        public ItemPriceType PriceType { get; }
        public ItemPeriodType PeriodType { get; }
        public ushort Period { get; }
        public byte Color { get; }
        public uint[] Effects { get; set; }
        public DateTimeOffset PurchaseDate { get; }
        public int DurabilityLoss { get; set; }

        public int Durability
        {
            get => _durability;
            set
            {
                if (_durability == value)
                    return;
                _durability = value;
                NeedsToSave = true;
            }
        }

        public uint Count
        {
            get => _count;
            set
            {
                if (_count == value)
                    return;
                _count = value;
                NeedsToSave = true;
            }
        }

        public DateTimeOffset ExpireDate =>
            PeriodType == ItemPeriodType.Days ? PurchaseDate.AddDays(Period) : DateTimeOffset.MinValue;

        public ItemEffect[] GetItemEffects()
        {
            if (Effects.Length == 0)
                return null;

            var effects = GameServer.Instance.ResourceCache.GetEffects();

            var ret_effects = new List<ItemEffect>();
            foreach (var eff in Effects)
                ret_effects.Add(effects.GetValueOrDefault(eff));
            return ret_effects.ToArray();
        }

        public ShopItem GetShopItem()
        {
            var shop = GameServer.Instance.ResourceCache.GetShop();
            return shop.GetItem(ItemNumber);
        }

        public ShopItemInfo GetShopItemInfo()
        {
            var shop = GameServer.Instance.ResourceCache.GetShop();
            return shop.GetItemInfo(ItemNumber, PriceType);
        }

        public ShopPrice GetShopPrice()
        {
            return GetShopItemInfo().PriceGroup.GetPrice(PeriodType, Period);
        }

        public Task LoseDurabilityAsync(int loss)
        {
            if (loss < 0)
                throw new ArgumentOutOfRangeException(nameof(loss));

            if (Inventory.Player.Room == null)
                throw new InvalidOperationException("Player is not inside a room");

            if (Durability == -1)
                return Task.CompletedTask;

            Durability -= loss;
            DurabilityLoss = loss;
            if (Durability < 0)
                Durability = 0;

            //return Inventory.Player.Session.SendAsync(new ItemDurabilityItemAckMessage(new ItemDurabilityInfoDto[] { new ItemDurabilityInfoDto { ItemId=this.ItemNumber, Durabilityloss = loss, Unk1 = 1 } } ));
            var send = Inventory.Player.Session.SendAsync(
                new ItemDurabilityItemAckMessage(new[] {this.Map<PlayerItem, ItemDurabilityInfoDto>()}));
            DurabilityLoss = 0;
            return send;
        }

        public uint CalculateRefund(ShopPrice price)
        {
            if (Count == 0)
                Count = 1;
            var shopprice = price.Price * Count;

            if (PriceType == ItemPriceType.Premium || PriceType == ItemPriceType.AP)
                return (uint) shopprice;
            return (uint) (shopprice * 0.25);
        }

        public uint CalculateRepair()
        {
            return 0; // Todo
        }
    }
}
