namespace ExitPath.Server.Multiplayer.Messages
{
    public record UpdateState
    {
        public object NewState { get; init; } = new();
    }
}
