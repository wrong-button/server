using ExitPath.Server.Multiplayer.Models;
using System;
using System.Collections.Generic;

namespace ExitPath.Server.Multiplayer.Messages
{
    public record JoinRoom
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<RemotePlayer> Players { get; init; } = Array.Empty<RemotePlayer>();
    }
}
