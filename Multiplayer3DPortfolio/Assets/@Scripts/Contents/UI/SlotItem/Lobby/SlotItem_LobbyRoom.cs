using TMPro;
using UnityEngine;

public class SlotItem_LobbyRoom : UI_SlotItem
{
    enum TMP_Texts
    {
        RoomInfoText,
    }
    protected override void OnInit()
    {
        base.OnInit();

        Bind<TMP_Text>(typeof(TMP_Texts));

        BindEvent(gameObject, OnClickRoom);
    }

    public void SetData(LocalLobby lobby)
    {
        Get<TMP_Text>(TMP_Texts.RoomInfoText).text = 
            $"{lobby.LobbyName.Value}    {lobby.HostID.Value}    ({lobby.PlayerCount}/{lobby.MaxPlayerCount.Value})";
    }

    void OnClickRoom()
    {
        Debug.Log("OnClickButton");
    }

}
