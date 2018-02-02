using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.Security.Cryptography;
using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Network.Message.Auth;
using ProudNetSrc;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network.Service
{
    internal class AuthService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthService));
        
        [MessageHandler(typeof(LoginKRReqMessage))]
        public async Task KRLoginHandler(ProudSession session, LoginKRReqMessage message)
        {

            var ip = session.RemoteEndPoint.Address.ToString();

            var account = new AccountDto();
            using (var db = AuthDatabase.Open())
            {
                if (message.AccountHashCode != "")
                {
                    Logger.Information($"Login from {ip}");

                    var result = await db.FindAsync<AccountDto>(statement => statement
                        .Where($"{nameof(AccountDto.LoginToken):C} = @{nameof(message.AccountHashCode)}")
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .WithParameters(new { message.AccountHashCode }));
                    account = result.FirstOrDefault();

                    if (account != null)
                    {
                        if ((DateTimeOffset.Now - DateTimeOffset.Parse(account.LastLogin)).Minutes >= 5)
                        {
                            session.SendAsync(new LoginKRAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong login for {ip}", ip);
                            return;
                        }
                    }
                    else
                    {
                        session.SendAsync(new LoginKRAckMessage(AuthLoginResult.Failed2));
                        Logger.Error("Wrong login for {ip}", ip);
                        return;
                    }
                }
                else if (message.AuthToken != "" && message.NewToken != "")
                {
                    Logger.Information("Session login from {ip}", ip);

                    var result = await db.FindAsync<AccountDto>(statement => statement
                        .Where($"{nameof(AccountDto.AuthToken):C} = @{nameof(message.AuthToken)}")
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .WithParameters(new { message.AuthToken }));
                    account = result.FirstOrDefault();

                    if (account != null)
                    {
                        if (account.AuthToken != message.AuthToken && account.newToken != message.NewToken)
                        {
                            session.SendAsync(new LoginKRAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong session login for {ip} ({AuthToken}, {newToken})", ip,
                                account.AuthToken, account.newToken);
                            return;
                        }
                    }
                    else
                    {
                        session.SendAsync(new LoginKRAckMessage(AuthLoginResult.Failed2));
                        Logger.Error("Wrong session login for {ip}", ip);
                        return;
                    }
                }

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var ban = account.Bans.FirstOrDefault(b => b.Date + (b.Duration ?? 0) > now);
                if (ban != null)
                {
                    var unbanDate = DateTimeOffset.FromUnixTimeSeconds(ban.Date + (ban.Duration ?? 0));
                    Logger.Error("{user} is banned until {until}", account.Username, unbanDate);
                    session.SendAsync(new LoginKRAckMessage(unbanDate));
                    return;
                }

                Logger.Information("Login success for {user}", account.Username);

                var entry = new LoginHistoryDto
                {
                    AccountId = account.Id,
                    Date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    IP = ip
                };
                await db.InsertAsync(entry);
            }


            var datetime = $"{DateTimeOffset.Now.DateTime}";
            var sessionId = Hash.GetUInt32<CRC32>($"<{account.Username}+{account.Password}>");
            var authsessionId = Hash.GetString<CRC32>($"<{account.Username}+{sessionId}+{datetime}>");
            var newsessionId = Hash.GetString<CRC32>($"<{authsessionId}+{sessionId}>");

            using (var db = AuthDatabase.Open())
            {
                account.LoginToken = "";
                account.AuthToken = authsessionId;
                account.newToken = newsessionId;
                await db.UpdateAsync(account);
            }
            session.SendAsync(new LoginKRAckMessage(AuthLoginResult.OK, (ulong)account.Id, sessionId, authsessionId,
                newsessionId, datetime));
        }

        [MessageHandler(typeof(LoginEUReqMessage))]
        public async Task EULoginHandler(ProudSession session, LoginEUReqMessage message)
        {
            var ip = session.RemoteEndPoint.Address.ToString();

            var account = new AccountDto();
            using (var db = AuthDatabase.Open())
            {
                if (message.token != "")
                {
                    Logger.Information($"Login from {ip}");

                    var result = await db.FindAsync<AccountDto>(statement => statement
                        .Where($"{nameof(AccountDto.LoginToken):C} = @{nameof(message.token)}")
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .WithParameters(new {message.token}));
                    account = result.FirstOrDefault();

                    if (account != null)
                    {
                        if ((DateTimeOffset.Now - DateTimeOffset.Parse(account.LastLogin)).Minutes >= 5)
                        {
                            session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong login for {ip}", ip);
                            return;
                        }
                    }
                    else
                    {
                        session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                        Logger.Error("Wrong login for {ip}", ip);
                        return;
                    }
                }
                else if (message.AuthToken != "" && message.NewToken != "")
                {
                    Logger.Information("Session login from {ip}", ip);

                    var result = await db.FindAsync<AccountDto>(statement => statement
                        .Where($"{nameof(AccountDto.AuthToken):C} = @{nameof(message.AuthToken)}")
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .WithParameters(new {message.AuthToken}));
                    account = result.FirstOrDefault();

                    if (account != null)
                    {
                        if (account.AuthToken != message.AuthToken && account.newToken != message.NewToken)
                        {
                            session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong session login for {ip} ({AuthToken}, {newToken})", ip,
                                account.AuthToken, account.newToken);
                            return;
                        }
                    }
                    else
                    {
                        session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                        Logger.Error("Wrong session login for {ip}", ip);
                        return;
                    }
                }

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var ban = account.Bans.FirstOrDefault(b => b.Date + (b.Duration ?? 0) > now);
                if (ban != null)
                {
                    var unbanDate = DateTimeOffset.FromUnixTimeSeconds(ban.Date + (ban.Duration ?? 0));
                    Logger.Error("{user} is banned until {until}", account.Username, unbanDate);
                    session.SendAsync(new LoginEUAckMessage(unbanDate));
                    return;
                }

                Logger.Information("Login success for {user}", account.Username);

                var entry = new LoginHistoryDto
                {
                    AccountId = account.Id,
                    Date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    IP = ip
                };
                await db.InsertAsync(entry);
            }


            var datetime = $"{DateTimeOffset.Now.DateTime}";
            var sessionId = Hash.GetUInt32<CRC32>($"<{account.Username}+{account.Password}>");
            var authsessionId = Hash.GetString<CRC32>($"<{account.Username}+{sessionId}+{datetime}>");
            var newsessionId = Hash.GetString<CRC32>($"<{authsessionId}+{sessionId}>");

            using (var db = AuthDatabase.Open())
            {
                account.LoginToken = "";
                account.AuthToken = authsessionId;
                account.newToken = newsessionId;
                await db.UpdateAsync(account);
            }
            session.SendAsync(new LoginEUAckMessage(AuthLoginResult.OK, (ulong) account.Id, sessionId, authsessionId,
                newsessionId, datetime));
        }

        [MessageHandler(typeof(ServerListReqMessage))]
        public async Task ServerListHandler(AuthServer server, ProudSession session)
        {
            session.SendAsync(new ServerListAckMessage(server.ServerManager.ToArray()));
        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            hexString = hexString.Replace("-", ""); // remove '-' symbols

            var result = new byte[hexString.Length / 2];

            for (var i = 0; i < hexString.Length; i += 2)
                result[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16); // base 16

            return result;
        }

        [MessageHandler(typeof(GameDataReqMessage))]
        public async Task DataHandler(AuthServer server, ProudSession session)
        {
            var file1 = File.ReadAllText(@"XBN\XBNFILE_1");
            var file2 = File.ReadAllText(@"XBN\XBNFILE_2");
            var file3 = File.ReadAllText(@"XBN\XBNFILE_3");
            var file4 = File.ReadAllText(@"XBN\XBNFILE_4");
            var file5 = File.ReadAllText(@"XBN\XBNFILE_5");
            var file6 = File.ReadAllText(@"XBN\XBNFILE_6");
            var file7 = File.ReadAllText(@"XBN\XBNFILE_7");
            var file8 = File.ReadAllText(@"XBN\XBNFILE_8");
            var file9 = File.ReadAllText(@"XBN\XBNFILE_9");
            var file10 = File.ReadAllText(@"XBN\XBNFILE_10");
            var file11 = File.ReadAllText(@"XBN\XBNFILE_11");
            var file12 = File.ReadAllText(@"XBN\XBNFILE_12");
            var file13 = File.ReadAllText(@"XBN\XBNFILE_13");
            var file14 = File.ReadAllText(@"XBN\XBNFILE_14");
            var file15 = File.ReadAllText(@"XBN\XBNFILE_15");


            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file1)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file2)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file3)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file4)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file5)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file6)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file7)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file8)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file9)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file10)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file11)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file12)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file13)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file14)), SendOptions.ReliableCompress);
            await session.SendAsync(new GameDataAckMessage(HexStringToByteArray(file15)), SendOptions.ReliableCompress);
        }
    }
}
