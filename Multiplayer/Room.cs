using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public interface IRoom
    {
        string Id { get; }
        string Name { get; }
        Realm Realm { get; }

        object State { get; }
        ImmutableDictionary<string, Player> Players { get; }

        void AddPlayer(Player player);
        void RemovePlayer(Player player);

        Task Tick();
    }

    public abstract class Room<T> : IRoom where T : notnull
    {
        public Realm Realm { get; }
        public string Id { get; }
        public string Name { get; }

        public T State { get; set; }
        object IRoom.State => this.State;
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
            // TODO: send JoinRoom message
        }

        public virtual void RemovePlayer(Player player)
        {
            this.Players = this.Players.Remove(player.ConnectionId);
        }

        public virtual async Task Tick()
        {
            foreach (var player in this.Players.Values)
            {
                await player.Tick(this);
            }
        }
    }
}
