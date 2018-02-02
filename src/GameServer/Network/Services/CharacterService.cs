using System;
using BlubLib.DotNetty.Handlers.MessageHandling;
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

        [MessageHandler(typeof(CharacterCreateReqMessage))]
        public void CreateCharacterHandler(GameSession session, CharacterCreateReqMessage message)
        {
            Logger.ForAccount(session)
                .Information("Creating character: {message}",
                    JsonConvert.SerializeObject(message, new StringEnumConverter()));

            try
            {
                session.Player.CharacterManager.Create(message.Slot, message.Style.Gender, message.Style.Hair,
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
                plr.CharacterManager.Select(message.Slot);
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
                session.Player.CharacterManager.Remove(message.Slot);
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
