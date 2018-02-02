using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Relay
{
    [BlubContract]
    public class SEnterLoginPlayerMessage : IRelayMessage
    {
        public SEnterLoginPlayerMessage()
        {
            Nickname = "";
        }

        public SEnterLoginPlayerMessage(uint hostId, ulong accountId, string nickname)
        {
            HostId = hostId;
            AccountId = accountId;
            Nickname = nickname;
        }

        [BlubMember(0)]
        public uint HostId { get; set; } // Not sure, but proudnet thing for sure

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Nickname { get; set; }
    }

    [BlubContract]
    public class SNotifyLoginResultMessage : IRelayMessage
    {
        public SNotifyLoginResultMessage()
        {
        }

        public SNotifyLoginResultMessage(int result)
        {
            Result = result;
        }

        [BlubMember(0)]
        public int Result { get; set; }
    }
}
