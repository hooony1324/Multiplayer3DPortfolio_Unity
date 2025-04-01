using System;
using System.Collections.Generic;
using System.Text;
using R3;

public enum ELobbyState
{
    Lobby,
    CountDown,
}

public class LocalLobby
{
    public Action<LocalPlayer> OnUserJoined;
    public Action<int> OnUserLeft;
    public Action<int> OnUserReadyChanged;
    public ReactiveProperty<string> LobbyID = new ReactiveProperty<string>("");
    public ReactiveProperty<string> LobbyCode = new ReactiveProperty<string>("");
    public ReactiveProperty<string> RelayCode = new ReactiveProperty<string>("");
    public ReactiveProperty<ServerAddress> RelayServer = new ReactiveProperty<ServerAddress>();
    public ReactiveProperty<string> LobbyName = new ReactiveProperty<string>("");
    public ReactiveProperty<string> HostID = new ReactiveProperty<string>("");
    public ReactiveProperty<ELobbyState> LobbyState = new ReactiveProperty<ELobbyState>(ELobbyState.Lobby);
    public ReactiveProperty<bool> Locked = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> Private = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> AvailableSlots = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> MaxPlayerCount = new ReactiveProperty<int>(0);
    public ReactiveProperty<long> LastUpdated = new ReactiveProperty<long>(0);


    private List<LocalPlayer> _localPlayers = new List<LocalPlayer>();
    private ServerAddress _relayServer;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public List<LocalPlayer> LocalPlayers => _localPlayers;
    public int PlayerCount => _localPlayers.Count;


    public void ResetLobby()
    {
        _localPlayers.Clear();

        LobbyName.Value = "";
        LobbyID.Value = "";
        LobbyCode.Value = "";
        Locked.Value = false;
        Private.Value = false;
        AvailableSlots.Value = 4;
        MaxPlayerCount.Value = 4;
        OnUserJoined = null;
        OnUserLeft = null;
    }

    public LocalLobby()
    {
        LastUpdated.Value = DateTime.Now.ToFileTimeUtc();

        _disposables.Add(LobbyID);
        _disposables.Add(LobbyCode);
        _disposables.Add(RelayCode);
        _disposables.Add(RelayServer);
        _disposables.Add(LobbyName);
        _disposables.Add(LobbyState);
        _disposables.Add(Locked);
        _disposables.Add(Private);
        _disposables.Add(AvailableSlots);
        _disposables.Add(MaxPlayerCount);
        _disposables.Add(LastUpdated);

        _disposables.Add(HostID.Subscribe(OnHostChanged));

    }

    public void Dispose()
    {
        _disposables.Dispose();

        OnUserJoined = null;
        OnUserLeft = null;
        OnUserReadyChanged = null;
    }

    private void OnHostChanged(string newHostID)
    {
        foreach (var player in _localPlayers)
        {
            player.IsHost.Value = player.ID.Value == newHostID;
        }
    }

    public LocalPlayer GetLocalPlayer(int index)
    {
        return index < PlayerCount ? _localPlayers[index] : null;
    }

    public void AddPlayer(int playerIndex, LocalPlayer user)
    {
        _localPlayers.Insert(playerIndex, user);
        user.UserStatus.Subscribe(OnUserStatusChanged);
    }

    public void RemovePlayer(int playerIndex)
    {
        _localPlayers[playerIndex].UserStatus.Dispose();
        _localPlayers.RemoveAt(playerIndex);
        OnUserLeft?.Invoke(playerIndex);
    }

    void OnUserStatusChanged(EUserStatus status)
    {
        int readyCount = 0;
        foreach (var player in _localPlayers)
        {
            if (player.UserStatus.Value == EUserStatus.Ready)
                readyCount++;
        }

        OnUserReadyChanged?.Invoke(readyCount);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("Lobby : ");
        sb.AppendLine(LobbyName.Value);
        sb.Append("ID: ");
        sb.AppendLine(LobbyID.Value);
        sb.Append("Code: ");
        sb.AppendLine(LobbyCode.Value);
        sb.Append("Locked: ");
        sb.AppendLine(Locked.Value.ToString());
        sb.Append("Private: ");
        sb.AppendLine(Private.Value.ToString());
        sb.Append("AvailableSlots: ");
        sb.AppendLine(AvailableSlots.Value.ToString());
        sb.Append("Max Players: ");
        sb.AppendLine(MaxPlayerCount.Value.ToString());
        sb.Append("LobbyState: ");
        sb.AppendLine(LobbyState.Value.ToString());
        sb.Append("Lobby LocalLobbyState Last Edit: ");
        sb.AppendLine(new DateTime(LastUpdated.Value).ToString());
        sb.Append("RelayCode: ");
        sb.AppendLine(RelayCode.Value);

        return sb.ToString();
    }
}