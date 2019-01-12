using System;
using BlubLib.DotNetty.Handlers.MessageHandling;
using ExpressMapper.Extensions;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Message.Chat;
using ProudNetSrc.Handlers;

namespace NeoNetsphere.Network.Services
{
    internal class CommunityService : ProudMessageHandler
    {
        [MessageHandler(typeof(OptionSaveCommunityReqMessage))]
        public void OptionSaveCommunityReq(ChatSession session, OptionSaveCommunityReqMessage message)
        {
            //handle
        }

        [MessageHandler(typeof(OptionSaveBinaryReqMessage))]
        public void OptionSaveBinaryReq(ChatSession session, OptionSaveBinaryReqMessage message)
        {
            //ToDo
        }

        [MessageHandler(typeof(UserDataOneReqMessage))]
        public void GetUserDataHandler(ChatSession session, UserDataOneReqMessage message)
        {
            var plr = session.Player;
            if (plr.Account.Id == message.AccountId)
                return;

            if (!plr.Channel.Players.TryGetValue(message.AccountId, out var target))
                return;

            //switch (target.Settings.Get<CommunitySetting>("AllowInfoRequest"))
            //{
            //    case CommunitySetting.Deny:
            //        // Not sure if there is an answer to this
            //        return;
            //
            //    case CommunitySetting.FriendOnly:
            //        // ToDo
            //        return;
            //}

            session.SendAsync(new UserDataFourAckMessage(0, target.Map<Player, UserDataDto>()));
        }

        [MessageHandler(typeof(DenyActionReqMessage))]
        public void DenyHandler(ChatServer service, ChatSession session, DenyActionReqMessage message)
        {
            var plr = session.Player;

            if (message.Deny.AccountId == plr.Account.Id)
                return;

            Deny deny;
            switch (message.Action)
            {
                case DenyAction.Add:
                    if (plr.DenyManager.Contains(message.Deny.AccountId))
                        return;

                    var target = GameServer.Instance.PlayerManager[message.Deny.AccountId];
                    if (target == null)
                        return;

                    deny = plr.DenyManager.Add(target);
                    session.SendAsync(new DenyActionAckMessage(0, DenyAction.Add, deny.Map<Deny, DenyDto>()));
                    break;

                case DenyAction.Remove:
                    deny = plr.DenyManager[message.Deny.AccountId];
                    if (deny == null)
                        return;

                    plr.DenyManager.Remove(message.Deny.AccountId);
                    session.SendAsync(new DenyActionAckMessage(0, DenyAction.Remove, deny.Map<Deny, DenyDto>()));
                    break;
            }
        }
    }
}
