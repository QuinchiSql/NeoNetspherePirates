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
    internal class UnbanCommands : ICommand
    {
        public UnbanCommands()
        {
            Name = "/unban";
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
                plr.SendConsoleMessage(S4Color.Red + "> /unban <username>");
                return true;
            }

            var nickname = args[0];
            using (var db = AuthDatabase.Open())
            {
                var account = db.Find<AccountDto>(statement => statement
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                        .WithParameters(new {Nickname = nickname}))
                    .FirstOrDefault();

                if (account == null)
                {
                    plr.SendConsoleMessage(S4Color.Red + "Unknown player");
                    return true;
                }

                foreach (var accountBan in account.Bans)
                {
                    accountBan.Duration = 0;
                    db.UpdateAsync(accountBan);
                }

                db.UpdateAsync(account);
                Console.WriteLine($"{plr?.Account?.Nickname ?? "Unknown player"} has unbanned {account.Nickname}");
                plr?.SendConsoleMessage(S4Color.Green + $"Unbanned {account.Nickname}");
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
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> roomkick - roomkick");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> - permanent ban");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> pardon - unban");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> days <duration(days)> - ban for x days");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> mins <duration(minutes)> - ban for x minutes");
                plr.SendConsoleMessage(S4Color.Red + "> /ban <username> secs <duration(seconds)> - ban for x seconds");
                plr.SendConsoleMessage(S4Color.Red +
                                       "> /ban <username> <currentdate(ex:20180130)> <unk> <duration(seconds)>");
                return true;
            }

            if (args.Length < 2)
            {
                Array.Resize(ref args, args.Length + 1);
                args[1] = "none";
            }

            var unban = false;
            var nickname = args[0];
            var durationInSeconds = 0;


            try
            {
                AccountDto account;
                switch (args[1])
                {
                    case "pardon":
                        unban = true;
                        break;
                    case "roomkick":
                        using (var db = AuthDatabase.Open())
                        {
                            account = db.Find<AccountDto>(statement => statement
                                    .Include<BanDto>(join => join.LeftOuterJoin())
                                    .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                                    .WithParameters(new {Nickname = nickname}))
                                .FirstOrDefault();

                            if (account == null)
                            {
                                plr.SendConsoleMessage(S4Color.Red + "Unknown player");
                                return true;
                            }

                            var player = GameServer.Instance.PlayerManager.Get((ulong) account.Id);
                            if (player == null)
                            {
                                plr.SendConsoleMessage(S4Color.Red + "Player is not online");
                                return true;
                            }

                            player.Room?.Leave(player, RoomLeaveReason.ModeratorKick);
                            plr.SendConsoleMessage(S4Color.Green + $"Kicked {account.Nickname} out of room");
                        }

                        return true;
                    case "secs":
                        int.TryParse(args[2], out durationInSeconds);
                        break;
                    case "mins":
                        int.TryParse(args[2], out var durationInMinutes);
                        durationInSeconds = durationInMinutes * 60;
                        break;
                    case "days":
                        int.TryParse(args[2], out var durationInDays);
                        durationInSeconds = durationInDays * 24 * 60 * 60; //days->hours->minutes->seconds
                        break;
                    default:
                        if (DateTimeOffset.TryParseExact(args[1], "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out _))
                        {
                            if (args.Length >= 4)
                            {
                                int.TryParse(args[2], out var unk);
                                int.TryParse(args[3], out var durationInMs);
                                durationInSeconds = durationInMs / 60;
                            }
                        }
                        else
                        {
                            durationInSeconds = (int) TimeSpan.FromDays(10 * 365).TotalSeconds;
                        }

                        break;
                }

                using (var db = AuthDatabase.Open())
                {
                    account = db.Find<AccountDto>(statement => statement
                            .Include<BanDto>(join => join.LeftOuterJoin())
                            .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                            .WithParameters(new {Nickname = nickname}))
                        .FirstOrDefault();

                    if (account == null)
                    {
                        plr.SendConsoleMessage(S4Color.Red + "Unknown player");
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
                        Console.WriteLine(
                            $"{plr?.Account?.Nickname ?? "Unknown player"} has unbanned {account.Nickname}");
                        plr?.SendConsoleMessage(S4Color.Green + $"Unbanned {account.Nickname}");
                    }
                    else
                    {
                        var duration = TimeSpan.FromSeconds(durationInSeconds);

                        var ban = new BanDto
                        {
                            AccountId = account.Id,
                            Account = account,
                            Date = 0,
                            Duration = DateTimeOffset.Now.Add(duration).ToUnixTimeSeconds(),
                            Reason = "GMConsole"
                        };

                        var player = GameServer.Instance.PlayerManager.Get((ulong) account.Id);
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

                        Console.WriteLine(
                            $"{plr?.Account?.Nickname ?? "Unknown player"} has banned {account.Nickname} for {uptime}");
                        plr?.SendConsoleMessage(S4Color.Green + $"Banned {account.Nickname} for {uptime}");
                    }
                }
            }
            catch (Exception)
            {
                plr.SendConsoleMessage(S4Color.Red + "Unknown player");
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
