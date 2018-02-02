using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlubLib.Threading.Tasks;
using ExpressMapper.Extensions;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Network.Services;
using NeoNetsphere.Resource;
using Netsphere;
using ProudNetSrc;

namespace NeoNetsphere.Commands
{
    internal class ReloadCommand : ICommand
    {
        public ReloadCommand()
        {
            Name = "reload";
            AllowConsole = true;
            Permission = SecurityLevel.Developer;
            SubCommands = new ICommand[] {new ShopCommand(), new ReqBoxCommand(), new RoomMassKickCommand(), new ServerMassKickCommand(), new ClubCommand(),  };
        }

        public string Name { get; }
        public bool AllowConsole { get; }
        public SecurityLevel Permission { get; }
        public IReadOnlyList<ICommand> SubCommands { get; }

        public bool Execute(GameServer server, Player plr, string[] args)
        {
            return true;
        }

        public string Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Name);
            foreach (var cmd in SubCommands)
            {
                sb.Append(" ");
                sb.AppendLine(cmd.Help());
            }
            return sb.ToString();
        }

        private class ClubCommand : ICommand
        {
            public ClubCommand()
            {
                Name = "clubs";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                Task.Factory.StartNew(() =>
                {
                    var message = "Reloading clubs, server may lag for a short period of time...";

                    if (plr == null)
                        Console.WriteLine(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);

                    server.BroadcastNotice(message);
                    server.ResourceCache.Clear(ResourceCacheType.Clubs);
                    server.ClubManager = new ClubManager(server.ResourceCache.GetClubs());
                    ClubService.Update(null, true);
                    message = "Club reload completed";
                    server.BroadcastNotice(message);
                    if (plr == null)
                        Console.WriteLine(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);
                });
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class ShopCommand : ICommand
        {
            public ShopCommand()
            {
                Name = "shop";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                Task.Factory.StartNew(() =>
                {
                    var message = "Reloading shop, server may lag for a short period of time...";

                    if (plr == null)
                        Console.WriteLine(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);

                    server.BroadcastNotice(message);
                    server.ResourceCache.Clear(ResourceCacheType.Shop);
                    ShopService.ShopUpdateMsg(null, true);

                    message = "Shop reload completed";
                    server.BroadcastNotice(message);
                    if (plr == null)
                        Console.WriteLine(message);
                    else
                        plr.SendConsoleMessage(S4Color.Green + message);
                });
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class ReqBoxCommand : ICommand
        {
            public ReqBoxCommand()
            {
                Name = "reqs";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                var message = "Trying to fix stuck request boxes..";

                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.ServerError));

                message = "Done trying to fix stuck request boxes.";
                //server.BroadcastNotice(message);
                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class ServerMassKickCommand : ICommand
        {
            public ServerMassKickCommand()
            {
                Name = "playerlist";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                var message = "Kicking all players..";

                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                foreach (var sess in GameServer.Instance.Sessions.Values)
                {
                    var session = (GameSession)sess;
                    if (session.Player != null && session.Player.Room != null)
                        session.Player.Room.Leave(session.Player);
                }
                GameServer.Instance.Broadcast(new ItemUseChangeNickAckMessage() {Result = 0});
                GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
                //GameServer.Instance.Broadcast(new ProudNetSrc.Serialization.Messages.RequestAutoPruneAckMessage(), SendOptions.Reliable);
                message = "Done with kickall";
                //server.BroadcastNotice(message);
                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }
        private class RoomMassKickCommand : ICommand
        {
            public RoomMassKickCommand()
            {
                Name = "rooms";
                AllowConsole = true;
                Permission = SecurityLevel.Developer;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                var message = "Kicking all players..";

                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                foreach (var sess in GameServer.Instance.Sessions.Values)
                {
                    var session = (GameSession) sess;
                    if (session.Player != null && session.Player.Room != null)
                        session.Player.Room.Leave(session.Player);
                }

                message = "Done kicking all players from all rooms.";
                //server.BroadcastNotice(message);
                if (plr == null)
                    Console.WriteLine(message);
                else
                    plr.SendConsoleMessage(S4Color.Green + message);

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }
    }
}
