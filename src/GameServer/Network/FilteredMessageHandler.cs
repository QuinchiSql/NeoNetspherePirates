using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.Collections.Generic;
using BlubLib.DotNetty.Handlers.MessageHandling;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NeoNetsphere.Network.Message.Game;
using ProudNetSrc;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network
{
    internal class FilteredMessageHandler<TSession> : ProudMessageHandler
        where TSession : ProudSession
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName,
            nameof(FilteredMessageHandler<TSession>));

        private readonly IDictionary<Type, List<Predicate<TSession>>> _filter =
            new Dictionary<Type, List<Predicate<TSession>>>();

        private readonly IList<IMessageHandler> _messageHandlers = new List<IMessageHandler>();


        public override async Task<bool> OnMessageReceived(IChannelHandlerContext context, object message)
        {
            _filter.TryGetValue(message.GetType(), out var predicates);

            if (!GetParameter(context, message, out TSession session))
                throw new Exception("Unable to retrieve session");

            if (predicates != null && predicates.Any(predicate => !predicate(session)))
            {
                Logger.Debug("Dropping message {messageName} from client {remoteAddress}", message.GetType().Name,
                    ((ISocketChannel) context.Channel).RemoteAddress);
                return false;
            }

            HandleMessage(context, message);
            return true;
        }

        private async Task HandleMessage(IChannelHandlerContext context, object message)
        {
            if (message.GetType().Name == "RecvContext")
            {
                var recv = (RecvContext)message;
                Logger.Debug("message {msgtype}", recv.Message.GetType().Name);
            }

            if (!GetParameter(context, message, out TSession session))
                throw new Exception("Unable to retrieve session");
            var handled = false;
            foreach (var messageHandler in _messageHandlers)
            {
                var result = await messageHandler.OnMessageReceived(context, message);
                if (result)
                {
                    handled = true;
                }
            }
            if (handled || message.GetType().Name == "RecvContext")
                return;
            Logger.Error("Unhandled message {messageName}", message.GetType().Name);
            if (session.GetType() == typeof(GameSession))
                await session.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
        }

        public FilteredMessageHandler<TSession> AddHandler(IMessageHandler handler)
        {
            _messageHandlers.Add(handler);
            return this;
        }

        public FilteredMessageHandler<TSession> RegisterRule<T>(params Predicate<TSession>[] predicates)
        {
            if (predicates == null)
                throw new ArgumentNullException(nameof(predicates));

            _filter.AddOrUpdate(typeof(T),
                new List<Predicate<TSession>>(predicates),
                (key, oldValue) =>
                {
                    oldValue.AddRange(predicates);
                    return oldValue;
                });
            return this;
        }

        public FilteredMessageHandler<TSession> RegisterRule<T>(Predicate<TSession> predicate)
        {
            _filter.AddOrUpdate(typeof(T),
                new List<Predicate<TSession>> {predicate},
                (key, oldValue) =>
                {
                    oldValue.Add(predicate);
                    return oldValue;
                });
            return this;
        }
    }
}
