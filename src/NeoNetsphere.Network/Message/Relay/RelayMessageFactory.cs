using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization;

namespace NeoNetsphere.Network.Message.Relay
{
    public interface IRelayMessage
    {
    }

    public class RelayMessageFactory : MessageFactory<RelayOpCode, IRelayMessage>
    {
        static RelayMessageFactory()
        {
            Serializer.AddCompiler(new PeerIdSerializer());
        }

        public RelayMessageFactory()
        {
            // S2C
            Register<SEnterLoginPlayerMessage>(RelayOpCode.SEnterLoginPlayer);
            Register<SNotifyLoginResultMessage>(RelayOpCode.SNotifyLoginResult);

            // C2S
            Register<CRequestLoginMessage>(RelayOpCode.CRequestLogin);
            Register<CNotifyP2PLogMessage>(RelayOpCode.CNotifyP2PLog);
        }
    }
}
