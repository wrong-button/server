using ExitPath.Server.Models;
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

        private readonly ILogger<MultiplayerHub> logger;
        private readonly Realm realm;

        private Player Player
        {
            get => (Player)this.Context.Items[PlayerKey]!;
            set => this.Context.Items[PlayerKey] = value;
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
            await this.realm.AddPlayer(player, "lobby");
            this.Player = player;

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
            this.realm.AddRoom(room);
            await this.realm.AddPlayer(this.Player, room.Id);
            return new();
        }

        public async Task<object> JoinRoom(string? roomId)
        {
            roomId ??= "";
            try
            {
                await this.realm.AddPlayer(this.Player, roomId);
            }
            catch (Exception e)
            {
                return new { Error = e.Message };
            }
            return new();
        }
    }
}
