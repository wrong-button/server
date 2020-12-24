using System;
using System.Collections.Generic;

namespace ExitPath.Server.Multiplayer.Messages
{
    public record RemotePlayer
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public int Color { get; init; }

        public RemotePlayer(Player player)
        {
            this.Id = player.ConnectionId;
            this.Name = player.Data.DisplayName;
            this.Color = player.Data.PrimaryColor;
        }
    }

    public record UpdatePlayers
    {
        public IReadOnlyList<RemotePlayer> Joined { get; init; } = Array.Empty<RemotePlayer>();
        public IReadOnlyList<string> Exited { get; init; } = Array.Empty<string>();
    }
}
