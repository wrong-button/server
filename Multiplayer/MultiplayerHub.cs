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

        public MultiplayerHub(ILogger<MultiplayerHub> logger, Realm realm)
        {
            this.logger = logger;
            this.realm = realm;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var playerDataJSON = Context.User.FindFirstValue("player") ?? "";
            var roomId = Context.User.FindFirstValue("roomId") ?? "lobby";
            var playerData = JsonSerializer.Deserialize<PlayerData>(playerDataJSON);
            if (playerData == null)
            {
                throw new Exception("Player data is null");
            }

            var player = new Player(Context.ConnectionId, playerData);
            await this.realm.AddPlayer(player, roomId);
            this.Context.Items[PlayerKey] = player;

            logger.LogInformation("Player {Name} ({ID}) connected to {Room}", Context.UserIdentifier, Context.ConnectionId, roomId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (this.Context.Items[PlayerKey] is Player player)
            {
                await this.realm.RemovePlayer(player);
            }

            logger.LogInformation("Player {Name} ({ID}) disconnected", Context.UserIdentifier, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
