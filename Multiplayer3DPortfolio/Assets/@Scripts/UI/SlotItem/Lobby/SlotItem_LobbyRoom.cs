using UnityEngine;

public class SlotItem_LobbyRoom : UI_SlotItem
{
    protected override void OnInit()
    {
        base.OnInit();
        gameObject.BindEvent(OnClickRoom);
    }

    void OnClickRoom()
    {
        Debug.Log("OnClickButton");
    }

}
