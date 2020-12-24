using ExitPath.Server.Models;
using ExitPath.Server.Multiplayer.Messages;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class Player
    {
        public string ConnectionId { get; }
        public PlayerData Data { get; }

        private ImmutableDictionary<string, Player> players = ImmutableDictionary.Create<string, Player>();

        public Player(string connId, PlayerData data)
        {
            this.ConnectionId = connId;
            this.Data = data;
        }

        public async Task Tick(IRoom room)
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
                await room.Realm.SendMessage(this, new UpdatePlayers
                {
                    Joined = newPlayers,
                    Exited = removedPlayers,
                });
            }
        }
    }
}
