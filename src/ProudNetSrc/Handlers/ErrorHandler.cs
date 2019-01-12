﻿namespace ProudNetSrc.Handlers
{
    using System;
    using System.Net.Sockets;
    using DotNetty.Transport.Channels;

    internal class ErrorHandler : ChannelHandlerAdapter
    {
        private readonly ProudServer _server;

        public ErrorHandler(ProudServer server)
        {
            _server = server;
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            var session = context.Channel.GetAttribute(ChannelAttributes.Session).Get();
            if (exception.GetType() == typeof(SocketException))
            {
                session.CloseAsync();
            }
            else
            {
                _server.Configuration.Logger?.Error(exception, "Unhandled exception");
                _server.RaiseError(new ErrorEventArgs(session, exception));
            }
        }
    }
}
