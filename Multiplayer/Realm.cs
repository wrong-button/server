using ExitPath.Server.Config;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class Realm
    {
        public const int TPS = 20;

        private readonly struct MessageSend
        {
            public string Target { get; init; }
            public string MessageName { get; init; }
            public object Message { get; init; }
        }

        private readonly ILogger<Realm> logger;
        private readonly IHubContext<MultiplayerHub> hub;
        private readonly AsyncLock realmLock = new();

        private readonly Dictionary<string, IRoom> rooms = new();
        private readonly Dictionary<Player, IRoom> players = new();

        public IEnumerable<IRoom> Rooms => this.rooms.Values;

        public RealmConfig Config { get; }

        private readonly Channel<MessageSend> msgChannel = Channel.CreateUnbounded<MessageSend>();

        public Realm(ILogger<Realm> logger, IHubContext<MultiplayerHub> hub, IOptions<RealmConfig> config)
        {
            this.logger = logger;
            this.hub = hub;
            this.Config = config.Value;
            this.AddRoom(new RoomLobby(this));
        }

        private void AddRoom(IRoom room)
        {
            rooms[room.Id] = room;
            logger.LogInformation("Room '{Name}' ({Id}) created", room.Name, room.Id);
        }

        private void DeleteRoom(IRoom room)
        {
            if (rooms.Remove(room.Id))
            {
                logger.LogInformation("Room '{Name}' ({Id}) deleted", room.Name, room.Id);
            }
        }

        private void AddPlayer(Player player, IRoom room)
        {
            room.AddPlayer(player);

            if (this.players.Remove(player, out var oldRoom))
            {
                oldRoom.RemovePlayer(player);
            }
            this.players[player] = room;

            logger.LogInformation("Player '{Name}' joined room '{Room}'", player.Data.DisplayName, room.Name);
        }

        public async Task AddPlayer(Player player, string roomId)
        {
            using var _lock = await this.realmLock.LockAsync();

            if (!this.rooms.TryGetValue(roomId, out var room))
            {
                throw new Exception("Room not found");
            }
            this.AddPlayer(player, room);
        }

        public async Task RemovePlayer(Player player)
        {
            using var _lock = await this.realmLock.LockAsync();

            if (!this.players.Remove(player, out var room))
            {
                return;
            }
            room.RemovePlayer(player);

            logger.LogInformation("Player '{Name}' left", player.Data.DisplayName);
        }

        public async Task CreateRoom(Player player, IRoom room)
        {
            using var _lock = await this.realmLock.LockAsync();

            this.AddRoom(room);
            this.AddPlayer(player, room);
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

            foreach (var room in this.rooms.Values.ToList())
            {
                if (room is RoomGame && room.Players.Count == 0)
                {
                    this.DeleteRoom(room);
                }
            }
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
