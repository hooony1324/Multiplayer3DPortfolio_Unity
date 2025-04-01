public static class Define
{

    public enum ERequestType
    {
        Query = 0,
        Join,
        QuickJoin,
        Host
    }

    public enum EGameState
    {
        Menu = 1,
        Lobby = 2,
        InGame = 3,
        JoinMenu = 4,
    }

    public const string key_RelayCode = "RelayCode";
    public const string key_LobbyState = "LobbyState";
    public const string key_Displayname = "DisplayName";
    public const string key_Userstatus = "UserStatus";
    public const int k_maxLobbiesToShow = 10;
}