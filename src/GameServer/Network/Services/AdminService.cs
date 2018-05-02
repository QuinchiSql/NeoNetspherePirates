using System.Threading.Tasks;
using BlubLib;
using BlubLib.DotNetty.Handlers.MessageHandling;
using NeoNetsphere.Network.Message.Game;
using ProudNetSrc.Handlers;

namespace NeoNetsphere.Network.Services
{
    internal class AdminService : ProudMessageHandler
    {
        [MessageHandler(typeof(AdminShowWindowReqMessage))]
        public Task ShowWindowHandler(GameSession session)
        {
            return session.SendAsync(
                new AdminShowWindowAckMessage(session.Player.Account.SecurityLevel <= SecurityLevel.Tester));
        }

        [MessageHandler(typeof(AdminActionReqMessage))]
        public void AdminActionHandler(GameServer server, GameSession session, AdminActionReqMessage message)
        {
            var args = message.Command.GetArgs();
            if (session.Player == null) return;
            if (server.CommandManager.Execute(session.Player, args))
                return;
            session.Player.SendConsoleMessage(S4Color.Red + "Command is not implemented.");
        }
    }
}
