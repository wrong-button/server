using System;

namespace ExitPath.Server.Multiplayer
{
    public record RoomGameState : RoomState<RoomGameState>
    {
        public override object ToJSON()
        {
            return new();
        }

        public override object? Diff(RoomGameState oldState)
        {
            return null;
        }
    }

    public class RoomGame : Room<RoomGameState>
    {
        public RoomGame(Realm realm, string name) : base(realm, Guid.NewGuid().ToString(), name, new RoomGameState())
        {
        }
    }
}
