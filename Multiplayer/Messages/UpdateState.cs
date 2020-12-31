namespace ExitPath.Server.Multiplayer.Messages
{
    public record UpdateState
    {
        public object Diff { get; init; } = new();
    }
}
