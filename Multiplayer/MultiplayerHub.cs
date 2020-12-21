using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    [Authorize(AuthenticationSchemes = "Multiplayer")]
    public class MultiplayerHub : Hub<IMultiplayerClient>
    {
        private readonly ILogger<MultiplayerHub> logger;

        public MultiplayerHub(ILogger<MultiplayerHub> logger)
        {
            this.logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation("Player {Name} ({ID}) connected", Context.UserIdentifier, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogInformation("Player {Name} ({ID}) disconnected", Context.UserIdentifier, Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
