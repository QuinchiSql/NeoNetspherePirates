using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
        
        [MessageHandler(typeof(LoginEUReqMessage))]
        public async Task EULoginHandler(ProudSession session, LoginEUReqMessage message)
        {
            var ip = session.RemoteEndPoint.Address.ToString();

            var account = new AccountDto();
            using (var db = AuthDatabase.Open())
            {
                if (message.Username != "" && message.Password != "")
                {
                    Logger.Information($"Login from {ip}");

                    if (message.Username.Length > 5 && message.Password.Length > 5)
                    {
                        if (!Namecheck.IsNameValid(message.Username))
                        {
                            await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.WrongIdorPw));
                            Logger.Error("Wrong login for {ip}", ip);
                            return;
                        }
                        
                        var result = db.Find<AccountDto>(statement => statement
                            .Where($"{nameof(AccountDto.Username):C} = @{nameof(message.Username)}")
                            .Include<BanDto>(join => join.LeftOuterJoin())
                            .WithParameters(new {message.Username}));

                        account = result.FirstOrDefault();

                        if (account == null && (Config.Instance.NoobMode || Config.Instance.AutoRegister))
                        {
                            account = new AccountDto {Username = message.Username};

                            var newSalt = new byte[24];
                            using (var csprng = new RNGCryptoServiceProvider())
                            {
                                csprng.GetBytes(newSalt);
                            }

                            var hash = new Rfc2898DeriveBytes(message.Password, newSalt, 24000).GetBytes(24);

                            account.Password = Convert.ToBase64String(hash);
                            account.Salt = Convert.ToBase64String(newSalt);
                            await db.InsertAsync(account);
                        }

                        var salt = Convert.FromBase64String(account?.Salt ?? "");

                        var passwordGuess = new Rfc2898DeriveBytes(message.Password, salt, 24000).GetBytes(24);
                        var actualPassword = Convert.FromBase64String(account?.Password ?? "");

                        var difference = (uint) passwordGuess.Length ^ (uint) actualPassword.Length;

                        for (var i = 0;
                            i < passwordGuess.Length && i < actualPassword.Length;
                            i++)
                            difference |= (uint) (passwordGuess[i] ^ actualPassword[i]);

                        if ((difference != 0 ||
                             string.IsNullOrWhiteSpace(account?.Password ?? "")) &&
                            !Config.Instance.NoobMode)
                        {
                            await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.WrongIdorPw));
                            Logger.Error("Wrong login for {ip}", ip);
                            return;
                        }

                        if (account != null)
                        {
                            account.AuthToken = "";
                            account.newToken = "";
                            await db.UpdateAsync(account);
                        }
                    }
                    else
                    {
                        await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.WrongIdorPw));
                        Logger.Error("Wrong login for {ip}", ip);
                        return;
                    }
                }
                else if (message.token != "")
                {
                    Logger.Information($"Login from {ip}");

                    var result = await db.FindAsync<AccountDto>(statement => statement
                        .Where($"{nameof(AccountDto.LoginToken):C} = @{nameof(message.token)}")
                        .Include<BanDto>(join => join.LeftOuterJoin())
                        .WithParameters(new {message.token}));
                    account = result.FirstOrDefault();

                    if (account != null)
                    {
                        var lastlogin = DateTimeOffset.ParseExact(account.LastLogin, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                            DateTimeStyles.None);

                        if ((DateTimeOffset.Now - lastlogin).Minutes >= 5)
                        {
                            await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong login for {ip}", ip);
                            return;
                        }
                    }
                    else
                    {
                        await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
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
                            await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                            Logger.Error("Wrong session login for {ip} ({AuthToken}, {newToken})", ip,
                                account.AuthToken, account.newToken);
                            return;
                        }
                    }
                    else
                    {
                        await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.Failed2));
                        Logger.Error("Wrong session login for {ip}", ip);
                        return;
                    }
                }

                if (account == null)
                    return;

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var ban = account.Bans.FirstOrDefault(b => b.Date + (b.Duration ?? 0) > now);
                if (ban != null)
                {
                    var unbanDate = DateTimeOffset.FromUnixTimeSeconds(ban.Date + (ban.Duration ?? 0));
                    Logger.Error("{user} is banned until {until}", account.Username, unbanDate);
                    await session.SendAsync(new LoginEUAckMessage(unbanDate));
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


            var datetime = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
            var sessionId = Hash.GetUInt32<CRC32>($"<{account.Username}+{account.Password}>");
            var authsessionId = Hash.GetString<CRC32>($"<{account.Username}+{sessionId}+{datetime}>");
            var newsessionId = Hash.GetString<CRC32>($"<{authsessionId}+{sessionId}>");

            using (var db = AuthDatabase.Open())
            {
                account.LastLogin = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
                account.LoginToken = "";
                account.AuthToken = authsessionId;
                account.newToken = newsessionId;
                await db.UpdateAsync(account);
            }
            await session.SendAsync(new LoginEUAckMessage(AuthLoginResult.OK, (ulong)account.Id, sessionId, authsessionId,
                newsessionId, datetime));
        }

        [MessageHandler(typeof(ServerListReqMessage))]
        public async Task ServerListHandler(AuthServer server, ProudSession session)
        {
            await session.SendAsync(new ServerListAckMessage(server.ServerManager.ToArray()));
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
            foreach (var xbn in Enum.GetValues(typeof(XBNType)).Cast<XBNType>().ToList())
            {
                if (Program.XBNdata.TryGetValue(xbn, out var xbninfo))
                {
                    var readoffset = 0;
                    while (readoffset != xbninfo.Length)
                    {
                        var size = xbninfo.Length - readoffset;

                        if (size > 40000)
                            size = 40000;

                        var data = new byte[size];
                        Array.Copy(xbninfo, readoffset, data, 0, size);

                        await session.SendAsync(new GameDataAckMessage((uint)xbn, data, (uint)xbninfo.Length), SendOptions.ReliableSecureCompress);
                        readoffset += size;
                    }
                }
            }
        }
    }
}
