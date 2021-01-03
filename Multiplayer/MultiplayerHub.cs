using ExitPath.Server.Models;
using ExitPath.Server.Multiplayer.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    [Authorize(AuthenticationSchemes = "Multiplayer")]
    public class MultiplayerHub : Hub
    {
        private static readonly object PlayerKey = new();
        private static readonly object RoomKey = new();
        private static readonly object LastMessageKey = new();

        private readonly ILogger<MultiplayerHub> logger;
        private readonly Realm realm;

        private Player Player
        {
            get => (Player)this.Context.Items[PlayerKey]!;
            set => this.Context.Items[PlayerKey] = value;
        }

        private IRoom Room
        {
            get => (IRoom)this.Context.Items[RoomKey]!;
            set => this.Context.Items[RoomKey] = value;
        }

        private DateTime LastMessage
        {
            get => (DateTime)this.Context.Items[LastMessageKey]!;
            set => this.Context.Items[LastMessageKey] = value;
        }

        public MultiplayerHub(ILogger<MultiplayerHub> logger, Realm realm)
        {
            this.logger = logger;
            this.realm = realm;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var playerDataJSON = Context.User.FindFirstValue("player") ?? "";
            var playerData = JsonSerializer.Deserialize<PlayerData>(playerDataJSON);
            if (playerData == null)
            {
                throw new Exception("Player data is null");
            }

            var player = new Player(Context.ConnectionId, playerData);
            var room = await this.realm.AddPlayer(player, "lobby");
            this.Player = player;
            this.Room = room;
            this.LastMessage = DateTime.MinValue;

            logger.LogInformation("Player '{Name}' ({ID}) connected", Context.UserIdentifier, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await this.realm.RemovePlayer(this.Player);

            logger.LogInformation("Player '{Name}' ({ID}) disconnected", Context.UserIdentifier, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<object> CreateGameRoom(string? roomName)
        {
            roomName ??= "";
            if (roomName.Length == 0)
            {
                roomName = NameUtils.GenerateRoomName();
            }
            else if (roomName.Length > 50)
            {
                return new { Error = "Room name is too long" };
            }

            var room = new RoomGame(this.realm, roomName);
            await this.realm.CreateRoom(this.Player, room);
            this.Room = room;
            return new();
        }

        public async Task<object> JoinRoom(string? roomId)
        {
            roomId ??= "";
            try
            {
                var room = await this.realm.AddPlayer(this.Player, roomId);
                this.Room = room;
            }
            catch (Exception e)
            {
                return new { Error = e.Message };
            }
            return new();
        }

        public void ReportPosition(int v, float x, float y, int fr, int sx, int t)
        {
            if (this.Room is RoomGame room)
            {
                var player = this.Player;
                this.realm.EnqueueAction(() => room.ReportPosition(player, new GamePlayerPosition(v, x, y, fr, sx, t)));
            }
        }

        public void ReportCheckpoint(int id)
        {
            if (this.Room is RoomGame room)
            {
                var player = this.Player;
                this.realm.EnqueueAction(() => room.ReportCheckpoint(player, id));
            }
        }

        public void GiveKudo(string targetId)
        {
            if (this.Room is RoomGame room)
            {
                var player = this.Player;
                this.realm.EnqueueAction(() => room.GiveKudo(player, targetId));
            }
        }

        public void SendMessage(string text)
        {
            var room = this.Room;
            var player = this.Player;
            if (text.StartsWith("/"))
            {
                var parts = text[1..].Split((string?)null, StringSplitOptions.RemoveEmptyEntries);
                this.realm.EnqueueAction(() =>
                {
                    if (!room.ProcessCommand(player, parts[0].ToLowerInvariant(), parts[1..]))
                    {
                        room.SendMessage(player, Message.Error($"Unrecognized command '/{parts[0]}'"));
                    }
                });
                return;
            }

            var now = DateTime.UtcNow;
            if (now.Subtract(this.LastMessage).TotalSeconds < 0.3)
            {
                return;
            }
            this.LastMessage = now;

            this.realm.EnqueueAction(() =>
            {
                room.BroadcastMessage(Message.Player(player, text));
            });
        }

        public void SetNextLevel(string level)
        {
            if (this.Room is RoomGame room)
            {
                var player = this.Player;
                this.realm.EnqueueAction(() => room.SetNextLevel(player, level));
            }
        }
    }
}
