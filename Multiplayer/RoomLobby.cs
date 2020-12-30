using ExitPath.Server.Multiplayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExitPath.Server.Multiplayer
{
    public record RoomLobbyState
    {
        public IReadOnlyList<RemoteRoom> Rooms { get; init; } = Array.Empty<RemoteRoom>();
    }

    public class RoomLobby : Room<RoomLobbyState>
    {
        public RoomLobby(Realm realm) : base(realm, "lobby", "Party Room", new RoomLobbyState())
        {
        }

        public override void Tick()
        {
            var rooms = new List<RemoteRoom>();
            foreach (var room in this.Realm.Rooms.OfType<RoomGame>())
            {
                rooms.Add(new RemoteRoom(room));
            }
            rooms.Sort((a, b) => a.Name.CompareTo(b.Name));
            if (!rooms.SequenceEqual(this.State.Rooms))
            {
                this.State = this.State with { Rooms = rooms };
            }

            base.Tick();
        }
    }
}
