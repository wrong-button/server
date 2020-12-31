using ExitPath.Server.Multiplayer.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ExitPath.Server.Multiplayer
{
    public record RoomLobbyState : RoomState<RoomLobbyState>
    {
        public ImmutableDictionary<string, RemoteRoom> Rooms { get; init; } = ImmutableDictionary.Create<string, RemoteRoom>();

        public override object ToJSON()
        {
            return new
            {
                Rooms = this.Rooms.Values.OrderBy(r => r.Name).ToList()
            };
        }

        public override object? Diff(RoomLobbyState oldState)
        {
            var diff = new
            {
                Removed = new List<string>(),
                Updated = new List<RemoteRoom>(),
            };

            foreach (var (id, room) in this.Rooms)
            {
                if (!oldState.Rooms.TryGetValue(id, out var oldRoom) || !oldRoom.Equals(room))
                {
                    diff.Updated.Add(room);
                }
            }
            foreach (var id in oldState.Rooms.Keys)
            {
                if (!this.Rooms.ContainsKey(id))
                {
                    diff.Removed.Add(id);
                }
            }

            if (diff.Removed.Count == 0 && diff.Updated.Count == 0)
            {
                return null;
            }

            return diff;
        }
    }

    public class RoomLobby : Room<RoomLobbyState>
    {
        public RoomLobby(Realm realm) : base(realm, "lobby", "Party Room", new RoomLobbyState())
        {
        }

        public override void Tick()
        {
            var rooms = ImmutableDictionary.CreateBuilder<string, RemoteRoom>();
            foreach (var room in this.Realm.Rooms.OfType<RoomGame>())
            {
                rooms[room.Id] = new RemoteRoom(room);
            }
            this.State = this.State with { Rooms = rooms.ToImmutable() };

            base.Tick();
        }
    }
}
