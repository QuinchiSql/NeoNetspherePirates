using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoNetsphere.Commands
{
    internal class BanCommands : ICommand
    {
        public BanCommands()
        {
            Name = "/ban";
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
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> - permanent ban");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> pardon - unban");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> days <duration(days)> - ban for x days");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> mins <duration(minutes)> - ban for x minutes");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> secs <duration(seconds)> - ban for x seconds");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> <currentdate(ex:20180130)> <unk> <duration(seconds)>");
                return true;
            }

            if(args.Length < 2)
            {
                Array.Resize(ref args, args.Length+1);
                args[1] = "none";
            }

            bool unban = false;
            string nickname = args[0];
            int DurationInSeconds = 0;
            DateTimeOffset curDate = DateTimeOffset.MinValue;

            if (args[1] == "pardon")
            {
                unban = true;
            }
            else if (args[1] == "secs")
            {
                int.TryParse(args[2], out DurationInSeconds);
            }
            else if (args[1] == "mins")
            {
                int.TryParse(args[2], out var DurationInMinutes);
                DurationInSeconds = DurationInMinutes * 60;
            }
            else if (args[1] == "days")
            {
                int.TryParse(args[2], out var DurationInDays);
                DurationInSeconds = DurationInDays * 24 * 60 * 60 ; //days->hours->minutes->seconds
            }
            else if(DateTimeOffset.TryParseExact(args[1], "yyyyMMdd",CultureInfo.InvariantCulture,DateTimeStyles.None, out curDate))
            {
                if(args.Length >= 4)
                {
                    int.TryParse(args[2], out var Unk);
                    int.TryParse(args[3], out var DurationInMs);
                    DurationInSeconds = DurationInMs / 60;
                }
            }
            else
            {
                DurationInSeconds = (int)TimeSpan.FromDays(10 * 365).TotalSeconds;
            }

            AccountDto account;
            using (var db = AuthDatabase.Open())
            {
                account = (db.Find<AccountDto>(statement => statement
                       .Include<BanDto>(join => join.LeftOuterJoin())
                       .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                       .WithParameters(new { Nickname = nickname })))
                    .FirstOrDefault();

                if (account == null)
                {
                    plr.SendConsoleMessage(S4Color.Red + "Unknown player!");
                    return true;
                }

                if (unban)
                {
                    foreach (var accountBan in account.Bans)
                    {
                        accountBan.Duration = 0;
                        db.UpdateAsync(accountBan);
                    }

                    db.UpdateAsync(account);
                    Console.WriteLine($"{plr?.Account?.Nickname?? "Unknown player"} has unbanned {account.Nickname}");
                    if (plr != null)
                        plr.SendConsoleMessage(S4Color.Green + $"Unbanned {account.Nickname}");
                }
                else
                {
                    var duration = TimeSpan.FromSeconds(DurationInSeconds);

                    var ban = new BanDto()
                    {
                        AccountId = account.Id,
                        Account = account,
                        Date = curDate.ToUnixTimeSeconds(),
                        Duration = DateTimeOffset.Now.Add(duration).ToUnixTimeSeconds()
                    };

                    var player = GameServer.Instance.PlayerManager.Get((ulong)account.Id);
                    player?.Session?.CloseAsync();

                    db.InsertAsync(ban);
                    db.UpdateAsync(account);

                    var uptime = new StringBuilder();
                    if (duration.Days > 0)
                        uptime.AppendFormat("{0} days ", duration.Days);
                    if (duration.Hours > 0)
                        uptime.AppendFormat("{0} hours ", duration.Hours);
                    if (duration.Minutes > 0)
                        uptime.AppendFormat("{0} minutes ", duration.Minutes);
                    if (duration.Seconds > 0)
                        uptime.AppendFormat("{0} seconds ", duration.Seconds);

                    Console.WriteLine($"{plr?.Account?.Nickname ?? "Unknown player"} has banned {account.Nickname} for {uptime.ToString()}");
                    if (plr != null)
                        plr.SendConsoleMessage(S4Color.Green + $"Banned {account.Nickname} for {uptime.ToString()}");
                }
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
