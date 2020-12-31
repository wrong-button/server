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

    public enum GamePhase
    {
        Lobby,
        InGame,
    }

    public record RoomGameStateDiff
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GamePlayerData>? Updated { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? Removed { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Phase { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Timer { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NextLevel { get; set; }
    }

    public record RoomGameState : RoomState<RoomGameState>
    {
        public int NextId { get; init; } = 1;
        public ImmutableDictionary<string, GamePlayer> Players { get; init; } = ImmutableDictionary.Create<string, GamePlayer>();

        public GamePhase Phase { get; init; } = GamePhase.Lobby;
        public int Timer { get; init; } = 0;
        public int NextLevel { get; init; } = 0;

        public override object ToJSON()
        {
            return new
            {
                Players = Players.Values.Select(p => new GamePlayerData(p)).OrderBy(p => p.Id).ToList(),
                Phase = this.Phase.ToString(),
                Timer = (int)Math.Ceiling((double)this.Timer / Realm.TPS),
                NextLevel = this.NextLevel,
            };
        }

        public override object? Diff(RoomGameState oldState)
        {
            var diff = new RoomGameStateDiff
            {
                Updated = new(),
                Removed = new(),
            };
            var needDiff = false;

            foreach (var (id, player) in this.Players)
            {
                var playerData = new GamePlayerData(player);
                if (!oldState.Players.TryGetValue(id, out var oldPlayer) ||
                    !new GamePlayerData(oldPlayer).Equals(playerData))
                {
                    diff.Updated.Add(playerData);
                    needDiff = true;
                }
            }
            foreach (var (id, player) in oldState.Players)
            {
                if (!this.Players.ContainsKey(id))
                {
                    diff.Removed.Add(player.LocalId);
                    needDiff = true;
                }
            }
            if (oldState.Phase != this.Phase)
            {
                diff.Phase = this.Phase.ToString();
                needDiff = true;
            }
            if (Math.Ceiling((double)oldState.Timer / Realm.TPS) != Math.Ceiling((double)this.Timer / Realm.TPS))
            {
                diff.Timer = (int)Math.Ceiling((double)this.Timer / Realm.TPS);
                needDiff = true;
            }
            if (oldState.NextLevel != this.NextLevel)
            {
                diff.NextLevel = this.NextLevel;
                needDiff = true;
            }

            if (diff.Removed.Count == 0)
            {
                diff.Removed = null;
            }
            if (diff.Updated.Count == 0)
            {
                diff.Updated = null;
            }
            if (!needDiff)
            {
                return null;
            }

            return diff;
        }
    }

    public class RoomGame : Room<RoomGameState>
    {
        private readonly Random rand = new Random();

        public RoomGame(Realm realm, string name) : base(realm, Guid.NewGuid().ToString(), name, new RoomGameState())
        {
            this.StartPhase(GamePhase.Lobby);
        }

        private void StartPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Lobby:
                    this.State = this.State with
                    {
                        Phase = GamePhase.Lobby,
                        Timer = this.Realm.Config.GameCountdown * Realm.TPS,
                        NextLevel = 100 + rand.Next(20),
                    };
                    break;

                case GamePhase.InGame:
                    this.State = this.State with
                    {
                        Phase = GamePhase.InGame,
                        Timer = 0,
                    };
                    break;
            }
        }

        public override void AddPlayer(Player player)
        {
            if (this.State.Phase == GamePhase.InGame)
            {
                throw new Exception("Game is in progress");
            }

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

        public override void Tick()
        {
            switch (this.State.Phase)
            {
                case GamePhase.Lobby:
                    var timer = this.State.Timer - 1;
                    if (this.Players.Count <= 1)
                    {
                        timer = this.Realm.Config.GameCountdown * Realm.TPS;
                    }
                    if (timer <= 0)
                    {
                        this.StartPhase(GamePhase.InGame);
                        goto case GamePhase.InGame;
                    }
                    this.State = this.State with { Timer = timer };
                    break;

                case GamePhase.InGame:
                    break;
            }

            base.Tick();
        }
    }
}
