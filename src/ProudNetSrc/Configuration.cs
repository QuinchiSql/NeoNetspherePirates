using System;
using BlubLib.DotNetty.Handlers.MessageHandling;
using ProudNetSrc.Serialization;
using Serilog;

namespace ProudNetSrc
{
    public class Configuration
    {
        public static int MaxClientSocketBuf = 50000;

        public Configuration()
        {
            Version = Guid.Empty;
            HostIdFactory = new HostIdFactory();
            P2PGroupHostIdFactory = new P2PGroupHostIdFactory();
            SessionFactory = new ProudSessionFactory();
            ConnectTimeout = TimeSpan.FromSeconds(10);
            MaxUncompressedMessageLength = 60000;

            EnableServerLog = false;
            FallbackMethod = FallbackMethod.None;
            MessageMaxLength = 1048576;
            IdleTimeout = TimeSpan.FromSeconds(20);
            DirectP2PStartCondition = DirectP2PStartCondition.Jit;
            OverSendSuspectingThresholdInBytes = 15360;
            EnableNagleAlgorithm = true;
            EncryptedMessageKeyLength = 128;
            FastEncryptedMessageKeyLength = 0;
            AllowServerAsP2PGroupMember = false;
            EnableP2PEncryptedMessaging = false;
            UpnpDetectNatDevice = true;
            UpnpTcpAddrPortMapping = true;
            EnableLookaheadP2PSend = false;
            EnablePingTest = false;
            EmergencyLogLineCount = 0;
        }

        public Guid Version { get; set; }
        public IHostIdFactory HostIdFactory { get; set; }
        public IP2PGroupHostIdFactory P2PGroupHostIdFactory { get; set; }
        public ISessionFactory SessionFactory { get; set; }
        public TimeSpan ConnectTimeout { get; set; }
        public MessageFactory[] MessageFactories { get; set; }
        public IMessageHandler[] MessageHandlers { get; set; }
        public uint MaxUncompressedMessageLength { get; set; }
        public ILogger Logger { get; set; }


        public bool EnableServerLog { get; set; }
        public FallbackMethod FallbackMethod { get; set; }
        public uint MessageMaxLength { get; set; }
        public TimeSpan IdleTimeout { get; set; }
        public DirectP2PStartCondition DirectP2PStartCondition { get; set; }
        public uint OverSendSuspectingThresholdInBytes { get; set; }
        public bool EnableNagleAlgorithm { get; set; }
        public int EncryptedMessageKeyLength { get; set; }
        public int FastEncryptedMessageKeyLength { get; set; }
        public bool AllowServerAsP2PGroupMember { get; set; }
        public bool EnableP2PEncryptedMessaging { get; set; }
        public bool UpnpDetectNatDevice { get; set; }
        public bool UpnpTcpAddrPortMapping { get; set; }
        public bool EnableLookaheadP2PSend { get; set; }
        public bool EnablePingTest { get; set; }
        public uint EmergencyLogLineCount { get; set; }
    }
}
