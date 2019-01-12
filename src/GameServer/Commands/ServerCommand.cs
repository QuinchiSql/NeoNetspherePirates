using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NeoNetsphere.Network;

namespace NeoNetsphere.Commands
{
    internal class ServerCommand : ICommand
    {
        public ServerCommand()
        {
            Name = "server";
            AllowConsole = true;
            Permission = SecurityLevel.GameMaster;
            SubCommands = new ICommand[] {new StatusCommand()};
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


        private class StatusCommand : ICommand
        {
            public StatusCommand()
            {
                Name = "status";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                // Todo peak
                var ts = DateTime.Now - Process.GetCurrentProcess().StartTime;
                var uptime = new StringBuilder();
                if (ts.Days > 0)
                    uptime.AppendFormat("{0} days ", ts.Days);
                if (ts.Hours > 0)
                    uptime.AppendFormat("{0} hours ", ts.Hours);
                if (ts.Minutes > 0)
                    uptime.AppendFormat("{0} minutes ", ts.Minutes);
                if (ts.Seconds > 0)
                    uptime.AppendFormat("{0} seconds ", ts.Seconds);

                var message =
                    $"Uptime: {uptime}{Environment.NewLine}Online: {server.Sessions.Values.Where(c => { var a = (GameSession) c; return a.IsLoggedIn(); }).ToList().Count} Peak: {0}";
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
