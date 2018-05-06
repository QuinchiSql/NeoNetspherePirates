using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoNetsphere.Network;

namespace NeoNetsphere.Commands
{
    internal class NoticeCommand : ICommand
    {
        public NoticeCommand()
        {
            Name = "/whole_notice";
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
            var notice = new StringBuilder();
            foreach (var x in args.ToList())
                notice.Append(" " + x);

            server.BroadcastNotice(notice.ToString().Replace("/whole_notice", ""));
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
