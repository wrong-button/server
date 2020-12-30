using System;

namespace ExitPath.Server.Multiplayer
{
    public record RoomGameState
    {
    }

    public class RoomGame : Room<RoomGameState>
    {
        public RoomGame(Realm realm, string name) : base(realm, Guid.NewGuid().ToString(), name, new RoomGameState())
        {
        }
    }
}
