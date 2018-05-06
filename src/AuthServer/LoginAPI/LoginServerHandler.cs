using System;
using System.Linq;
using System.Security.Cryptography;
using Dapper.FastCrud;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NeoNetsphere.Database.Auth;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.LoginAPI
{
    public class LoginServerHandler : ChannelHandlerAdapter
    {
        private const short Magic = 0x5713;
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, "LoginServer");

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            var firstMessage = new CCMessage();
            firstMessage.Write(CCMessage.MessageType.Notify);
            firstMessage.Write("<region>EU-S4MAX</region>");
            SendA(context, firstMessage);
        }

        public override void ChannelRead(IChannelHandlerContext context, object messageData)
        {
            var buffer = messageData as IByteBuffer;
            var data = new byte[0];
            if (buffer != null)
                data = buffer.ToArray();

            var msg = new CCMessage(data, data.Length);
            short magic = 0;
            var message = new ByteArray();

            if (msg.Read(ref magic)
                && magic == Magic
                && msg.Read(ref message))
            {
                var receivedMessage = new CCMessage(message);
                CCMessage.MessageType coreId = 0;
                if (!receivedMessage.Read(ref coreId))
                    return;

                switch (coreId)
                {
                    case CCMessage.MessageType.Rmi:
                        short rmiId = 0;
                        if (receivedMessage.Read(ref rmiId))
                            switch (rmiId)
                            {
                                case 15:
                                    {
                                        var username = "";
                                        var password = "";
                                        var register = false;

                                        if (receivedMessage.Read(ref username)
                                        && receivedMessage.Read(ref password)
                                        && receivedMessage.Read(ref register))
                                        {
                                            using (var db = AuthDatabase.Open())
                                            {
                                                Logger.Information("Authentication login from {endpoint}",
                                                    context.Channel.RemoteAddress.ToString());

                                                if (username.Length > 5 && password.Length > 5 && Namecheck.IsNameValid(username))
                                                {
                                                    var result = db.Find<AccountDto>(statement => statement
                                                        .Where($"{nameof(AccountDto.Username):C} = @{nameof(username)}")
                                                        .Include<BanDto>(join => @join.LeftOuterJoin())
                                                        .WithParameters(new { Username = username }));

                                                    var account = result.FirstOrDefault();

                                                    if (account == null &&
                                                        (Config.Instance.NoobMode || Config.Instance.AutoRegister))
                                                    {
                                                        account = new AccountDto { Username = username };

                                                        var newSalt = new byte[24];
                                                        using (var csprng = new RNGCryptoServiceProvider())
                                                        {
                                                            csprng.GetBytes(newSalt);
                                                        }

                                                        var hash = new Rfc2898DeriveBytes(password, newSalt, 24000).GetBytes(24);

                                                        account.Password = Convert.ToBase64String(hash);
                                                        account.Salt = Convert.ToBase64String(newSalt);
                                                        db.InsertAsync(account);
                                                    }

                                                    var salt = Convert.FromBase64String(account?.Salt ?? "");

                                                    var passwordGuess = new Rfc2898DeriveBytes(password, salt, 24000).GetBytes(24);
                                                    var actualPassword = Convert.FromBase64String(account?.Password ?? "");

                                                    var difference =
                                                        (uint)passwordGuess.Length ^ (uint)actualPassword.Length;

                                                    for (var i = 0;
                                                        i < passwordGuess.Length && i < actualPassword.Length;
                                                        i++)
                                                        difference |= (uint)(passwordGuess[i] ^ actualPassword[i]);

                                                    if ((difference != 0 ||
                                                         string.IsNullOrWhiteSpace(account?.Password ?? "")) &&
                                                        !Config.Instance.NoobMode)
                                                    {
                                                        Logger.Error(
                                                            "Wrong authentication credentials for {username} / {endpoint}",
                                                            username, context.Channel.RemoteAddress.ToString());
                                                        var ack = new CCMessage();
                                                        ack.Write(false);
                                                        ack.Write("Login failed");
                                                        RmiSend(context, 16, ack);
                                                    }
                                                    else
                                                    {
                                                        if (account != null)
                                                        {
                                                            account.LoginToken = AuthHash
                                                                .GetHash256(
                                                                    $"{context.Channel.RemoteAddress}-{account.Username}-{account.Password}")
                                                                .ToLower();
                                                            account.AuthToken = "";
                                                            account.newToken = "";
                                                            db.UpdateAsync(account);

                                                            var ack = new CCMessage();
                                                            ack.Write(true);
                                                            ack.Write(account.LoginToken);
                                                            RmiSend(context, 16, ack);
                                                        }
                                                        Logger.Information("Authentication success for {username}",
                                                            username);
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.Error(
                                                        "Wrong authentication credentials for {username} / {endpoint}",
                                                        username, context.Channel.RemoteAddress.ToString());
                                                    var ack = new CCMessage();
                                                    ack.Write(false);
                                                    ack.Write("Invalid length of username/password");
                                                    RmiSend(context, 16, ack);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logger.Error("Wrong login for {endpoint}",
                                                context.Channel.RemoteAddress.ToString());
                                            var ack = new CCMessage();
                                            ack.Write(false);
                                            ack.Write("Invalid loginpacket");
                                            RmiSend(context, 16, ack);
                                        }
                                        break;
                                    }
                                case 17:
                                    context.CloseAsync();
                                    break;
                                default:
                                    Logger.Error("Received unknown rmiId{rmi} from {endpoint}", rmiId,
                                        context.Channel.RemoteAddress.ToString());
                                    break;
                            }
                        break;
                    case CCMessage.MessageType.Notify:
                        context.CloseAsync();
                        break;
                    default:
                        Logger.Error("Received unknown coreID{coreid} from {endpoint}", coreId,
                            context.Channel.RemoteAddress.ToString());
                        break;
                }
            }
            else
            {
                Logger.Error("Received invalid packetstruct from {endpoint}", context.Channel.RemoteAddress.ToString());
                context.CloseAsync();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
#if DEBUG
        Logger.Error("Exception: " + exception);
#endif
            context.CloseAsync();
        }

        private static void RmiSend(IChannelHandlerContext ctx, short rmiId, CCMessage message)
        {
            var rmiframe = new CCMessage();
            rmiframe.Write(CCMessage.MessageType.Rmi);
            rmiframe.Write(rmiId);
            rmiframe.Write(message);
            SendA(ctx, rmiframe);
        }

        private static void SendA(IChannelHandlerContext ctx, CCMessage data)
        {
            var coreframe = new CCMessage();
            coreframe.Write(Magic);
            coreframe.WriteScalar(data.Length);
            coreframe.Write(data);

            var buffer = Unpooled.Buffer(coreframe.Length);
            buffer.WriteBytes(coreframe.Buffer);
            ctx.WriteAndFlushAsync(buffer);
        }
    }
}
