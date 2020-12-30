using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class Realm
    {
        private readonly struct MessageSend
        {
            public string Target { get; init; }
            public string MessageName { get; init; }
            public object Message { get; init; }
        }

        private readonly ILogger<Realm> logger;
        private readonly IHubContext<MultiplayerHub> hub;
        private readonly AsyncLock realmLock = new();

        private readonly ConcurrentDictionary<string, IRoom> rooms = new();
        private readonly ConcurrentDictionary<Player, IRoom> players = new();
        public int RoomListVersion { get; set; }

        private readonly Channel<MessageSend> msgChannel = Channel.CreateUnbounded<MessageSend>();

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

        public void SendMessage(Player target, object msg)
        {
            this.msgChannel.Writer.TryWrite(new MessageSend
            {
                Target = target.ConnectionId,
                MessageName = msg.GetType().Name,
                Message = msg
            });
        }

        public async Task Tick()
        {
            using var _lock = await this.realmLock.LockAsync();

            foreach (var room in this.rooms.Values)
            {
                room.Tick();
            }
        }

        public async Task PumpMessages(CancellationToken token)
        {
            await foreach (var msg in this.msgChannel.Reader.ReadAllAsync(token))
            {
                await this.hub.Clients.Client(msg.Target).SendAsync(msg.MessageName, msg.Message, token);
            }
        }
    }
}
