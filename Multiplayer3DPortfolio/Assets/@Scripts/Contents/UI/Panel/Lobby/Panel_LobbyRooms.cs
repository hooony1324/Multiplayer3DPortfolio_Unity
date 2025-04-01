using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyRooms : UI_Base
{
    [SerializeField] private SlotItem_LobbyRoom _roomSlot;

    enum GameObjects
    {
        Content,
    }

    private Transform _content;

    protected override void OnInit()
    {
        Bind<GameObject>(typeof(GameObjects));

        _content = Get<GameObject>(GameObjects.Content).transform;

        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

        

        Managers.Lobby.LobbyList.CurrentLobbies.Subscribe(OnLobbyListUpdate);
    }

    void OnEnable()
    {
        Managers.Lobby.RefreshLobbies();
    }
    
    private void OnLobbyListUpdate(Dictionary<string, LocalLobby> lobbies)
    {
        Debug.Log("OnLobbyListUpdate: " + lobbies.Count);

        foreach (var lobby in lobbies)
        {
            var slot = Instantiate(_roomSlot, _content);
            slot.SetData(lobby.Value);
        }

    }
}
