namespace ExitPath.Server.Multiplayer
{
    public record RoomLobbyState
    {

    }

    public class RoomLobby : Room<RoomLobbyState>
    {
        public RoomLobby(Realm realm) : base(realm, "lobby", "Party Room", new RoomLobbyState())
        {
        }
    }
}
