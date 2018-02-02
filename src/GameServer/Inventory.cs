using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BlubLib.Collections.Concurrent;
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Shop;
using Netsphere;

namespace NeoNetsphere
{
    internal class Inventory : IReadOnlyCollection<PlayerItem>
    {
        private readonly ConcurrentDictionary<ulong, PlayerItem> _items = new ConcurrentDictionary<ulong, PlayerItem>();
        private readonly ConcurrentStack<PlayerItem> _itemsToDelete = new ConcurrentStack<PlayerItem>();

        internal Inventory(Player plr, PlayerDto dto)
        {
            Player = plr;

            foreach (var item in dto.Items.Select(i => new PlayerItem(this, i)))
                _items.TryAdd(item.Id, item);
        }

        public Player Player { get; }

        /// <summary>
        ///     Returns the item with the given id or null if not found
        /// </summary>
        public PlayerItem this[ulong id] => GetItem(id);

        public int Count => _items.Count;

        public IEnumerator<PlayerItem> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Returns the item with the given id or null if not found
        /// </summary>
        public PlayerItem GetItem(ulong id)
        {
            PlayerItem item;
            _items.TryGetValue(id, out item);
            return item;
        }

        /// <summary>
        ///     Returns the item with the given id or null if not found
        /// </summary>
        public PlayerItem GetItemByShopInfoId(uint id)
        {
            try
            {
                var item = _items.Values.Where(item_ => item_.GetShopItemInfo().Id == id).ToList();
                if (item.Count < 1)
                    return null;

                return item.LastOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///     Creates a new item
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public PlayerItem Create(ItemNumber itemNumber, ItemPriceType priceType, ItemPeriodType periodType,
            ushort period, byte color, uint[] effects, uint count)
        {
            var shop = GameServer.Instance.ResourceCache.GetShop();
            var shopItemInfo = shop.GetItemInfo(itemNumber, priceType);
            if (shopItemInfo == null)
                throw new ArgumentException("Item not found");

            var price = shopItemInfo.PriceGroup.GetPrice(periodType, period);
            if (price == null)
                throw new ArgumentException("Price not found");
            return Create(shopItemInfo, price, color, effects, count);
        }

        /// <summary>
        ///     Creates a new item
        /// </summary>
        /// <exception cref="CharacterException"></exception>
        public PlayerItem Create(ShopItemInfo shopItemInfo, ShopPrice price, byte color, uint[] effects, uint count)
        {
            if (effects.Length == 0)
                effects = new uint[] {0};
            var item = new PlayerItem(this, shopItemInfo, price, color, effects, DateTimeOffset.Now, count);
            _items.TryAdd(item.Id, item);
            Player.Session.SendAsync(
                new ItemUpdateInventoryAckMessage(InventoryAction.Add, item.Map<PlayerItem, ItemDto>()));
            return item;
        }

        /// <summary>
        ///     Removes the item from the inventory
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Remove(PlayerItem item)
        {
            Remove(item.Id);
        }

        /// <summary>
        ///     Removes the item from the inventory
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Remove(ulong id)
        {
            var item = GetItem(id);
            if (item == null)
                throw new ArgumentException($"Item {id} not found", nameof(id));

            _items.Remove(item.Id);
            if (item.ExistsInDatabase)
                _itemsToDelete.Push(item);

            Player.Session.SendAsync(new ItemInventroyDeleteAckMessage(item.Id));
        }

        internal void Save(IDbConnection db)
        {
            if (!_itemsToDelete.IsEmpty)
            {
                var idsToRemove = new StringBuilder();
                var firstRun = true;
                PlayerItem itemToDelete;
                while (_itemsToDelete.TryPop(out itemToDelete))
                {
                    if (firstRun)
                        firstRun = false;
                    else
                        idsToRemove.Append(',');
                    idsToRemove.Append(itemToDelete.Id);
                }

                db.BulkDelete<PlayerItemDto>(statement => statement
                    .Where($"{nameof(PlayerItemDto.Id):C} IN ({idsToRemove})"));
            }

            foreach (var item in _items.Values)
            {
                var raw_effects = item.Effects.ToList();
                var dtoEffects = "";
                try
                {
                    dtoEffects = string.Join(",", raw_effects);
                }
                catch (Exception ex)
                {
                    dtoEffects = "0";
                }

                if (!item.ExistsInDatabase)
                {
                    db.Insert(new PlayerItemDto
                    {
                        Id = (int) item.Id,
                        PlayerId = (int) Player.Account.Id,
                        ShopItemInfoId = item.GetShopItemInfo().Id,
                        ShopPriceId = item.GetShopItemInfo().PriceGroup.GetPrice(item.PeriodType, item.Period).Id,
                        Effects = dtoEffects,
                        Color = item.Color,
                        PurchaseDate = item.PurchaseDate.ToUnixTimeSeconds(),
                        Durability = item.Durability,
                        Count = (int) item.Count
                    });
                    item.ExistsInDatabase = true;
                }
                else
                {
                    if (!item.NeedsToSave)
                        continue;

                    db.Update(new PlayerItemDto
                    {
                        Id = (int) item.Id,
                        PlayerId = (int) Player.Account.Id,
                        ShopItemInfoId = item.GetShopItemInfo().Id,
                        ShopPriceId = item.GetShopPrice().Id,
                        Effects = dtoEffects,
                        Color = item.Color,
                        PurchaseDate = item.PurchaseDate.ToUnixTimeSeconds(),
                        Durability = item.Durability,
                        Count = (int) item.Count
                    });
                    item.NeedsToSave = false;
                }
            }
        }

        public bool Contains(ulong id)
        {
            return _items.ContainsKey(id);
        }
    }
}
