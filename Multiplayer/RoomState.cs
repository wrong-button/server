namespace ExitPath.Server.Multiplayer
{
    public interface IRoomState
    {
        object ToJSON();
        object? Diff(object oldState);
    }

    public interface IRoomState<T> : IRoomState where T : IRoomState<T>
    {
        object? Diff(T oldState);
    }

    public abstract record RoomState<T> : IRoomState<T> where T : RoomState<T>
    {
        public abstract object? Diff(T oldState);
        public abstract object ToJSON();

        object? IRoomState.Diff(object oldState)
        {
            return this.Diff((T)oldState);
        }
    }
}
