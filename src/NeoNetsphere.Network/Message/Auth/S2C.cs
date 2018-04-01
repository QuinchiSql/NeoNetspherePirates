using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Data.Auth;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Auth
{
    [BlubContract]
    public class LoginKRAckMessage : IAuthMessage
    {
        public LoginKRAckMessage()
        {
            Unk1 = "9";
            SessionId2 = "";
            Unk2 = "";
            BannedUntil = "";
        }

        public LoginKRAckMessage(DateTimeOffset bannedUntil)
            : this()
        {
            Result = AuthLoginResult.Banned;
            BannedUntil = bannedUntil.ToString("yyyyMMddHHmmss");
        }

        public LoginKRAckMessage(AuthLoginResult result)
            : this()
        {
            Result = result;
        }

        public LoginKRAckMessage(AuthLoginResult result, ulong accountId, uint sessionId, string authsession,
            string newsession, string datetime)
            : this()
        {
            Result = result;
            AccountId = accountId;
            SessionId = (uint)accountId;
            SessionId2 = sessionId.ToString();
            AuthToken = authsession;
            NewToken = newsession;
            Datetime = datetime;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public uint SessionId { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string SessionId2 { get; set; }

        [BlubMember(4)]
        public AuthLoginResult Result { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string BannedUntil { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string AuthToken { get; set; }

        [BlubMember(9, typeof(StringSerializer))]
        public string NewToken { get; set; }

        [BlubMember(10, typeof(StringSerializer))]
        public string Datetime { get; set; }
    }

    [BlubContract]
    public class LoginEUAckMessage : IAuthMessage
    {
        public LoginEUAckMessage()
        {
            Unk1 = "";
            SessionId2 = "";
            Unk2 = "";
            BannedUntil = "";
        }

        public LoginEUAckMessage(DateTimeOffset bannedUntil)
            : this()
        {
            Result = AuthLoginResult.Banned;
            BannedUntil = bannedUntil.ToString("yyyyMMddHHmmss");
        }

        public LoginEUAckMessage(AuthLoginResult result)
            : this()
        {
            Result = result;
        }

        public LoginEUAckMessage(AuthLoginResult result, ulong accountId, uint sessionId, string authsession,
            string newsession, string datetime)
            : this()
        {
            Result = result;
            AccountId = accountId;
            SessionId = (uint) accountId;
            SessionId2 = sessionId.ToString();
            AuthToken = authsession;
            NewToken = newsession;
            Datetime = datetime;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public uint SessionId { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string SessionId2 { get; set; }

        [BlubMember(4)]
        public AuthLoginResult Result { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string BannedUntil { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string mUnknow04 { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string AuthToken { get; set; }

        [BlubMember(9, typeof(StringSerializer))]
        public string NewToken { get; set; }

        [BlubMember(10, typeof(StringSerializer))]
        public string Datetime { get; set; }
    }

    [BlubContract]
    public class ServerListAckMessage : IAuthMessage
    {
        public ServerListAckMessage()
            : this(Array.Empty<ServerInfoDto>())
        {
        }

        public ServerListAckMessage(ServerInfoDto[] serverList)
        {
            ServerList = serverList;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ServerInfoDto[] ServerList { get; set; }
    }

    [BlubContract]
    public class OptionVersionCheckAckMessage : IAuthMessage
    {
        public OptionVersionCheckAckMessage()
        {
            Data = Array.Empty<byte>();
        }

        public OptionVersionCheckAckMessage(byte[] data)
        {
            Data = data;
        }

        [BlubMember(0, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }

    [BlubContract]
    public class GameDataAckMessage : IAuthMessage
    {

        public GameDataAckMessage()
        {
        }

        public GameDataAckMessage(uint type, byte[] data)
        {
            Type = type;
            Data = data;
        }

        [BlubMember(0)]
        public uint Type { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }
}
