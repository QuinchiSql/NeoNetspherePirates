using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.IO;
using ExpressMapper.Extensions;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;
using ProudNetSrc;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network.Services
{
    internal class ShopService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ShopService));

        public static async Task ShopUpdateMsg(ProudSession session = (ProudSession) null, bool broadcast = false)
        {
            if (session == null && broadcast == false)
                return;
            var targets = new List<ProudSession>();
            if (broadcast)
                foreach (var sessionsValue in GameServer.Instance.Sessions.Values)
                    targets.Add(sessionsValue);
            else
                targets.Add(session);

            var shop = GameServer.Instance.ResourceCache.GetShop();
            var version = shop.Version;

            var w = new BinaryWriter(new MemoryStream());
            w.Serialize(shop.Prices.Values.ToArray());
            var shopPrices = w.ToArray();
            w.Dispose();
            w = new BinaryWriter(new MemoryStream());

            w.Serialize(shop.Effects.Values.ToArray());
            var shopEffects = w.ToArray();
            w.Dispose();
            w = new BinaryWriter(new MemoryStream());

            w.Serialize(shop.Items.Values.ToArray());
            var shopItems = w.ToArray();
            w.Dispose();


            foreach (var proudSession in targets)
            {
                await proudSession.SendAsync(new NewShopUpdataInfoAckMessage
                {
                    Type = ShopResourceType.NewShopPrice,
                    Data = shopPrices,
                    Date = version
                }, SendOptions.ReliableSecureCompress);


                await proudSession.SendAsync(new NewShopUpdataInfoAckMessage
                {
                    Type = ShopResourceType.NewShopEffect,
                    Data = shopEffects,
                    Date = version
                }, SendOptions.ReliableSecureCompress);

                await proudSession.SendAsync(new NewShopUpdataInfoAckMessage
                {
                    Type = ShopResourceType.NewShopItem,
                    Data = shopItems,
                    Date = version
                }, SendOptions.ReliableSecureCompress);

                // ToDo
                w = new BinaryWriter(new MemoryStream());
                w.Write(0);
                await proudSession.SendAsync(new NewShopUpdataInfoAckMessage
                {
                    Type = ShopResourceType.NewShopUniqueItem,
                    Data = w.ToArray(),
                    Date = version
                }, SendOptions.ReliableSecureCompress);

                await proudSession.SendAsync(new NewShopUpdateEndAckMessage());
            }
        }

        [MessageHandler(typeof(NewShopUpdateCheckReqMessage))]
        public async Task ShopUpdateCheckHandler(GameSession session, NewShopUpdateCheckReqMessage message)
        {
            var shop = GameServer.Instance.ResourceCache.GetShop();
            var version = shop.Version;
            await session.SendAsync(new NewShopUpdateCheckAckMessage
            {
                Date01 = version,
                Date02 = version,
                Date03 = version,
                Date04 = version,
                Unk = 0
            });

            if (message.Date01 == version &&
                message.Date02 == version &&
                message.Date03 == version &&
                message.Date04 == version)
                return;

            await ShopUpdateMsg(session, false);
        }

        [MessageHandler(typeof(RandomShopUpdateCheckReqMessage))]
        public async Task RandomShopUpdateCheckHandler(GameSession session, RandomShopUpdateCheckReqMessage message)
        {
            //Todo
        }

        [MessageHandler(typeof(CollectBookItemRegistReqMessage))]
        public async Task CollectBookItemRegistReq(GameSession session, CollectBookItemRegistReqMessage message)
        {
            //Todo
        }

        [MessageHandler(typeof(ItemBuyItemReqMessage))]
        public async Task BuyItemHandler(GameSession session, ItemBuyItemReqMessage message)
        {
            try
            {
                var shop = GameServer.Instance.ResourceCache.GetShop();
                var plr = session.Player;
                foreach (var item in message.Items)
                {
                    var shopItemInfo = shop.GetItemInfo(item.ItemNumber, item.PriceType);
                    if (shopItemInfo == null)
                    {
                        Logger.ForAccount(session)
                            .Error("No shop entry found for {item}",
                                new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                        await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                        return;
                    }

                    if (!shopItemInfo.IsEnabled)
                    {
                        Logger.ForAccount(session)
                            .Error("Shop entry is not enabled {item}",
                                new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                        await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                        return;
                    }

                    var priceGroup = shopItemInfo.PriceGroup;
                    var price = priceGroup.GetPrice(item.PeriodType, item.Period);
                    if (price == null)
                    {
                        Logger.ForAccount(session)
                            .Error("Invalid price group for shop entry {item}",
                                new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                        await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                        return;
                    }

                    if (!price.IsEnabled)
                    {
                        Logger.ForAccount(session)
                            .Error("Shop entry is not enabled {item}",
                                new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                        await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                        return;
                    }

                    if (item.Color > shopItemInfo.ShopItem.ColorGroup)
                    {
                        Logger.ForAccount(session)
                            .Error("Shop entry has no color {color} {item}",
                                item.Color, new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                        await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                        return;
                    }

                    var itemeffects = new List<uint>();
                    if (item.Effect != 0)
                        if (shopItemInfo.EffectGroup.MainEffect == item.Effect)
                        {
                            foreach (var effect in shopItemInfo.EffectGroup.Effects)
                                itemeffects.Add(effect.Effect);
                        }
                        else
                        {
                            Logger.ForAccount(session)
                                .Error("Shop entry has no effect {effect} {item}",
                                    item.Effect, new {item.ItemNumber, item.PriceType, item.Period, item.PeriodType});

                            await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.UnkownItem));
                            return;
                        }
                    else
                    {
                        itemeffects.Add(0);
                    }

                    // ToDo missing price types
                    switch (shopItemInfo.PriceGroup.PriceType)
                    {
                        case ItemPriceType.PEN:
                            if (plr.PEN < price.Price)
                            {
                                await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.NotEnoughMoney));
                                return;
                            }

                            plr.PEN -= (uint) price.Price;
                            break;

                        case ItemPriceType.AP:
                        case ItemPriceType.Premium:
                            if (plr.AP < price.Price)
                            {
                                await session.SendAsync(new ItemBuyItemAckMessage(ItemBuyResult.NotEnoughMoney));
                                return;
                            }

                            plr.AP -= (uint) price.Price;
                            break;

                        default:
                            Logger.ForAccount(session)
                                .Error("Unknown PriceType {priceType}", shopItemInfo.PriceGroup.PriceType);
                            return;
                    }

                    PlayerItem stackitem = null;
                    var stacked = false;
                    switch (item.PeriodType)
                    {
                        case ItemPeriodType.None:
                            break;
                        case ItemPeriodType.Units:
                            stackitem = session.Player.Inventory.GetItemByShopInfoId((uint) shopItemInfo.Id);
                            if (stackitem != null)
                            {
                                stackitem.Count += item.Period;
                                stackitem.NeedsToSave = true;
                                stacked = true;
                            }

                            break;
                        case ItemPeriodType.Days:
                        case ItemPeriodType.Hours:
                            break;
                        default:
                            Logger.ForAccount(session)
                                .Error("Unknown PriceType {priceType}", item.PeriodType);
                            break;
                    }

                    var plrItem = stackitem;

                    if (!stacked)
                    {
                        plrItem = session.Player.Inventory.Create(shopItemInfo, price, item.Color,
                            itemeffects.ToArray(),
                            (uint) (price.PeriodType == ItemPeriodType.Units ? price.Period : 0));
                    }
                    else
                    {
                        await session.SendAsync(new ItemUpdateInventoryAckMessage(InventoryAction.Update,
                            plrItem.Map<PlayerItem, ItemDto>()));
                    }

                    await session.SendAsync(new ItemBuyItemAckMessage(new[] {plrItem.Id}, item));
                    await session.SendAsync(new MoneyRefreshCashInfoAckMessage(plr.PEN, plr.AP));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
