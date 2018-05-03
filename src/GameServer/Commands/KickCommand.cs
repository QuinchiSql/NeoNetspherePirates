using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Network;

namespace NeoNetsphere.Commands
{
    internal class UserkickCommand : ICommand
    {
        public UserkickCommand()
        {
            Name = "/userkick";
            AllowConsole = true;
            Permission = SecurityLevel.GameMaster;
            SubCommands = new ICommand[0];
        }

        public string Name { get; }
        public bool AllowConsole { get; }
        public SecurityLevel Permission { get; }
        public IReadOnlyList<ICommand> SubCommands { get; }

        public bool Execute(GameServer server, Player plr, string[] args)
        {
            return new KickCommand().Execute(server, plr, args);
        }

        public string Help()
        {
            return new KickCommand().Help();
        }
    }

    internal class KickCommand : ICommand
    {
        public KickCommand()
        {
            Name = "/kick";
            AllowConsole = true;
            Permission = SecurityLevel.GameMaster;
            SubCommands = new ICommand[0];
        }

        public string Name { get; }
        public bool AllowConsole { get; }
        public SecurityLevel Permission { get; }
        public IReadOnlyList<ICommand> SubCommands { get; }

        public bool Execute(GameServer server, Player plr, string[] args)
        {
            if (args.Length < 1)
            {
                plr.SendConsoleMessage(S4Color.Red + "Wrong Usage, possible usages:");
                plr.SendConsoleMessage(S4Color.Red + "> /kick <username>");
                return true;
            }

            var nickname = args[0];
            using (var db = AuthDatabase.Open())
            {
                var account = db.Find<AccountDto>(statement => statement
                        .Include<BanDto>(join => @join.LeftOuterJoin())
                        .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                        .WithParameters(new { Nickname = nickname }))
                    .FirstOrDefault();

                if (account == null)
                {
                    plr.SendConsoleMessage(S4Color.Red + "Unknown player");
                    return true;
                }

                var player = GameServer.Instance.PlayerManager.Get((ulong)account.Id);
                if (player == null)
                {
                    plr.SendConsoleMessage(S4Color.Red + "Player is not online");
                    return true;
                }

                player.Disconnect();
                plr.SendConsoleMessage(S4Color.Green + $"Kicked {account.Nickname} out of room");
            }
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
    }
}
