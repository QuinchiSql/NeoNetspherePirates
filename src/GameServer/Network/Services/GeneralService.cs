using BlubLib.DotNetty.Handlers.MessageHandling;
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

            //if (session.Player?.Room == null && session.UnreliablePing > 500)
            //    session.SendAsync(new ServerResultAckMessage(ServerResult.InternetSlow));
        }

        [MessageHandler(typeof(CheckhashKeyvaluereqMessage))]
        public void CheckhashKeyvaluereq(GameSession session, CheckhashKeyvaluereqMessage message)
        {
            //if(message.hash != "")
            //    session.Dispose();
            //handle
        }
    }
}
