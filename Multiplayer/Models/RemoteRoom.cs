namespace ExitPath.Server.Multiplayer.Models
{
    public record RemoteRoom
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public int NumPlayers { get; init; }

        public RemoteRoom(RoomGame room)
        {
            this.Id = room.Id;
            this.Name = room.Name;
            this.NumPlayers = room.Players.Count;
        }
    }
}
