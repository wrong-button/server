using ExitPath.Server.Multiplayer.Models;
using System;
using System.Collections.Generic;

namespace ExitPath.Server.Multiplayer.Messages
{
    public record UpdatePlayers
    {
        public IReadOnlyList<RemotePlayer> Joined { get; init; } = Array.Empty<RemotePlayer>();
        public IReadOnlyList<string> Exited { get; init; } = Array.Empty<string>();
    }
}
