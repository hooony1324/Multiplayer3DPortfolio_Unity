using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static Define;
using R3;
using System.Collections;


public class LobbyManager : MonoBehaviour
{
    LocalLobby _localLobby;
    LocalPlayer _localUser;
    public LocalLobby LocalLobby => _localLobby;
    public LocalLobbyList LobbyList { get; private set; } = new();
    public Lobby CurrentLobby { get; private set; }
    private LobbyEventCallbacks _lobbyEventCallbacks = new();


    private void Awake()
    {
        Application.wantsToQuit += OnWantsToQuit;
        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LobbyState = { Value = ELobbyState.Lobby } };
    }

    private void OnDestroy()
    {
        if (CurrentLobby != null)
        {
            TryQuitLobby().Forget();
        }
        
        TryForcedLeave();
        Dispose();
    }

    public void Dispose()
    {
        CurrentLobby = null;
        _lobbyEventCallbacks = new LobbyEventCallbacks();
    }

    #region Manage Lobby
    private async UniTaskVoid StartHeartbeatLoop()
    {
        while (CurrentLobby != null)
        {
            try
            {
                await _heartbeatLimiter.QueueUntilCooldown();
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (Exception e)
            {
                Debug.LogError($"하트비트 전송 실패: {e.Message}");
                break;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(8));
        }
    }

    private Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
    {
        Dictionary<string, PlayerDataObject> data = new();

        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
        data.Add("DisplayName", displayNameObject);
        return data;
    }

    void ParseCustomPlayerData(LocalPlayer player, string dataKey, string playerDataValue)
    {
        if (dataKey == key_Userstatus)
            player.UserStatus.Value = (EUserStatus)int.Parse(playerDataValue);
        else if (dataKey == key_Displayname)
            player.DisplayName.Value = playerDataValue;
    }

    public async UniTask BindLocalLobbyToRemote(string lobbyId, LocalLobby localLobby)
    {
        _lobbyEventCallbacks.LobbyDeleted += async () =>
        {
            await LeaveLobbyAsync();
        };

        _lobbyEventCallbacks.DataChanged += changes =>
        {
            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = changedValue.Value.Value;

                if (changedKey == key_LobbyState)
                    localLobby.LobbyState.Value = (ELobbyState)int.Parse(changedValue.Value.Value);
            }
        };

        _lobbyEventCallbacks.DataAdded += changes =>
        {
            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = changedValue.Value.Value;

                if (changedKey == key_LobbyState)
                    localLobby.LobbyState.Value = (ELobbyState)int.Parse(changedValue.Value.Value);
            }
        };

        _lobbyEventCallbacks.DataRemoved += changes =>
        {
            foreach (var change in changes)
            {
                var changedKey = change.Key;
                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = "";
            }
        };

        _lobbyEventCallbacks.PlayerLeft += players =>
        {
            foreach (var leftPlayerIndex in players)
            {
                localLobby.RemovePlayer(leftPlayerIndex);
            }
        };

        _lobbyEventCallbacks.PlayerJoined += players =>
        {
            foreach (var playerChanges in players)
            {
                Player joinedPlayer = playerChanges.Player;

                var id = joinedPlayer.Id;
                var index = playerChanges.PlayerIndex;
                var isHost = localLobby.HostID.Value == id;

                var newPlayer = new LocalPlayer(id, index, isHost);

                foreach (var dataEntry in joinedPlayer.Data)
                {
                    var dataObject = dataEntry.Value;
                    ParseCustomPlayerData(newPlayer, dataEntry.Key, dataObject.Value);
                }

                localLobby.AddPlayer(index, newPlayer);
            }
        };

        _lobbyEventCallbacks.PlayerDataChanged += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                foreach (var playerChange in playerChanges)
                {
                    var changedValue = playerChange.Value;

                    //There are changes on some of the changes in the player list of changes
                    var playerDataObject = changedValue.Value;
                    ParseCustomPlayerData(localPlayer, playerChange.Key, playerDataObject.Value);
                }
            }
        };

        _lobbyEventCallbacks.PlayerDataAdded += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                foreach (var playerChange in playerChanges)
                {
                    var changedValue = playerChange.Value;

                    //There are changes on some of the changes in the player list of changes
                    var playerDataObject = changedValue.Value;
                    ParseCustomPlayerData(localPlayer, playerChange.Key, playerDataObject.Value);
                }
            }
        };

        _lobbyEventCallbacks.PlayerDataRemoved += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                if (playerChanges == null)
                    continue;

                foreach (var playerChange in playerChanges.Values)
                {
                    //There are changes on some of the changes in the player list of changes
                    Debug.LogWarning("This Sample does not remove Player Values currently.");
                }
            }
        };

        _lobbyEventCallbacks.LobbyChanged += async changes =>
        {
            //Lobby Fields
            if (changes.Name.Changed)
                localLobby.LobbyName.Value = changes.Name.Value;
            if (changes.HostId.Changed)
                localLobby.HostID.Value = changes.HostId.Value;
            if (changes.IsPrivate.Changed)
                localLobby.Private.Value = changes.IsPrivate.Value;
            if (changes.IsLocked.Changed)
                localLobby.Locked.Value = changes.IsLocked.Value;
            if (changes.AvailableSlots.Changed)
                localLobby.AvailableSlots.Value = changes.AvailableSlots.Value;
            if (changes.MaxPlayers.Changed)
                localLobby.MaxPlayerCount.Value = changes.MaxPlayers.Value;

            if (changes.LastUpdated.Changed)
                localLobby.LastUpdated.Value = changes.LastUpdated.Value.ToFileTimeUtc();

            //Custom Lobby Fields

            if (changes.PlayerData.Changed)
                PlayerDataChanged();

            void PlayerDataChanged()
            {
                foreach (var lobbyPlayerChanges in changes.PlayerData.Value)
                {
                    var playerIndex = lobbyPlayerChanges.Key;
                    var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                    if (localPlayer == null)
                        continue;
                    var playerChanges = lobbyPlayerChanges.Value;
                    if (playerChanges.ConnectionInfoChanged.Changed)
                    {
                        var connectionInfo = playerChanges.ConnectionInfoChanged.Value;
                        Debug.Log(
                            $"ConnectionInfo for player {playerIndex} changed to {connectionInfo}");
                    }

                    if (playerChanges.LastUpdatedChanged.Changed) { }
                }
            }
        };

        _lobbyEventCallbacks.LobbyEventConnectionStateChanged += lobbyEventConnectionState =>
        {
            Debug.Log($"Lobby ConnectionState Changed to {lobbyEventConnectionState}");
        };

        _lobbyEventCallbacks.KickedFromLobby += () =>
        {
            Debug.Log("Left Lobby");
            Dispose();
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, _lobbyEventCallbacks);
    }

    public async UniTask<Lobby> GetLobbyAsync(string lobbyId = null)
    {
        if (CurrentLobby == null)
            return null;

        await _getLobbyLimiter.QueueUntilCooldown();
        lobbyId ??= CurrentLobby.Id;
        return CurrentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
    }

    public async UniTask LeaveLobbyAsync()
    {
        await _leaveLobbyOrRemovePlayerLimiter.QueueUntilCooldown();
        if (CurrentLobby == null)
            return;

        string playerId = AuthenticationService.Instance.PlayerId;

        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        Dispose();
    }

    public void RefreshLobbies()
    {
        QueryLobbies().Forget();
    }
    public async UniTaskVoid QueryLobbies()
    {
        LobbyList.QueryState.Value = ELobbyQueryState.Fetching;
        var query = await Managers.Lobby.RetrieveLobbyListAsync();
        if (query == null)
        {
            return;
        }

        SetCurrentLobbies(LobbyConverters.QueryToLocalList(query));
    }
    void SetCurrentLobbies(List<LocalLobby> lobbies)
    {
        var newLobbies = new Dictionary<string, LocalLobby>();
        foreach (var lobby in lobbies)
            newLobbies.Add(lobby.LobbyID.Value, lobby);

        LobbyList.CurrentLobbies.Value = newLobbies;
        LobbyList.QueryState.Value = ELobbyQueryState.Fetched;
    }
    public async UniTask<QueryResponse> RetrieveLobbyListAsync()
    {
        if (_queryLimiter.TaskQueued)
            return null;

        await _queryLimiter.QueueUntilCooldown();

        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            Count = k_maxLobbiesToShow,
        };

        return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
    }

    public async UniTask UpdatePlayerDataAsync(Dictionary<string, string> data)
    {
        if (CurrentLobby == null) return;

        string playerId = AuthenticationService.Instance.PlayerId;
        Dictionary<string, PlayerDataObject> playerData = new();
        foreach (var newData in data)
        {
            PlayerDataObject dataObject = new PlayerDataObject(
                visibility: PlayerDataObject.VisibilityOptions.Member,
                value: newData.Value
            );

            if (playerData.ContainsKey(newData.Key))
                playerData[newData.Key] = dataObject;
            else
                playerData.Add(newData.Key, dataObject);
        }

        if (_updatePlayerDataLimiter.TaskQueued)
            return;
        await _updatePlayerDataLimiter.QueueUntilCooldown();

        UpdatePlayerOptions options = new UpdatePlayerOptions 
        { 
            Data = playerData,
            AllocationId = null,
            ConnectionInfo = null,
        };

        CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, playerId, new UpdatePlayerOptions { Data = playerData }); 
    }

    public async UniTask UpdatePlayerRelayInfoAsync(string lobbyId, string allocationId, string connectionInfo)
    {
        if (CurrentLobby == null)
            return;

        string playerId = AuthenticationService.Instance.PlayerId;

        if (_updatePlayerDataLimiter.TaskQueued)
            return;
        await _updatePlayerDataLimiter.QueueUntilCooldown();

        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>(),
            AllocationId = allocationId,
            ConnectionInfo = connectionInfo,
        };

        CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, playerId, options);
    }


    public async UniTask UpdateLobbyDataAsync(Dictionary<string, string> data)
    {
        if (CurrentLobby == null)
            return;

        Dictionary<string, DataObject> currentData = CurrentLobby.Data ?? new();

        var shouldLock = false;
        foreach (var newData in data)
        {
            DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newData.Value);

            if (currentData.ContainsKey(newData.Key))
                currentData[newData.Key] = dataObject;
            else
                currentData.Add(newData.Key, dataObject);

            if (newData.Key == key_LobbyState)
            {
                Enum.TryParse(newData.Value, out ELobbyState lobbyState);
                shouldLock = lobbyState != ELobbyState.Lobby;
            }
        }

        if (_updateLobbyLimiter.TaskQueued)
            return;
        await _updateLobbyLimiter.QueueUntilCooldown();

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = currentData, IsLocked = shouldLock };
        CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOptions);
        
    }

    public async UniTask DeleteLobbyAsync()
    {
        if (CurrentLobby == null)
            return;

        await _deleteLobbyLimiter.QueueUntilCooldown();

        await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
    }
    public async void CreateLobby(string name, bool isPrivate, string password = null, int maxPlayers = 4)
    {
        try
        {
            var lobby = await Managers.Lobby.CreateLobbyAsync(
                name,
                maxPlayers,
                isPrivate,
                _localUser,
                password);


            LobbyConverters.RemoteToLocal(lobby, ref _localLobby);
            await CreateLobby();
        }
        catch (LobbyServiceException exception)
        {
            Managers.Game.SetGameState(EGameState.JoinMenu);
            Managers.UI.ShowPopupMessage($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }
    async UniTask CreateLobby()
    {
        _localUser.IsHost.Value = true;
        _localLobby.OnUserReadyChanged = OnPlayersReady;
        
        Managers.Game.LocalGameState.Value = EGameState.Lobby;
        try
        {
            await BindLobby();
        }
        catch (LobbyServiceException exception)
        {
            Managers.Game.SetGameState(EGameState.JoinMenu);
            Managers.UI.ShowPopupMessage($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    // Only Host
    public void HostSetRelayCode(string code)
    {
        _localLobby.RelayCode.Value = code;
        SendLocalLobbyData();
    }

    // Only Host
    void OnPlayersReady(int readyCount)
    {
        if (readyCount == _localLobby.PlayerCount &&
            _localLobby.LobbyState.Value != ELobbyState.CountDown)
        {
            _localLobby.LobbyState.Value = ELobbyState.CountDown;
            SendLocalLobbyData();
        }
        else if (_localLobby.LobbyState.Value == ELobbyState.CountDown)
        {
            _localLobby.LobbyState.Value = ELobbyState.Lobby;
            SendLocalLobbyData();
        }
    }
    void OnLobbyStateChanged(ELobbyState state)
    {
        switch (state)
        {
            case ELobbyState.Lobby:
                // CancelCountDown(); UI카운트다운
                break;
            case ELobbyState.CountDown:
                // StartCountDown();
                break;
        }
    }

    public void SetLocalUserName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        _localUser.DisplayName.Value = name;
        SendLocalUserData();
    }

    public void SetLocalUserStatus(EUserStatus state)
    {
        _localUser.UserStatus.Value = state;
        SendLocalUserData();
    }

    async void SendLocalLobbyData()
    {
        await UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
    }
    async void SendLocalUserData()
    {
        await UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
    }

    #endregion


    #region Joining
    public async void JoinLobby(string lobbyId, string lobbyCode, string password = null)
    {
        try
        {
            var lobby = await JoinLobbyAsync(lobbyId, lobbyCode, _localUser, password:password);

            LobbyConverters.RemoteToLocal(lobby, ref _localLobby);
            await JoinLobby();
        }
        catch (LobbyServiceException exception)
        {
            Managers.Game.LocalGameState.Value = EGameState.JoinMenu;
            Managers.UI.ShowPopupMessage($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    async UniTask<Lobby> JoinLobbyAsync(string lobbyId, string lobbyCode, LocalPlayer localUser, string password = null)
    {
        if (_joinLimiter.IsCoolingDown || lobbyId == null && lobbyCode == null)
            return null;

        await _joinLimiter.QueueUntilCooldown();

        string uasId = AuthenticationService.Instance.PlayerId;
        var playerData = CreateInitialPlayerData(localUser);

        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
            {
                Player = new Player(id: uasId, data: playerData),
                Password = password,
            };
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }
        else
        {
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player(id: uasId, data: playerData),
                Password = password,
            };
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
        }

        return CurrentLobby;
    }

    async UniTask JoinLobby()
    {
        _localUser.IsHost.OnNext(false);
        await BindLobby();
    }

    async UniTask BindLobby()
    {
        await Managers.Lobby.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
        _localLobby.LobbyState.Subscribe(OnLobbyStateChanged);

        // SetLobbyView : 로비 안으로 들어옴, UI 세팅
        // GameState => Lobby 변경

    }

    public async UniTask<Lobby> QuickJoinLobby(Dictionary<QueryFilter.FieldOptions, string> filterData = null)
    {
        if (CurrentLobby != null)
            await TryQuitLobby();

        CurrentLobby = null;

        await _quickJoinLimiter.QueueUntilCooldown();

        try
        {
            var filter = new QuickJoinLobbyOptions
            {
                Filter = new List<QueryFilter>()
            };

            if (filterData != null)
            {
                foreach (var (key, value) in filterData)
                {
                    var filterItem = new QueryFilter(key, value, QueryFilter.OpOptions.EQ);
                    filter.Filter.Add(filterItem);
                }
            }

            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(filter);
            if (lobby == null)
                return null;

            CurrentLobby = lobby;
            Debug.Log($"Quick Joined Lobby: {CurrentLobby.Id}", this);

            return CurrentLobby;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to quick join lobby: {e.Message}", this);
            return null;
        }
    }

    public async UniTask<bool> TryQuitLobby()
    {
        if (CurrentLobby == null)
            return false;

        await _leaveLobbyOrRemovePlayerLimiter.QueueUntilCooldown();

        try
        {
            var id = CurrentLobby.Id;
            CurrentLobby = null;

            await LobbyService.Instance.RemovePlayerAsync(id, AuthenticationService.Instance.PlayerId);
            Debug.Log("Quit Lobby", this);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to quit lobby: {e.Message}", this);
            return false;
        }
    }

    #endregion


    #region Hosting

    private async UniTask<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate,
            LocalPlayer localUser, string password)
    {
        await _createLimiter.QueueUntilCooldown();

        string userId = AuthenticationService.Instance.PlayerId;

        CreateLobbyOptions createOptions = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = new Player(id: userId, data: CreateInitialPlayerData(localUser)),
            Password = password,
        };

        CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        StartHeartbeatLoop().Forget();

        return CurrentLobby;
    }

    public async UniTask<bool> TryDeleteLobby()
    {
        if (CurrentLobby == null)
            return false;

        await _deleteLobbyLimiter.QueueUntilCooldown();

        try
        {
            var id = CurrentLobby.Id;
            CurrentLobby = null;

            await LobbyService.Instance.DeleteLobbyAsync(id);
            Debug.Log("Lobby Deleted", this);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to delete lobby: {e.Message}", this);
            return false;
        }
    }

    #endregion
    public void LeaveLobby()
    {
        _localUser.ResetState();
#pragma warning disable 4014
        Managers.Lobby.LeaveLobbyAsync();
#pragma warning restore 4014
        LobbyList.Clear();
    }

    IEnumerator RetryConnection(Action doConnection, string lobbyId)
    {
        yield return new WaitForSeconds(5);
        if (_localLobby != null && _localLobby.LobbyID.Value == lobbyId && !string.IsNullOrEmpty(lobbyId))
            doConnection?.Invoke();
    }

    void ResetLocalLobby()
    {
        _localLobby.ResetLobby();
        _localLobby.RelayServer = null;
    }


    #region Teardown
    // On Application Quit할 때 Leave Request 발생 시, 한 프레임 안에서는 제대로 작동 안함
    // 따라서 한 프레임 뒤에 재시도 하는 방식으로 처리
    bool OnWantsToQuit()
    {
        bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID.Value);
        StartCoroutine(LeaveBeforeQuit());
        return canQuit;
    }

    void TryForcedLeave()
    {
        if (!string.IsNullOrEmpty(_localLobby?.LobbyID.Value))
        {
#pragma warning disable 4014
            Managers.Lobby.LeaveLobbyAsync().Forget();
#pragma warning restore 4014
            _localLobby = null;
        }
    }

    IEnumerator LeaveBeforeQuit()
    {
        TryForcedLeave();

        yield return null;
        Application.Quit();
    }
    #endregion

    #region Rate Limiting
    //Manages the Amount of times you can hit a service call.
    //Adds a buffer to account for ping times.
    //Will Queue the latest overflow task for when the cooldown ends.
    //Created to mimic the way rate limits are implemented Here:  https://docs.unity.com/lobby/rate-limits.html

    // Rate Limiters
    private readonly RateLimiter _queryLimiter = new(1, 1f);         // 초당 1회
    private readonly RateLimiter _getLobbyLimiter = new(1, 1f);         // 초당 1회
    private readonly RateLimiter _createLimiter = new(2, 6f);        // 6초당 2회
    private readonly RateLimiter _joinLimiter = new(2, 6f);         // 6초당 2회
    private readonly RateLimiter _quickJoinLimiter = new(1, 10f);    // 10초당 1회
    private readonly RateLimiter _deleteLobbyLimiter = new(2, 1f);    // 1초당 2회
    private readonly RateLimiter _updateLobbyLimiter = new(5, 5f);    // 5초당 5회
    private readonly RateLimiter _updatePlayerDataLimiter = new(5, 5f); // 5초당 5회
    private readonly RateLimiter _leaveLobbyOrRemovePlayerLimiter = new(5, 1f); // 1초당 5회
    private readonly RateLimiter _heartbeatLimiter = new(5, 30f);     // 30초당 5회

    public RateLimiter GetRateLimiter(ERequestType type)
    {
        switch (type)
        {
            case ERequestType.Join:
                return _joinLimiter;
            case ERequestType.QuickJoin:
                return _quickJoinLimiter;
            case ERequestType.Host:
                return _createLimiter;
            default:
                return _queryLimiter;
        }
    }

    #endregion
}
