using ExitPath.Server.Models;

namespace ExitPath.Server.Multiplayer
{
    public class Player
    {
        public string ConnectionId { get; }
        public PlayerData Data { get; }

        public int StateVersion { get; set; } = 0;
        public object? State { get; set; } = null;

        public Player(string connId, PlayerData data)
        {
            this.ConnectionId = connId;
            this.Data = data;
        }
    }
}
