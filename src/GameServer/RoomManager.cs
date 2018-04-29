using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Collections.Concurrent;
using BlubLib.Threading.Tasks;
using NeoNetsphere;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;
using Netsphere.Game;
using ProudNetSrc;

// ReSharper disable once CheckNamespace
namespace Netsphere
{
    internal class RoomManager : IReadOnlyCollection<Room>
    {
        public readonly ConcurrentDictionary<uint, Room> _rooms = new ConcurrentDictionary<uint, Room>();
        private readonly AsyncLock _sync = new AsyncLock();

        public RoomManager(Channel channel)
        {
            Channel = channel;
            GameRuleFactory = new GameRuleFactory();
        }

        public Channel Channel { get; }
        public GameRuleFactory GameRuleFactory { get; }

        public void Update(TimeSpan delta)
        {
            foreach (var room in _rooms.Values)
                room.Update(delta);
        }

        public Room Get(uint id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        public Room Create_2(RoomCreationOptions options, P2PGroup p2pGroup)
        {
            try
            {
                using (_sync.Lock())
                {
                    uint id = 1;
                    while (true)
                    {
                        if (!_rooms.ContainsKey(id))
                            break;
                        id++;
                    }

                    var room = new Room(this, id, options, p2pGroup, options.Creator);
                    //_rooms.TryAdd(id, room);
                    var roomDto = room.GetRoomInfo();
                    roomDto.Password =
                        !string.IsNullOrWhiteSpace(room.Options.Password) ||
                        !string.IsNullOrEmpty(room.Options.Password)
                            ? "nice try :)"
                            : "";
                    Channel.Broadcast(new RoomDeployAck2Message(roomDto));

                    return room;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public Room Create(RoomCreationOptions options, P2PGroup p2pGroup)
        {
            using (_sync.Lock())
            {
                uint id = 1;
                while (true)
                {
                    if (!_rooms.ContainsKey(id))
                        break;
                    id++;
                }

                var room = new Room(this, id, options, p2pGroup, options.Creator);
                var roomDto = room.GetRoomInfo();
                roomDto.Password =
                    !string.IsNullOrWhiteSpace(room.Options.Password) ||
                    !string.IsNullOrEmpty(room.Options.Password)
                        ? "nice try :)"
                        : "";
                Channel.Broadcast(new RoomDeployAckMessage(roomDto));

                return room;
            }
        }

        public void Remove(Room room)
        {
            if (room.TeamManager.Players.Any())
                throw new RoomException("Players are still in this room");
            
            _rooms.Remove(room.Id);
            Channel.Broadcast(new RoomDisposeAckMessage(room.Id));
        }

        #region Events

        #endregion

        #region IReadOnlyCollection

        public int Count => _rooms.Count;

        public Room this[uint id] => Get(id);

        public IEnumerator<Room> GetEnumerator()
        {
            return _rooms.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
