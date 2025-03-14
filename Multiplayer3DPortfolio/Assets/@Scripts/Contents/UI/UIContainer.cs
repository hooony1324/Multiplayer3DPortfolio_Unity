using R3;
using UnityEngine;

public class UIContainer : InitializeBase
{
    private Popup_Message _popupMessage;
    protected override void OnInit()
    {
        _popupMessage = Util.FindChild(gameObject, "Popup_Message").GetComponent<Popup_Message>();

        EventBus.Subscribe<PopupMessageEvent>(ShowPopupMessage);
    }


    private void ShowPopupMessage(PopupMessageEvent @event)
    {
        _popupMessage.gameObject.SetActive(true);
        _popupMessage.SetInfo(@event.Message);
    }


}
