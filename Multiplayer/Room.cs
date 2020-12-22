namespace ExitPath.Server.Multiplayer
{
    public interface IRoom
    {
        string Id { get; }
        string Name { get; }

        void AddPlayer(Player player);
        void RemovePlayer(Player player);

        void Tick();
    }

    public abstract class Room<T> : IRoom
    {
        private T state;

        public Realm Realm { get; }
        public string Id { get; }
        public string Name { get; }

        public int StateVersion { get; private set; } = 0;
        public T State
        {
            get => state;
            set
            {
                this.state = value;
                this.StateVersion++;
            }
        }

        public Room(Realm realm, string id, string name, T state)
        {
            this.Realm = realm;
            this.Id = id;
            this.Name = name;
            this.state = state;
        }

        public virtual void AddPlayer(Player player)
        {

        }

        public virtual void RemovePlayer(Player player)
        {

        }

        public virtual void Tick()
        {

        }
    }
}
