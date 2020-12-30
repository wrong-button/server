namespace ExitPath.Server.Multiplayer.Models
{
    public record RemotePlayer
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public int Color { get; init; }

        public RemotePlayer(Player player)
        {
            this.Id = player.ConnectionId;
            this.Name = player.Data.DisplayName;
            this.Color = player.Data.PrimaryColor;
        }
    }
}
