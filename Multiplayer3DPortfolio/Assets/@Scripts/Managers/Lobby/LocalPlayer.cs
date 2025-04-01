using R3;
using System;

public enum EUserStatus
{
    None = 0,
    Connecting = 1,     // Join Lobby(O), Relay Connect(X)
    Lobby = 2,          // Join Lobby(O), Relay Connect(O)
    Ready = 3,          // Ready(O), ready to start game
    InGame = 4,
    Menu = 5,
}

public class LocalPlayer : IDisposable
{
    public ReactiveProperty<bool> IsHost = new ReactiveProperty<bool>(false);
    public ReactiveProperty<string> DisplayName = new ReactiveProperty<string>("");
    public ReactiveProperty<EUserStatus> UserStatus = new ReactiveProperty<EUserStatus>(EUserStatus.None);
    public ReactiveProperty<string> ID = new ReactiveProperty<string>("");
    public ReactiveProperty<int> Index = new ReactiveProperty<int>(0);

    public DateTime LastUpdated;

    public LocalPlayer(string id, int index, bool isHost, string displayName = "", EUserStatus status = EUserStatus.None)
    {
        ID.Value = id;
        Index.Value = index;
        IsHost.Value = isHost;
        DisplayName.Value = displayName;
        UserStatus.Value = status;
    }
    
    public void ResetState()
    {
        IsHost.Value = false;
        UserStatus.Value = EUserStatus.Menu;
    }

    public void Dispose()
    {
        IsHost.Dispose();
        DisplayName.Dispose();
        UserStatus.Dispose();
        ID.Dispose();
        Index.Dispose();
    }

}