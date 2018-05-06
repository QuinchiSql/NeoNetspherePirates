using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NeoNetsphere.Network.Data.Auth;
using NeoNetsphere.Network.Message.Auth;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    internal class ServerManager : IEnumerable<ServerInfoDto>
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ServerManager));

        internal readonly ConcurrentDictionary<ushort, ServerEntry> ServerList =
            new ConcurrentDictionary<ushort, ServerEntry>();

        public IEnumerator<ServerInfoDto> GetEnumerator()
        {
            return ServerList.Values
                .Select(entry => entry.Game)
                .Concat(ServerList.Values.Select(entry => entry.Chat))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public byte Add(AuthServer.ServiceModel.ServerInfoDto serverInfo)
        {
            if (serverInfo.ApiKey == null || serverInfo.ApiKey != Config.Instance.API.ApiKey)
            {
                Logger.Information($"Refused server request: {serverInfo.Name}({serverInfo.Id}) - invalid apikey!");
                return 2;
            }

            var chat = new ServerInfoDto
            {
                IsEnabled = true,
                Id = serverInfo.Id,
                GroupId = serverInfo.Id,
                Type = ServerType.Chat,
                Name = serverInfo.Name,
                PlayerLimit = serverInfo.PlayerLimit,
                PlayerOnline = serverInfo.PlayerOnline,
                EndPoint = serverInfo.ChatEndPoint
            };
            var game = new ServerInfoDto
            {
                IsEnabled = true,
                Id = serverInfo.Id,
                GroupId = serverInfo.Id,
                Type = ServerType.Game,
                Name = serverInfo.Name,
                PlayerLimit = serverInfo.PlayerLimit,
                PlayerOnline = serverInfo.PlayerOnline,
                EndPoint = serverInfo.EndPoint
            };

            if (ServerList.TryAdd(serverInfo.Id, new ServerEntry(game, chat)))
            {
                Logger.Information($"Added server with valid apikey: {serverInfo.Name}({serverInfo.Id})");
                Network.AuthServer.Instance.Broadcast(new ServerListAckMessage(Network.AuthServer.Instance.ServerManager.ToArray()));
                return 0;
            }
            return 1;
        }

        public bool Update(AuthServer.ServiceModel.ServerInfoDto serverInfo)
        {
            if (!ServerList.TryGetValue(serverInfo.Id, out var entry))
                return false;

            entry.Game.PlayerLimit = serverInfo.PlayerLimit;
            entry.Game.PlayerOnline = serverInfo.PlayerOnline;

            entry.Chat.PlayerLimit = serverInfo.PlayerLimit;
            entry.Chat.PlayerOnline = serverInfo.PlayerOnline;

            entry.LastUpdate = DateTimeOffset.Now;

            return true;
        }

        public void Flush()
        {
            foreach (var pair in ServerList)
            {
                var diff = DateTimeOffset.Now - pair.Value.LastUpdate;
                if (diff >= Config.Instance.API.Timeout)
                    Remove(pair.Key);
            }
        }

        public bool Remove(ushort id)
        {
            if (ServerList.TryRemove(id, out var entry))
            {
                Logger.Information($"Removed server {entry.Game.Name}({entry.Game.GroupId})");
                Network.AuthServer.Instance.Broadcast(new ServerListAckMessage(Network.AuthServer.Instance.ServerManager.ToArray()));
                return true;
            }
            return false;
        }

        internal class ServerEntry
        {
            public ServerEntry(ServerInfoDto game, ServerInfoDto chat)
            {
                Game = game;
                Chat = chat;
                LastUpdate = DateTimeOffset.Now;
            }

            public ServerInfoDto Game { get; set; }
            public ServerInfoDto Chat { get; set; }
            public DateTimeOffset LastUpdate { get; set; }
        }
    }
}
