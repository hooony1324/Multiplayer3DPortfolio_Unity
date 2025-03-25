using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    #region Joining
    public Lobby CurrentLobby { get; private set; }

    public async UniTask<Lobby> QuickJoinLobby(Dictionary<QueryFilter.FieldOptions, string> filterData = null)
    {
        if (CurrentLobby != null)
            await TryQuitLobby();

        CurrentLobby = null;

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

    public async UniTaskVoid GetLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                asc: false,
                field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            Debug.Log($"Lobbies: {lobbies.Results.Count}");

            foreach (var lobby in lobbies.Results)
            {
                Debug.Log($"Lobby: {lobby.Name}");
                Debug.Log($"Lobby: {lobby.AvailableSlots}");
                Debug.Log($"Lobby: {lobby.HasPassword}");
                Debug.Log($"Lobby: {lobby.LobbyCode}");
                Debug.Log($"Lobby: {lobby.MaxPlayers}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion


    #region Hosting
    private const float HeartbeatInterval = 15f;
    private float _lastHeartBeatTime;

    private void Update()
    {
        if (CurrentLobby == null || Time.time - _lastHeartBeatTime < HeartbeatInterval)
            return;

        _lastHeartBeatTime = Time.time;
        TryHeartbeatLobby().Forget();
    }

    private void OnDestroy()
    {
        if (CurrentLobby == null)
            return;

        TryDeleteLobby().Forget();
    }

    public async UniTask<Lobby> CreateLobbyWithPassword(int maxPlayers, string lobbyName, string password = null)
    {
        // 비밀번호 유효성 검사 추가
        if (password != null && password.Length < 8)
        {
            Managers.UI.ShowPopupMessage("Password must be at least 8 characters long");
            return null;
        }
        
        return await CreateLobby(maxPlayers, lobbyName, password);
    }

    private async UniTask<Lobby> CreateLobby(int maxPlayers, string lobbyName = null, string password = null, Dictionary<string, string> publicData = null,
     Dictionary<string, string> privateData = null, Dictionary<string, string> memberData = null)
    {
        if (CurrentLobby != null)
            await TryDeleteLobby();

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

            _lastHeartBeatTime = Time.time;

            Debug.Log($"Lobby Created: {CurrentLobby.Id}", this);

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

    private async UniTask<bool> TryHeartbeatLobby()
    {
        if (CurrentLobby == null)
            return false;

        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            Debug.Log("Heartbeat Sent", this);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to send heartbeat: {e.Message}", this);
            return false;
        }
    }

    private async UniTask<bool> TryLockLobby(bool lockstate)
    {
        if (CurrentLobby == null)
            return false;

        try
        {
            var option = new UpdateLobbyOptions
            {
                IsLocked = lockstate
            };

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, option);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to heartbeat lobby: {e.Message}", this);
            return false;
        }
    }

    #endregion

}
