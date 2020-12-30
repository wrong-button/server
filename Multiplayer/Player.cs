using ExitPath.Server.Models;
using ExitPath.Server.Multiplayer.Messages;
using ExitPath.Server.Multiplayer.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ExitPath.Server.Multiplayer
{
    public class Player
    {
        public string ConnectionId { get; }
        public PlayerData Data { get; }

        private ImmutableDictionary<string, Player> players = ImmutableDictionary.Create<string, Player>();
        private object? state = null;

        public Player(string connId, PlayerData data)
        {
            this.ConnectionId = connId;
            this.Data = data;
        }

        public void OnJoinRoom(IRoom room)
        {
            this.players = room.Players;
            this.state = room.State;
            room.Realm.SendMessage(this, new JoinRoom
            {
                Id = room.Id,
                Name = room.Name,
                Players = room.Players.Values.Select(p => new RemotePlayer(p)).ToList(),
                State = room.State,
            });
        }

        public void Tick(IRoom room)
        {
            if (this.players != room.Players)
            {
                var newPlayers = new List<RemotePlayer>();
                var removedPlayers = new List<string>();
                foreach (var (id, player) in room.Players)
                {
                    if (!this.players.ContainsKey(id))
                    {
                        newPlayers.Add(new RemotePlayer(player));
                    }
                }
                foreach (var id in this.players.Keys)
                {
                    if (!room.Players.ContainsKey(id))
                    {
                        removedPlayers.Add(id);
                    }
                }

                this.players = room.Players;
                room.Realm.SendMessage(this, new UpdatePlayers
                {
                    Joined = newPlayers,
                    Exited = removedPlayers,
                });
            }
            if (this.state != room.State)
            {
                this.state = room.State;
                room.Realm.SendMessage(this, new UpdateState
                {
                    NewState = room.State,
                });
            }
        }
    }
}
