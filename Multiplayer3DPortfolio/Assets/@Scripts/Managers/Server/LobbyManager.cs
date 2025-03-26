using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;


public struct LobbyListUpdateEvent : IEventData
{
    public QueryResponse QueryResponse { get; }
    public LobbyListUpdateEvent(QueryResponse queryResponse)
    {
        QueryResponse = queryResponse;
    }
}


public class LobbyManager : MonoBehaviour
{
    public Lobby CurrentLobby { get; private set; }
    private LobbyEventCallbacks _lobbyEventCallbacks = new();

    private async UniTaskVoid StartHeartbeatLoop()
    {
        while (this != null && CurrentLobby != null)
        {
            await SendHeartbeat();
            await UniTask.Delay(TimeSpan.FromSeconds(15));
        }
    }

    private async UniTask SendHeartbeat()
    {
        if (CurrentLobby == null) return;

        await _heartbeatLimiter.WaitForRequest();

        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            Debug.Log("하트비트 전송 성공");
        }
        catch (Exception e)
        {
            Debug.LogError($"하트비트 전송 실패: {e.Message}");
        }
    }


    #region Joining
    public async UniTask<Lobby> QuickJoinLobby(Dictionary<QueryFilter.FieldOptions, string> filterData = null)
    {
        if (CurrentLobby != null)
            await TryQuitLobby();

        CurrentLobby = null;

        await _quickJoinLimiter.WaitForRequest();

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

    public void RefreshLobbyList()
    {
        QueryLobbies().Forget();
    }

    private async UniTask<QueryResponse> QueryLobbies()
    {
        await _queryLimiter.WaitForRequest();

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            EventBus.Publish(new LobbyListUpdateEvent(lobbies));
            return lobbies;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 리스트 업데이트 실패: {e.Message}");
            return null;
        }
    }


    #endregion


    #region Hosting
    private void OnDestroy()
    {
        if (CurrentLobby == null)
            return;

        TryDeleteLobby().Forget();
    }

    public void CreateLobbyWithPassword(int maxPlayers, string lobbyName, string password = null)
    {
        // 비밀번호 유효성 검사 추가
        if (password != null && password.Length < 8)
        {
            Managers.UI.ShowPopupMessage("Password must be at least 8 characters long");
            return;
        }

        CreateLobby(maxPlayers, lobbyName, password).Forget();
    }

    private async UniTask<Lobby> CreateLobby(int maxPlayers, string lobbyName = null, string password = null, Dictionary<string, string> publicData = null,
     Dictionary<string, string> privateData = null, Dictionary<string, string> memberData = null)
    {
        if (CurrentLobby != null)
            await TryDeleteLobby();

        await _createLimiter.WaitForRequest();

        try
        {
            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>()
            };

            if (publicData != null)
            {
                if (publicData.Count > 10)
                {
                    Debug.LogWarning("Public data cannot have more than 10 entries", this);
                    return null;
                }

                var index = DataObject.IndexOptions.S1;

                foreach (var (key, value) in publicData)
                {
                    createLobbyOptions.Data.Add(key, new DataObject(DataObject.VisibilityOptions.Public, value, index));
                    index++;
                }
            }

            if (privateData != null)
            {
                foreach (var (key, value) in privateData)
                    createLobbyOptions.Data.Add(key, new DataObject(DataObject.VisibilityOptions.Private, value));
            }

            if (memberData != null)
            {
                foreach (var (key, value) in memberData)
                    createLobbyOptions.Data.Add(key, new DataObject(DataObject.VisibilityOptions.Member, value));
            }

            if (lobbyName == null)
                lobbyName = MyTools.GetRandomString(10);

            if (password != null)
                createLobbyOptions.Password = password;

            CurrentLobby =
                await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            Debug.Log($"Lobby Created: {CurrentLobby.Id}", this);

            StartHeartbeatLoop().Forget();
            return CurrentLobby;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to create lobby: {e.Message}", this);
            return null;
        }
    }

    public async UniTask<bool> TryDeleteLobby()
    {
        if (CurrentLobby == null)
            return false;

        await _deleteLobbyLimiter.WaitForRequest();

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


    #region Rate Limiting
    //Manages the Amount of times you can hit a service call.
    //Adds a buffer to account for ping times.
    //Will Queue the latest overflow task for when the cooldown ends.
    //Created to mimic the way rate limits are implemented Here:  https://docs.unity.com/lobby/rate-limits.html

    // Rate Limiters
    private readonly ServiceRateLimiter _queryLimiter = new(1, 5f);         // 초당 1회
    private readonly ServiceRateLimiter _createLimiter = new(2, 6f);        // 6초당 2회
    private readonly ServiceRateLimiter _joinLimiter = new(2, 6f);         // 6초당 2회
    private readonly ServiceRateLimiter _quickJoinLimiter = new(1, 10f);    // 10초당 1회
    private readonly ServiceRateLimiter _deleteLobbyLimiter = new(2, 1f);    // 1초당 2회
    private readonly ServiceRateLimiter _updateLobbyLimiter = new(5, 5f);    // 5초당 5회
    private readonly ServiceRateLimiter _updatePlayerDataLimiter = new(5, 5f); // 5초당 5회
    private readonly ServiceRateLimiter _leaveLobbyOrRemovePlayerLimiter = new(5, 1f); // 1초당 5회
    private readonly ServiceRateLimiter _heartbeatLimiter = new(5, 30f);     // 30초당 5회


    public enum RequestType
    {
        Query = 0,
        Join,
        QuickJoin,
        Host
    }
    public ServiceRateLimiter GetRateLimit(RequestType type)
    {
        switch (type)
        {
            case RequestType.Join:
                return _joinLimiter;
            case RequestType.QuickJoin:
                return _quickJoinLimiter;
            case RequestType.Host:
                return _createLimiter;
            default:
                return _queryLimiter;
        }
    }

    public class ServiceRateLimiter
    {
        public event Action<bool> OnCooldownStateChanged; // 이벤트 이름을 더 명확하게
        private readonly int _cooldownMS;
        private readonly int _maxRequests;
        private int _remainingRequests;
        private bool _isCoolingDown;

        public bool IsCoolingDown
        {
            get => _isCoolingDown;
            private set
            {
                if (_isCoolingDown != value)
                {
                    _isCoolingDown = value;
                    OnCooldownStateChanged?.Invoke(value);
                    Debug.Log($"Rate Limit 상태 변경: {value}"); // 디버깅용
                }
            }
        }

        public ServiceRateLimiter(int maxRequests, float cooldownSeconds, int pingBuffer = 100)
        {
            _maxRequests = maxRequests;
            _remainingRequests = maxRequests;
            _cooldownMS = Mathf.CeilToInt(cooldownSeconds * 1000) + pingBuffer;
        }

        public async UniTask WaitForRequest()
        {
            if (!IsCoolingDown && _remainingRequests <= 0)
            {
                IsCoolingDown = true;
                await UniTask.Delay(_cooldownMS);
                _remainingRequests = _maxRequests;
                IsCoolingDown = false;
            }

            _remainingRequests--;
        }
    }


    #endregion
}
