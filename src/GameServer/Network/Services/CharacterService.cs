using System;
using System.Collections.Generic;
using System.Linq;
using BlubLib.DotNetty.Handlers.MessageHandling;
using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Message.Game;
using Netsphere;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network.Services
{
    internal class CharacterService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(CharacterService));

        [MessageHandler(typeof(CharacterFirstCreateReqMessage))]
        public async void CharacterFirstCreateHandler(GameSession session, CharacterFirstCreateReqMessage message)
        {
            var plr = session.Player;
            var cmng = plr.CharacterManager;
            if (!await AuthService.IsNickAvailableAsync(message.Nickname))
            {
                await session.SendAsync(new ServerResultAckMessage(ServerResult.NicknameUnavailable));
                return;
            }

            using (var db = AuthDatabase.Open())
            {
                var result = (await db.FindAsync<AccountDto>(smtp => smtp
                    .Where($"{nameof(AccountDto.Id):C} = @Id")
                    .WithParameters(new { session.Player.Account.Id })
                )).FirstOrDefault();

                if (result == null)
                {
                    await session.SendAsync(new ServerResultAckMessage(ServerResult.CreateCharacterFailed));
                    return;
                }

                result.Nickname = message.Nickname;
                await db.UpdateAsync(result);
                plr.Account.Nickname = message.Nickname;
            }

            try
            {
                cmng.Create(0, (CharacterGender)message.Gender, 0, 0, 0, 0);
                cmng.Select(0);
            }
            catch (CharacterException ex)
            {
                Logger.ForAccount(session)
                    .Error(ex.Message);
                await session.SendAsync(new ServerResultAckMessage(ServerResult.CreateCharacterFailed));
                return;
            }

            await AuthService.LoginAsync(session);

            IEnumerable<StartItemDto> startItems;
            using (var db = GameDatabase.Open())
            {
                startItems = await db.FindAsync<StartItemDto>(statement => statement
                    .Where(
                        $"{nameof(StartItemDto.RequiredSecurityLevel):C} <= @{nameof(plr.Account.SecurityLevel)}")
                    .WithParameters(new { plr.Account.SecurityLevel }));
            }

            foreach (var startItem in startItems)
            {
                var shop = GameServer.Instance.ResourceCache.GetShop();
                var item = shop.Items.Values.First(group => group.GetItemInfo(startItem.ShopItemInfoId) != null);
                var itemInfo = item.GetItemInfo(startItem.ShopItemInfoId);
                var effect = itemInfo.EffectGroup.GetEffect(startItem.ShopEffectId);

                var price = itemInfo.PriceGroup.GetPrice(startItem.ShopPriceId);
                if (price == null)
                {
                    Logger.Warning("Cant find ShopPrice for Start item {startItemId} - Forgot to reload the cache?",
                        startItem.Id);
                    continue;
                }

                var color = startItem.Color;
                if (color > item.ColorGroup)
                {
                    Logger.Warning("Start item {startItemId} has an invalid color {color}", startItem.Id, color);
                    color = 0;
                }

                var count = startItem.Count;
                if (count > 0 && item.ItemNumber.Category <= ItemCategory.Skill)
                {
                    Logger.Warning("Start item {startItemId} cant have stacks(quantity={count})", startItem.Id,
                        count);
                    count = 0;
                }

                if (count < 0)
                    count = 0;
                var reteff = new List<uint>
                    {
                        effect.Effect
                    };
                plr.Inventory.Create(itemInfo, price, color, reteff.ToArray(), (uint)count);
            }

            try
            {
                foreach (var itemNumber in message.FirstItems)
                {
                    if (itemNumber != 0)
                    {
                        var pi = plr.Inventory.Create(itemNumber, ItemPriceType.PEN, ItemPeriodType.None, 0, 0, new uint[] { 0 }, 1);
                        cmng.CurrentCharacter.Costumes.Equip(pi, (CostumeSlot)pi.ItemNumber.SubCategory);
                    }
                }
            }
            catch (ArgumentException e)
            {
                Logger.Debug(e, "Problem creating new items");
            }
        }

        [MessageHandler(typeof(CharacterCreateReqMessage))]
        public void CreateCharacterHandler(GameSession session, CharacterCreateReqMessage message)
        {
            Logger.ForAccount(session)
                .Information("Creating character: {message}",
                    JsonConvert.SerializeObject(message, new StringEnumConverter()));

            try
            {
                session.Player?.CharacterManager?.Create(message.Slot, message.Style.Gender, message.Style.Hair,
                    message.Style.Face, message.Style.Shirt, message.Style.Pants);
            }
            catch (CharacterException ex)
            {
                Logger.ForAccount(session)
                    .Error(ex.Message);
                session.SendAsync(new ServerResultAckMessage(ServerResult.CreateCharacterFailed));
            }
        }

        [MessageHandler(typeof(CharacterSelectReqMessage))]
        public void SelectCharacterHandler(GameSession session, CharacterSelectReqMessage message)
        {
            var plr = session.Player;

            if (plr == null)
                return;

            // Prevents player from changing characters while playing
            if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby &&
                !plr.Room.GameRuleManager.GameRule.StateMachine.IsInState(GameRuleState.HalfTime))
            {
                session.SendAsync(new ServerResultAckMessage(ServerResult.SelectCharacterFailed));
                return;
            }

            Logger.ForAccount(session)
                .Information("Selecting character {slot}", message.Slot);

            try
            {
                plr.CharacterManager?.Select(message.Slot);
            }
            catch (Exception ex)
            {
                Logger.ForAccount(session)
                    .Error(ex.Message);
                session.SendAsync(new ServerResultAckMessage(ServerResult.SelectCharacterFailed));
            }
        }

        [MessageHandler(typeof(CharacterDeleteReqMessage))]
        public void DeleteCharacterHandler(GameSession session, CharacterDeleteReqMessage message)
        {
            Logger.ForAccount(session)
                .Information("Removing character {slot}", message.Slot);

            try
            {
                session.Player?.CharacterManager?.Remove(message.Slot);
            }
            catch (CharacterException ex)
            {
                Logger.ForAccount(session)
                    .Error(ex.Message);
                session.SendAsync(new ServerResultAckMessage(ServerResult.DeleteCharacterFailed));
            }
        }
    }
}
