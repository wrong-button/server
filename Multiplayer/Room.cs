using ExitPath.Server.Multiplayer.Messages;
using System.Collections.Immutable;

namespace ExitPath.Server.Multiplayer
{
    public interface IRoom
    {
        string Id { get; }
        string Name { get; }
        Realm Realm { get; }

        IRoomState State { get; }
        ImmutableDictionary<string, Player> Players { get; }

        void AddPlayer(Player player);
        void RemovePlayer(Player player);

        void Tick();
        bool ProcessCommand(Player source, string name, string[] args);

        void SendMessage(Player player, Message msg);
        void BroadcastMessage(Message msg);
    }

    public abstract class Room<T> : IRoom where T : IRoomState<T>
    {
        public Realm Realm { get; }
        public string Id { get; }
        public string Name { get; }

        public T State { get; set; }
        IRoomState IRoom.State => this.State;
        public ImmutableDictionary<string, Player> Players { get; private set; } = ImmutableDictionary.Create<string, Player>();

        public Room(Realm realm, string id, string name, T state)
        {
            this.Realm = realm;
            this.Id = id;
            this.Name = name;
            this.State = state;
        }

        public virtual void AddPlayer(Player player)
        {
            this.Players = this.Players.Add(player.ConnectionId, player);
            player.OnJoinRoom(this);
        }

        public virtual void RemovePlayer(Player player)
        {
            this.Players = this.Players.Remove(player.ConnectionId);
        }

        public virtual void Tick()
        {
            foreach (var player in this.Players.Values)
            {
                player.Tick(this);
            }
        }

        public virtual bool ProcessCommand(Player source, string name, string[] args)
        {
            return false;
        }

        public void SendMessage(Player player, Message msg)
        {
            this.Realm.SendMessage(player, msg);
        }

        public void BroadcastMessage(Message msg)
        {
            foreach (var player in this.Players.Values)
            {
                this.Realm.SendMessage(player, msg);
            }
        }
    }
}
