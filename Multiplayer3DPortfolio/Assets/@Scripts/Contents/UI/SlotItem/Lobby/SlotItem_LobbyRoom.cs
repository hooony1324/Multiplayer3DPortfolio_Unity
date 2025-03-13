using UnityEngine;

public class SlotItem_LobbyRoom : UI_SlotItem
{
    protected override void OnInit()
    {
        base.OnInit();
        BindEvent(gameObject, OnClickRoom);
    }

    void OnClickRoom()
    {
        Debug.Log("OnClickButton");
    }

}
