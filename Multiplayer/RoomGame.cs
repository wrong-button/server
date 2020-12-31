using ExitPath.Server.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace ExitPath.Server.Multiplayer
{
    public record GamePlayer(int LocalId, Player Player);

    public record GamePlayerData
    {
        public string Id { get; init; }
        public int LocalId { get; init; }
        public PlayerData Data { get; init; }

        public GamePlayerData(GamePlayer player)
        {
            this.Id = player.Player.ConnectionId;
            this.LocalId = player.LocalId;
            this.Data = player.Player.Data;
        }
    }

    public record RoomGameStateDiff
    {
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GamePlayerData>? Updated { get; set; }

        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? Removed { get; set; }
    }

    public record RoomGameState : RoomState<RoomGameState>
    {
        public int NextId { get; init; } = 1;
        public ImmutableDictionary<string, GamePlayer> Players { get; init; } = ImmutableDictionary.Create<string, GamePlayer>();

        public override object ToJSON()
        {
            return new
            {
                Players = Players.Values.Select(p => new GamePlayerData(p)).OrderBy(p => p.Id).ToList()
            };
        }

        public override object? Diff(RoomGameState oldState)
        {
            var diff = new RoomGameStateDiff
            {
                Updated = new(),
                Removed = new(),
            };

            foreach (var (id, player) in this.Players)
            {
                var playerData = new GamePlayerData(player);
                if (!oldState.Players.TryGetValue(id, out var oldPlayer) ||
                    !new GamePlayerData(oldPlayer).Equals(playerData))
                {
                    diff.Updated.Add(playerData);
                }
            }
            foreach (var (id, player) in oldState.Players)
            {
                if (!this.Players.ContainsKey(id))
                {
                    diff.Removed.Add(player.LocalId);
                }
            }

            if (diff.Removed.Count == 0)
            {
                diff.Removed = null;
            }
            if (diff.Updated.Count == 0)
            {
                diff.Updated = null;
            }
            if (diff.Removed == null && diff.Updated == null)
            {
                return null;
            }

            return diff;
        }
    }

    public class RoomGame : Room<RoomGameState>
    {
        public RoomGame(Realm realm, string name) : base(realm, Guid.NewGuid().ToString(), name, new RoomGameState())
        {
        }

        public override void AddPlayer(Player player)
        {
            base.AddPlayer(player);

            this.State = this.State with
            {
                NextId = this.State.NextId + 1,
                Players = this.State.Players.Add(player.ConnectionId, new(this.State.NextId, player)),
            };
        }

        public override void RemovePlayer(Player player)
        {
            base.RemovePlayer(player);

            this.State = this.State with
            {
                Players = this.State.Players.Remove(player.ConnectionId)
            };
        }
    }
}
