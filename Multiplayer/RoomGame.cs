using ExitPath.Server.Models;
using ExitPath.Server.Multiplayer.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace ExitPath.Server.Multiplayer
{
    public record GamePlayerPosition(
        int Version,
        float X,
        float Y,
        int Frame,
        int ScaleX,
        int CompletionTime)
    {
        public object[] ToJSON(int id)
        {
            return new object[] { id, this.Version, this.X, this.Y, this.Frame, this.ScaleX, this.CompletionTime };
        }
    }

    public record GamePlayerCheckpoints(ImmutableHashSet<int> Ids)
    {
        public object[] ToJSON(int id)
        {
            return new int[] { id }.Concat(Ids).Cast<object>().ToArray();
        }
    }

    public record GamePlayerReward(
        int Id,
        int Placing,
        int MatchXP,
        int AvailableKudos,
        int ReceivedKudos
    );

    public record GamePlayerData(string Id, int LocalId, PlayerData Data);

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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NextLevelCode { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NextLevelName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<object[]>? Positions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<object[]>? Checkpoints { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GamePlayerReward>? Rewards { get; set; }
    }

    public record RoomGameState : RoomState<RoomGameState>
    {
        public int NextId { get; init; } = 1;
        public ImmutableDictionary<string, GamePlayerData> Players { get; init; } = ImmutableDictionary.Create<string, GamePlayerData>();

        public GamePhase Phase { get; init; } = GamePhase.Lobby;
        public int Timer { get; init; } = 0;
        public int NextLevel { get; init; } = 0;
        public string NextLevelCode { get; init; } = "";
        public string NextLevelName { get; init; } = "";
        public bool IsLevelFinished { get; init; } = false;

        public ImmutableDictionary<int, GamePlayerPosition> Positions = ImmutableDictionary.Create<int, GamePlayerPosition>();
        public ImmutableDictionary<int, GamePlayerCheckpoints> Checkpoints = ImmutableDictionary.Create<int, GamePlayerCheckpoints>();
        public ImmutableDictionary<int, GamePlayerReward> Rewards = ImmutableDictionary.Create<int, GamePlayerReward>();

        public override object ToJSON()
        {
            return new
            {
                Players = Players.Values.OrderBy(p => p.LocalId).ToList(),
                Phase = this.Phase.ToString(),
                Timer = (int)Math.Ceiling((double)this.Timer / Realm.TPS),
                NextLevel = this.NextLevel,
                NextLevelCode = this.NextLevelCode,
                NextLevelName = this.NextLevelName,
                Positions = Positions.Select((p) => p.Value.ToJSON(p.Key)).ToList(),
                Checkpoints = Checkpoints.Select((p) => p.Value.ToJSON(p.Key)).ToList(),
                Rewards = Rewards.Values.ToList()
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
                if (!oldState.Players.TryGetValue(id, out var oldPlayer) || !oldPlayer.Equals(player))
                {
                    diff.Updated.Add(player);
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
            if (oldState.NextLevelCode != this.NextLevelCode)
            {
                diff.NextLevelCode = this.NextLevelCode;
                needDiff = true;
            }
            if (oldState.NextLevelName != this.NextLevelName)
            {
                diff.NextLevelName = this.NextLevelName;
                needDiff = true;
            }

            if (this.Positions.Count == 0 && oldState.Positions.Count != 0)
            {
                diff.Positions = new();
                needDiff = true;
            }
            else if (this.Positions.Count != 0)
            {
                foreach (var (id, pos) in this.Positions)
                {
                    if (!oldState.Positions.TryGetValue(id, out var oldPos) || !oldPos.Equals(pos))
                    {
                        if (diff.Positions == null)
                        {
                            diff.Positions = new();
                        }
                        diff.Positions.Add(pos.ToJSON(id));
                        needDiff = true;
                    }
                }
            }

            if (this.Checkpoints.Count == 0 && oldState.Checkpoints.Count != 0)
            {
                diff.Checkpoints = new();
                needDiff = true;
            }
            else if (this.Checkpoints.Count != 0)
            {
                foreach (var (id, cp) in this.Checkpoints)
                {
                    if (!oldState.Checkpoints.TryGetValue(id, out var oldCP) || !oldCP.Ids.SequenceEqual(cp.Ids))
                    {
                        if (diff.Checkpoints == null)
                        {
                            diff.Checkpoints = new();
                        }
                        diff.Checkpoints.Add(cp.ToJSON(id));
                        needDiff = true;
                    }
                }
            }

            if (this.Rewards.Count == 0 && oldState.Rewards.Count != 0)
            {
                diff.Rewards = new();
                needDiff = true;
            }
            else if (this.Rewards.Count != 0)
            {
                foreach (var (id, reward) in this.Rewards)
                {
                    if (!oldState.Rewards.TryGetValue(id, out var oldReward) || !oldReward.Equals(reward))
                    {
                        if (diff.Rewards == null)
                        {
                            diff.Rewards = new();
                        }
                        diff.Rewards.Add(reward);
                        needDiff = true;
                    }
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
                    var nextLevel = 100 + rand.Next(20);
                    this.State = this.State with
                    {
                        Phase = GamePhase.Lobby,
                        Timer = this.Realm.Config.GameCountdown * Realm.TPS,
                        NextLevel = nextLevel,
                        NextLevelCode = "",
                        NextLevelName = GameData.GameLevelName(nextLevel) ?? "Unknown",
                        Checkpoints = ImmutableDictionary<int, GamePlayerCheckpoints>.Empty,
                    };
                    break;

                case GamePhase.InGame:
                    this.State = this.State with
                    {
                        Phase = GamePhase.InGame,
                        Timer = this.Realm.Config.FinishCountdown * Realm.TPS,
                        Positions = ImmutableDictionary<int, GamePlayerPosition>.Empty,
                        Checkpoints = ImmutableDictionary<int, GamePlayerCheckpoints>.Empty,
                        Rewards = ImmutableDictionary<int, GamePlayerReward>.Empty,
                        IsLevelFinished = false,
                    };
                    break;
            }
        }

        public override void AddPlayer(Player player)
        {
            if (!player.IsSpectator)
            {
                if (this.State.Phase == GamePhase.InGame)
                {
                    throw new Exception("Game is in progress");
                }

                this.State = this.State with
                {
                    NextId = this.State.NextId + 1,
                    Players = this.State.Players.Add(
                        player.ConnectionId,
                        new(player.ConnectionId, this.State.NextId, player.Data with { })
                    ),
                };
            }

            base.AddPlayer(player);
        }

        public override void RemovePlayer(Player player)
        {
            this.State = this.State with
            {
                Players = this.State.Players.Remove(player.ConnectionId)
            };

            base.RemovePlayer(player);
        }

        public override void Tick()
        {
            switch (this.State.Phase)
            {
                case GamePhase.Lobby:
                    {
                        var timer = this.State.Timer - 1;
                        if (this.State.Players.Count <= 1)
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
                    }
                case GamePhase.InGame:
                    {
                        var numFinished = 0;
                        foreach (var p in this.State.Players.Values)
                        {
                            if (this.State.Positions.TryGetValue(p.LocalId, out var pos) && pos.CompletionTime > 0)
                            {
                                numFinished++;
                            }
                        }

                        var timer = this.State.Timer;
                        if (numFinished == this.State.Players.Count && timer > 3)
                        {
                            timer = 3;
                        }
                        if (this.State.IsLevelFinished)
                        {
                            timer--;
                        }

                        if (timer <= 0)
                        {
                            this.StartPhase(GamePhase.Lobby);
                            this.FinalizeMatchRewards();
                            goto case GamePhase.Lobby;
                        }
                        else if (timer != this.State.Timer)
                        {
                            this.State = this.State with { Timer = timer };
                        }
                        break;
                    }
            }

            foreach (var playerData in this.State.Players.Values)
            {
                var player = this.Players[playerData.Id];
                if (playerData.Data != player.Data)
                {
                    this.State = this.State with
                    {
                        Players = this.State.Players.SetItem(playerData.Id, playerData with { Data = player.Data })
                    };
                }
            }

            base.Tick();
        }

        private void FinalizeMatchRewards()
        {
            var rankedPlayers = this.State.Players.Values
                .Select(p => new
                {
                    Player = p,
                    Time = this.State.Positions.GetValueOrDefault(p.LocalId)?.CompletionTime ?? 0
                })
                .OrderBy(p => p.Time == 0 ? int.MaxValue : p.Time)
                .Select(p => p.Player)
                .ToList();

            var rewards = ImmutableDictionary.CreateBuilder<int, GamePlayerReward>();
            var xps = new List<(Player, int)>();
            for (var i = 0; i < rankedPlayers.Count; i++)
            {
                var playerData = rankedPlayers[i];
                var player = this.Players[playerData.Id];
                var matchXP = GameData.MatchXP(i);
                var matchKudos = GameData.MatchKudos(i);
                rewards.Add(playerData.LocalId, new GamePlayerReward(playerData.LocalId, i + 1, matchXP, matchKudos, 0));
                xps.Add((player, matchXP));

                playerData = playerData with
                {
                    Data = playerData.Data with
                    {
                        Matches = playerData.Data.Matches + 1,
                        Wins = playerData.Data.Wins + (i == 0 ? 1 : 0)
                    }
                };
                player.Data = playerData.Data;
            }
            this.State = this.State with { Rewards = rewards.ToImmutable() };

            foreach (var (player, xp) in xps)
            {
                this.GiveXP(player, xp);
            }
        }

        private void GiveXP(Player player, int xp)
        {
            var oldXP = player.Data.XP;
            player.Data = player.Data with { XP = player.Data.XP + xp };

            var newLevel = GameData.XPLevel(player.Data.XP);
            if (newLevel > GameData.XPLevel(oldXP))
            {
                this.BroadcastMessage(Message.System(
                    $"{player.Data.DisplayName} has just leveled up to Level {newLevel}: {GameData.LevelName(newLevel)}",
                    0x00CC00
                ));
                this.SendKudoBomb();
            }
        }

        private void GiveKudo(Player player)
        {
            if (!this.State.Players.TryGetValue(player.ConnectionId, out var data))
            {
                return;
            }

            player.Data = player.Data with { Kudos = player.Data.Kudos + 1 };
            if (this.State.Rewards.TryGetValue(data.LocalId, out var reward))
            {
                reward = reward with { ReceivedKudos = reward.ReceivedKudos + 1 };
                this.State = this.State with
                {
                    Rewards = this.State.Rewards.SetItem(data.LocalId, reward)
                };
            }
            this.GiveXP(player, 5);
        }

        private void SendKudoBomb()
        {
            var rewards = ImmutableDictionary.CreateBuilder<int, GamePlayerReward>();
            foreach (var (id, reward) in this.State.Rewards)
            {
                rewards.Add(id, reward with { AvailableKudos = reward.AvailableKudos + 2 });
            }
            this.State = this.State with { Rewards = rewards.ToImmutable() };

            this.BroadcastMessage(Message.System(
                $"KudoBomb!  As celebration, all room members earn +2 kudos to give!",
                0x00CC00
            ));
        }

        public void ReportPosition(Player player, GamePlayerPosition pos)
        {
            if (this.State.Phase != GamePhase.InGame)
            {
                return;
            }

            if (!this.State.Players.TryGetValue(player.ConnectionId, out var p))
            {
                return;
            }

            if (this.State.Positions.TryGetValue(p.LocalId, out var oldPos) && oldPos.CompletionTime == 0 && pos.CompletionTime > 0)
            {
                this.BroadcastMessage(Message.System($"{player.Data.DisplayName} has just beat the level."));
            }
            this.State = this.State with
            {
                Positions = this.State.Positions.SetItem(p.LocalId, pos),
                IsLevelFinished = this.State.IsLevelFinished || pos.CompletionTime > 0
            };
        }

        public void ReportCheckpoint(Player player, int id)
        {
            if (this.State.Phase != GamePhase.InGame)
            {
                return;
            }

            if (!this.State.Players.TryGetValue(player.ConnectionId, out var p))
            {
                return;
            }

            var cp = this.State.Checkpoints.GetValueOrDefault(p.LocalId) ?? new(ImmutableHashSet.Create<int>());
            cp = cp with { Ids = cp.Ids.Add(id) };
            this.State = this.State with
            {
                Checkpoints = this.State.Checkpoints.SetItem(p.LocalId, cp)
            };
        }

        public void GiveKudo(Player player, string targetId)
        {
            if (!this.State.Players.TryGetValue(player.ConnectionId, out var p))
            {
                return;
            }

            if (!this.Players.TryGetValue(targetId, out var target))
            {
                return;
            }

            var reward = this.State.Rewards.GetValueOrDefault(p.LocalId);
            if (reward == null || reward.AvailableKudos == 0)
            {
                return;
            }

            reward = reward with { AvailableKudos = reward.AvailableKudos - 1 };
            this.State = this.State with { Rewards = this.State.Rewards.SetItem(p.LocalId, reward) };
            this.GiveKudo(target);

            this.SendMessage(target, Message.System(
                $"{player.Data.DisplayName} has given you kudos!  You now have {target.Data.Kudos}"
            ));
        }

        public void SetNextLevel(Player player, string level)
        {
            if (this.State.Phase == GamePhase.InGame)
            {
                this.SendMessage(player, Message.Error("Game is already started."));
                return;
            }

            if (int.TryParse(level, out var levelNum))
            {
                var levelName = GameData.GameLevelName(levelNum);
                if (levelName == null)
                {
                    this.SendMessage(player, Message.Error("Invalid level number."));
                    return;
                }
                this.State = this.State with
                {
                    NextLevel = levelNum,
                    NextLevelCode = "",
                    NextLevelName = levelName,
                };
            }
            else
            {
                string? name = null;
                try
                {
                    name = LevelData.Parse(level).Name;
                }
                catch (Exception)
                {
                    this.SendMessage(player, Message.Error("Invalid level code."));
                    return;
                }
                this.State = this.State with
                {
                    NextLevel = 999,
                    NextLevelCode = level,
                    NextLevelName = name,
                };
            }
        }

        public override bool ProcessCommand(Player source, string name, string[] args)
        {
            if (base.ProcessCommand(source, name, args))
            {
                return true;
            }

            switch (name)
            {
                case "start":
                    {
                        if (this.State.Phase == GamePhase.InGame)
                        {
                            this.SendMessage(source, Message.Error("Game is already started."));
                            break;
                        }
                        else if (this.State.Players.Count == 0)
                        {
                            this.SendMessage(source, Message.Error("Not enough players."));
                            break;
                        }

                        this.StartPhase(GamePhase.InGame);
                        break;
                    }

                case "endgame":
                    {
                        if (source.IsSpectator)
                        {
                            this.SendMessage(source, Message.Error("Only players can end the game."));
                            break;
                        }
                        if (this.State.Phase != GamePhase.InGame)
                        {
                            this.SendMessage(source, Message.Error("Not in game."));
                            break;
                        }

                        this.StartPhase(GamePhase.Lobby);
                        break;
                    }

                case "resettime":
                    {
                        this.State = this.State with
                        {
                            Timer = this.State.Phase switch
                            {
                                GamePhase.InGame => this.Realm.Config.FinishCountdown * Realm.TPS,
                                GamePhase.Lobby => this.Realm.Config.GameCountdown * Realm.TPS,
                                _ => this.State.Timer,
                            }
                        };
                        break;
                    }

                default:
                    return false;
            }
            return true;
        }
    }
}
