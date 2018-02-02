using BlubLib;
using BlubLib.Collections.Concurrent;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.Threading;
using BlubLib.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ProudNetSrc.Codecs;
using ProudNetSrc.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;

namespace ProudNetSrc
{
    public class ProudServer : IDisposable
    {
        private readonly ConcurrentDictionary<uint, ProudSession> _sessions =
            new ConcurrentDictionary<uint, ProudSession>();

        private bool _disposed;
        private IChannel _listenerChannel;
        private IEventLoopGroup _listenerEventLoopGroup;
        private IEventLoopGroup _workerEventLoopGroup;
        private readonly ILoop _worker;

        public ProudServer(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (configuration.Version == null)
                throw new ArgumentNullException(nameof(configuration.Version));

            if (configuration.HostIdFactory == null)
                throw new ArgumentNullException(nameof(configuration.HostIdFactory));

            if (configuration.MessageFactories == null)
                throw new ArgumentNullException(nameof(configuration.MessageFactories));

            Configuration = configuration;
            Rsa = new RSACryptoServiceProvider(1024);
            P2PGroupManager = new P2PGroupManager(this);
            UdpSocketManager = new UdpSocketManager(this);
            _worker = new ThreadLoop(TimeSpan.FromMilliseconds(1000), Worker);
        }

        public bool IsRunning { get; private set; }
        public IReadOnlyDictionary<uint, ProudSession> Sessions => _sessions;
        public P2PGroupManager P2PGroupManager { get; }

        internal Configuration Configuration { get; }
        internal RSACryptoServiceProvider Rsa { get; }

        internal ConcurrentDictionary<uint, ProudSession> SessionsByUdpId { get; } =
            new ConcurrentDictionary<uint, ProudSession>();

        internal UdpSocketManager UdpSocketManager { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            OnStopping();
            UdpSocketManager.Dispose();
            _listenerChannel.CloseAsync().WaitEx();
            _listenerEventLoopGroup.ShutdownGracefullyAsync().WaitEx();
            _worker.Stop();
            _workerEventLoopGroup.ShutdownGracefullyAsync().WaitEx();
            Rsa.Dispose();
            OnStopped();
        }

        public void Listen(IPEndPoint tcpListener, IPAddress udpAddress = null, int[] udpListenerPorts = null,
            IEventLoopGroup listenerEventLoopGroup = null, IEventLoopGroup workerEventLoopGroup = null)
        {
            ThrowIfDisposed();
            _worker.Start();
            _listenerEventLoopGroup = listenerEventLoopGroup ?? new MultithreadEventLoopGroup(1);
            _workerEventLoopGroup = workerEventLoopGroup ?? new MultithreadEventLoopGroup();
            try
            {
                _listenerChannel = new ServerBootstrap()
                    .Group(_listenerEventLoopGroup, _workerEventLoopGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Handler(new ActionChannelInitializer<IServerSocketChannel>(ch => { }))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(ch =>
                    {
                        var userMessageHandler = new SimpleMessageHandler();
                        foreach (var handler in Configuration.MessageHandlers)
                            userMessageHandler.Add(handler);

                        ch.Pipeline
                            .AddLast(new SessionHandler(this))
                            .AddLast(new ProudFrameDecoder((int) Configuration.MessageMaxLength))
                            .AddLast(new ProudFrameEncoder())
                            .AddLast(new CoreMessageDecoder())
                            .AddLast(new CoreMessageEncoder())
                            .AddLast("coreHandler", new SimpleMessageHandler()
                                .Add(new CoreHandler(this)))
                            .AddLast(new SendContextEncoder())
                            .AddLast(new MessageDecoder(Configuration.MessageFactories))
                            .AddLast(new MessageEncoder(Configuration.MessageFactories))

                            // SimpleMessageHandler discards all handled messages
                            // So internal messages(if handled) wont reach the user messagehandler
                            .AddLast(new SimpleMessageHandler()
                                .Add(new ServerHandler()))
                            .AddLast(userMessageHandler)
                            .AddLast(new ErrorHandler(this));
                    }))
                    .ChildOption(ChannelOption.TcpNodelay, !Configuration.EnableNagleAlgorithm)
                    .ChildOption(ChannelOption.AllowHalfClosure, false)
                    .ChildOption(ChannelOption.SoSndbuf, Configuration.MaxClientSocketBuf)
                    .ChildAttribute(ChannelAttributes.Session, default(ProudSession))
                    .ChildAttribute(ChannelAttributes.Server, this)
                    .BindAsync(tcpListener).WaitEx();

                if (udpListenerPorts != null)
                    UdpSocketManager.Listen(udpAddress, tcpListener.Address, udpListenerPorts, _workerEventLoopGroup);
            }
            catch (Exception ex)
            {
                _listenerEventLoopGroup.ShutdownGracefullyAsync();
                _listenerEventLoopGroup = null;
                _workerEventLoopGroup.ShutdownGracefullyAsync();
                _workerEventLoopGroup = null;
                ex.Rethrow();
            }

            IsRunning = true;
            OnStarted();
        }

        private void Worker(TimeSpan delta)
        {
            foreach (var session in _sessions.Values)
            {
                var diff = DateTimeOffset.Now - session.LastHeartBeatOrMessage;
                if (diff >= this.Configuration.IdleTimeout)
                {
                    session?.Dispose();
                }
                //Todo
                //if (this.Configuration.EnablePingTest)
                //{
                //    if (session.PingTestWorkerCount != 5)
                //        session.PingTestWorkerCount++;
                //    else
                //    {
                //        session.PingTestWorkerCount = 0;
                //
                //        CMessage msg = new CMessage();
                //        msg.Write((byte)ProudCoreOpCode.ServerPingTest);
                //        session.SendAsync(new S2CRoutedMulticast1Message(MessagePriority.High, 0, msg.Buffer));
                //    }
                //}
            }
        }

        public void Broadcast(object message, SendOptions options)
        {
            foreach (var session in Sessions.Values)
                session?.SendAsync(message, options);
        }

        public void Broadcast(object message)
        {
            foreach (var session in Sessions.Values)
                session?.SendAsync(message);
        }

        internal void AddSession(ProudSession session)
        {
            _sessions[session.HostId] = session;
            OnConnected(session);
        }

        internal void RemoveSession(ProudSession session)
        {
            _sessions.Remove(session.HostId);
            SessionsByUdpId.Remove(session.UdpSessionId);
            OnDisconnected(session);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #region Events

        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;

        public event EventHandler<ProudSession> Connected;
        public event EventHandler<ProudSession> Disconnected;

        public event EventHandler<ErrorEventArgs> Error;

        protected virtual void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopping()
        {
            Stopping?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConnected(ProudSession session)
        {
            Connected?.Invoke(this, session);
        }

        protected virtual void OnDisconnected(ProudSession session)
        {
            Disconnected?.Invoke(this, session);
        }

        protected virtual void OnIncative(ProudSession session)
        {
            Disconnected?.Invoke(this, session);
        }

        protected virtual void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        internal void RaiseError(ErrorEventArgs e)
        {
            OnError(e);
        }

        #endregion
    }
}
