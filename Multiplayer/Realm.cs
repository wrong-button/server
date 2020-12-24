using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class Realm
    {
        private readonly ILogger<Realm> logger;
        private readonly IHubContext<MultiplayerHub> hub;
        private readonly AsyncLock realmLock = new();

        private readonly ConcurrentDictionary<string, IRoom> rooms = new();
        private readonly ConcurrentDictionary<Player, IRoom> players = new();
        public int RoomListVersion { get; set; }

        public Realm(ILogger<Realm> logger, IHubContext<MultiplayerHub> hub)
        {
            this.logger = logger;
            this.hub = hub;
            this.AddRoom(new RoomLobby(this));
        }

        private void AddRoom(IRoom room)
        {
            this.RoomListVersion++;
            rooms[room.Id] = room;
            logger.LogInformation("Room {Id}({Name}) created", room.Id, room.Name);
        }

        public async Task AddPlayer(Player player, string roomId)
        {
            using var _lock = await this.realmLock.LockAsync();

            if (!this.rooms.TryGetValue(roomId, out var room))
            {
                throw new Exception("Room not found");
            }
            room.AddPlayer(player);
            this.players[player] = room;
        }

        public async Task RemovePlayer(Player player)
        {
            using var _lock = await this.realmLock.LockAsync();

            if (!this.players.TryRemove(player, out var room))
            {
                return;
            }
            room.RemovePlayer(player);
        }

        public async Task SendMessage(Player target, object msg)
        {
            await this.hub.Clients.Client(target.ConnectionId).SendAsync(msg.GetType().Name, msg);
        }

        public async Task Tick()
        {
            using var _lock = await this.realmLock.LockAsync();

            foreach (var room in this.rooms.Values)
            {
                await room.Tick();
            }
        }
    }
}
