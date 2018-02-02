using System;
using System.Threading.Tasks;
using AuthServer.ServiceModel;
using BlubLib.DotNetty.SimpleRmi;

namespace NeoNetsphere.API
{
    internal class ServerlistService : RmiService, IServerlistService
    {
        public async Task<RegisterResult> Register(ServerInfoDto serverInfo)
        {
            UpdateLastActivity();
            var result = (RegisterResult) Network.AuthServer.Instance.ServerManager.Add(serverInfo);

            if (result == RegisterResult.OK)
            {
                var state = CurrentContext.Channel.GetAttribute(ChannelAttributes.State).Get();
                state.ServerId = serverInfo.Id;
            }

            return result;
        }

        public async Task<bool> Update(ServerInfoDto serverInfo)
        {
            UpdateLastActivity();
            return Network.AuthServer.Instance.ServerManager.Update(serverInfo);
        }

        public async Task<bool> Remove(byte id)
        {
            return Network.AuthServer.Instance.ServerManager.Remove(id);
        }

        private void UpdateLastActivity()
        {
            var state = CurrentContext.Channel.GetAttribute(ChannelAttributes.State).Get();
            state.LastActivity = DateTimeOffset.Now;
        }
    }
}
