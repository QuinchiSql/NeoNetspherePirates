using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;

namespace NeoNetsphere.Commands
{
    internal class AdminCommands : ICommand
    {
        public AdminCommands()
        {
            Name = "admin_cmd";
            AllowConsole = true;
            Permission = SecurityLevel.GameMaster;
            SubCommands = new ICommand[] { new RenameCommand(), new SecurityCommand(), new LevelCommand() };
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

        private class RenameCommand : ICommand
        {
            public RenameCommand()
            {
                Name = "rename";
                AllowConsole = false;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[] { };
            }

            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 2)
                {
                    plr.SendConsoleMessage(S4Color.Red + "Wrong Usage, possible usages:");
                    plr.SendConsoleMessage(S4Color.Red + "> rename <username> <newname>");
                    return true;
                }

                if(args.Length >= 2)
                {
                    using (var db = AuthDatabase.Open())
                    {
                        var account = (db.Find<AccountDto>(statement => statement
                                .Include<BanDto>(join => @join.LeftOuterJoin())
                                .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                                .WithParameters(new { Nickname = args[0] })))
                            .FirstOrDefault();

                        if (account == null)
                        {
                            plr.SendConsoleMessage(S4Color.Red + "Unknown player!");
                            return true;
                        }

                        plr.SendConsoleMessage(S4Color.Green + $"Changed {account.Nickname}'s nickname to {args[1]}");
                        account.Nickname = args[1];
                        db.Update(account);

                        var player = GameServer.Instance.PlayerManager.Get((ulong)account.Id);
                        player?.Session?.SendAsync(new ItemUseChangeNickAckMessage() { Result = 0 });
                        player?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
                    }
                }
                
                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class SecurityCommand : ICommand
        {
            public SecurityCommand()
            {
                Name = "seclevel";
                AllowConsole = false;
                Permission = SecurityLevel.Administrator;
                SubCommands = new ICommand[] { };
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
                    plr.SendConsoleMessage(S4Color.Red + "> seclevel <username> <level>");
                    return true;
                }

                if (args.Length >= 2)
                {
                    using (var db = AuthDatabase.Open())
                    {
                        var account = (db.Find<AccountDto>(statement => statement
                                .Include<BanDto>(join => @join.LeftOuterJoin())
                                .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                                .WithParameters(new { Nickname = args[0] })))
                            .FirstOrDefault();

                        if (account == null)
                        {
                            plr.SendConsoleMessage(S4Color.Red + "Unknown player!");
                            return true;
                        }
                        else
                        {
                            if (byte.TryParse(args[1], out var level))
                            {
                                plr.SendConsoleMessage(S4Color.Green + $"Changed {account.Nickname}'s seclevel to {args[1]}");
                                account.SecurityLevel = level;
                                db.Update(account);

                                var player = GameServer.Instance.PlayerManager.Get((ulong)account.Id);
                                player?.Session?.SendAsync(new ItemUseChangeNickAckMessage() { Result = 0 });
                                player?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
                            }
                            else
                            {
                                plr.SendConsoleMessage(S4Color.Red + "Wrong Usage, possible usages:");
                                plr.SendConsoleMessage(S4Color.Red + "> seclevel <username> <level>");
                                plr.SendConsoleMessage(S4Color.Red + "> seclevel <level>");
                            }

                        }
                    }
                }

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class LevelCommand : ICommand
        {
            public LevelCommand()
            {
                Name = "level";
                AllowConsole = false;
                Permission = SecurityLevel.Administrator;
                SubCommands = new ICommand[] { };
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
                    plr.SendConsoleMessage(S4Color.Red + "> level <username> <level>");
                    return true;
                }

                if (args.Length >= 2)
                {
                    AccountDto account;
                    PlayerDto playerdto;
                    using (var db = AuthDatabase.Open())
                    {
                        account = (db.Find<AccountDto>(statement => statement
                               .Include<BanDto>(join => join.LeftOuterJoin())
                               .Where($"{nameof(AccountDto.Nickname):C} = @Nickname")
                               .WithParameters(new { Nickname = args[0] })))
                            .FirstOrDefault();
                        
                        if (account == null)
                        {
                            plr.SendConsoleMessage(S4Color.Red + "Unknown player!");
                            return true;
                        }

                        playerdto = (db.Find<PlayerDto>(statement => statement
                               .Where($"{nameof(PlayerDto.Id):C} = @Id")
                               .WithParameters(new { account.Id })))
                            .FirstOrDefault();

                        if (playerdto == null)
                        {
                            plr.SendConsoleMessage(S4Color.Red + "Unknown player!");
                            return true;
                        }

                        if (byte.TryParse(args[1], out var level))
                        {
                            var expTable = GameServer.Instance.ResourceCache.GetExperience();

                            if (expTable.TryGetValue(level, out var exp))
                            {
                                plr.SendConsoleMessage(S4Color.Green + $"Changed {account.Nickname}'s level to {args[1]}");
                                
                                plr.TotalExperience = exp.TotalExperience;
                                playerdto.Level = level;
                                playerdto.TotalExperience = exp.TotalExperience;

                                db.Update(playerdto);

                                var player = GameServer.Instance.PlayerManager.Get((ulong)account.Id);
                                if (player != null)
                                {
                                    player.Level = level;
                                    player.TotalExperience = exp.TotalExperience;
                                    player.Session?.SendAsync(new ExpRefreshInfoAckMessage(player.TotalExperience));
                                    player.Session?.SendAsync(new PlayerAccountInfoAckMessage(player.Map<Player, PlayerAccountInfoDto>()));
                                    player.NeedsToSave = true;
                                }
                            }
                            else
                            {
                                plr.SendConsoleMessage(S4Color.Red + "Invalid Level");
                            }
                        }
                        else
                        {
                            plr.SendConsoleMessage(S4Color.Red + "Wrong Usage, possible usages:");
                            plr.SendConsoleMessage(S4Color.Red + "> level <username> <level>");
                        }
                    }
                }

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }
    }
}
