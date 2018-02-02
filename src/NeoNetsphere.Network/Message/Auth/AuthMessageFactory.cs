using ProudNetSrc.Serialization;

namespace NeoNetsphere.Network.Message.Auth
{
    public interface IAuthMessage
    {
    }

    public class AuthMessageFactory : MessageFactory<AuthOpCode, IAuthMessage>
    {
        public AuthMessageFactory()
        {
            // S2C
            Register<LoginEUAckMessage>(AuthOpCode.LoginEUAck);
            Register<ServerListAckMessage>(AuthOpCode.ServerListAck);
            Register<OptionVersionCheckAckMessage>(AuthOpCode.OptionVersionCheckAck);
            Register<GameDataAckMessage>(AuthOpCode.GameData_XBN_Ack);

            // C2S
            Register<LoginEUReqMessage>(AuthOpCode.LoginEUReq);
            Register<ServerListReqMessage>(AuthOpCode.ServerListReq);
            Register<OptionVersionCheckReqMessage>(AuthOpCode.OptionVersionCheckReq);
            Register<GameDataReqMessage>(AuthOpCode.GameData_XBN_Req);
        }
    }
}
