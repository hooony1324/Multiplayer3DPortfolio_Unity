
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public static class LobbyConverters
{
    const string key_RelayCode = nameof(LocalLobby.RelayCode);
    const string key_LobbyState = nameof(LocalLobby.LobbyState);
    const string key_LastUpdated = nameof(LocalLobby.LastUpdated);
    const string key_DisplayName = nameof(LocalPlayer.DisplayName);
    const string key_UsersStatus = nameof(LocalPlayer.UserStatus);


    public static Dictionary<string, string> LocalToRemoteLobbyData(LocalLobby lobby)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data[key_RelayCode] = lobby.RelayCode.Value;
        data[key_LobbyState] = lobby.LobbyState.Value.ToString();
        data[key_LastUpdated] = lobby.LastUpdated.Value.ToString();

        return data;
    }

    public static Dictionary<string, string> LocalToRemoteUserData(LocalPlayer player)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data[key_DisplayName] = player.DisplayName.Value;
        data[key_UsersStatus] = player.UserStatus.Value.ToString();

        return data;
    }

    public static bool RemoteToLocal(Lobby remoteLobby, ref LocalLobby localLobby)
    {
        if (remoteLobby == null)
        {
            Debug.LogError("Remote lobby is null, cannot convert.");
            localLobby = null;
            return false;
        }

        localLobby.LobbyID.Value = remoteLobby.Id;
        localLobby.HostID.Value = remoteLobby.HostId;
        localLobby.LobbyName.Value = remoteLobby.Name;
        localLobby.LobbyCode.Value = remoteLobby.LobbyCode;
        localLobby.Private.Value = remoteLobby.IsPrivate;
        localLobby.AvailableSlots.Value = remoteLobby.AvailableSlots;
        localLobby.MaxPlayerCount.Value = remoteLobby.MaxPlayers;
        localLobby.LastUpdated.Value = remoteLobby.LastUpdated.ToFileTimeUtc();

        //Custom Lobby Data Conversions
        localLobby.RelayCode.Value = remoteLobby.Data?.ContainsKey(key_RelayCode) == true
            ? remoteLobby.Data[key_RelayCode].Value
            : localLobby.RelayCode.Value;
        localLobby.LobbyState.Value = remoteLobby.Data?.ContainsKey(key_LobbyState) == true
            ? (ELobbyState)int.Parse(remoteLobby.Data[key_LobbyState].Value)
            : ELobbyState.Lobby;


        //Custom User Data Conversions
        List<string> remotePlayerIDs = new List<string>();
        int index = 0;
        foreach (var player in remoteLobby.Players)
        {
            var id = player.Id;
            remotePlayerIDs.Add(id);
            var isHost = remoteLobby.HostId.Equals(player.Id);
            var displayName = player.Data?.ContainsKey(key_DisplayName) == true
                ? player.Data[key_DisplayName].Value
                : default;
            var userStatus = player.Data?.ContainsKey(key_UsersStatus) == true
                ? (EUserStatus)int.Parse(player.Data[key_UsersStatus].Value)
                : EUserStatus.Lobby;

            LocalPlayer localPlayer = localLobby.GetLocalPlayer(index);

            if (localPlayer == null)
            {
                localPlayer = new LocalPlayer(id, index, isHost, displayName, userStatus);
                localLobby.AddPlayer(index, localPlayer);
            }
            else
            {
                localPlayer.ID.Value = id;
                localPlayer.Index.Value = index;
                localPlayer.IsHost.Value = isHost;
                localPlayer.DisplayName.Value = displayName;
                localPlayer.UserStatus.Value = userStatus;
            }

            index++;
        }

        return true;
    }

    public static List<LocalLobby> QueryToLocalList(QueryResponse query)
    {
        List<LocalLobby> result = new();
        foreach (var lobby in query.Results)
            result.Add(RemoteToNewLocal(lobby));

        return result;
    }

    public static LocalLobby RemoteToNewLocal(Lobby remoteLobby)
    {
        LocalLobby data = new();
        RemoteToLocal(remoteLobby, ref data);
        return data;
    }
}