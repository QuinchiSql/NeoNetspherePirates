using System;
using BlubLib.DotNetty.Handlers.MessageHandling;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Network.Services;
using ProudNetSrc;
using ProudNetSrc.Serialization;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network
{
    internal class ChatServer : ProudServer
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ChatServer))
            ;

        private ChatServer(Configuration config)
            : base(config)
        {
        }

        public static ChatServer Instance { get; private set; }

        public static void Initialize(Configuration config)
        {
            if (Instance != null)
                throw new InvalidOperationException("Server is already initialized");

            config.Version = new Guid("{97d36acf-8cc0-4dfb-bcc9-97cab255e2bc}");
            config.MessageFactories = new MessageFactory[] {new ChatMessageFactory(), new ClubMessageFactory()};
            config.SessionFactory = new ChatSessionFactory();

            // ReSharper disable InconsistentNaming
            bool MustBeLoggedIn(ChatSession session) => session.IsLoggedIn();
            bool MustNotBeLoggedIn(ChatSession session) => !session.IsLoggedIn();
            bool MustBeInChannel(ChatSession session) => session.Player.Channel != null;
            // ReSharper restore InconsistentNaming

            config.MessageHandlers = new IMessageHandler[]
            {
                new FilteredMessageHandler<ChatSession>()
                    .AddHandler(new AuthService())
                    .AddHandler(new CommunityService())
                    .AddHandler(new ChannelService())
                    .AddHandler(new PrivateMessageService())
                    .AddHandler(new ClubService())
                    .RegisterRule<LoginReqMessage>(MustNotBeLoggedIn)
                    .RegisterRule<UserDataOneReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<DenyActionReqMessage>(MustBeLoggedIn)
                    .RegisterRule<MessageChatReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<MessageWhisperChatReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<NoteListReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<NoteReadReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<NoteDeleteReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<NoteSendReqMessage>(MustBeLoggedIn, MustBeInChannel)
            };
            Instance = new ChatServer(config);
        }

        #region Events

        protected override void OnDisconnected(ProudSession session)
        {
            ((ChatSession) session).GameSession?.Dispose();
            ((ChatSession) session).GameSession = null;
            base.OnDisconnected(session);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            var chatSession = (ChatSession) e.Session;
            var log = Logger;
            if (e.Session != null)
                log = log.ForAccount((ChatSession) e.Session);

            if (e.Exception.ToString().Contains("opcode") || e.Exception.ToString().Contains("Bad format in"))
            {
                log.Warning(e.Exception.InnerException.Message);
                if (chatSession != null && chatSession.GameSession != null)
                    chatSession.GameSession.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
            }
            else
            {
                log.Error(e.Exception, "Unhandled server error");
            }
            base.OnError(e);
        }

        //private void OnUnhandledMessage(object sender, MessageReceivedEventArgs e)
        //{
        //    var session = (ChatSession)e.Session;
        //    Log.Warning($"Unhandled message {e.Message.GetType().Name}");
        //}

        #endregion
    }
}
