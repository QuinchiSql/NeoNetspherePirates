using System;
using BlubLib.DotNetty.Handlers.MessageHandling;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using ProudNetSrc.Handlers;

namespace NeoNetsphere.Network.Services
{
    internal class GeneralService : ProudMessageHandler
    {
        [MessageHandler(typeof(TimeSyncReqMessage))]
        public void TimeSyncHandler(GameSession session, TimeSyncReqMessage message)
        {
            session.SendAsync(new TimeSyncAckMessage
            {
                ClientTime = message.Time,
                ServerTime = (uint) Program.AppTime.ElapsedMilliseconds
            });
            
            if (session.Player?.Room == null && session.UnreliablePing > 400)
                session.SendAsync(new ServerResultAckMessage(ServerResult.InternetSlow));
        }
        [MessageHandler(typeof(CheckhashKeyvaluereqMessage))]
        public void CheckhashKeyvaluereq(GameSession session, CheckhashKeyvaluereqMessage message)
        {
            //handle
        }
        
    }
}
