using R3;
using UnityEngine;

public class UIContainer : InitializeBase
{
    private Popup_Message _popupMessage;
    protected override void OnInit()
    {
        _popupMessage = transform.FindChild<Popup_Message>("Popup_Message");
        if (_popupMessage != null)
            EventBus.Subscribe<PopupMessageEvent>(ShowPopupMessage);
    }


    private void ShowPopupMessage(PopupMessageEvent eventData)
    {
        if (_popupMessage == null)
            return;
        
        Debug.Log("ShowPopupMessage: " + eventData.Message);
        _popupMessage.gameObject.SetActive(true);
        _popupMessage.SetInfo(eventData.Message);
        
    }

    void OnDestroy()
    {
        EventBus.Reset();
    }

}
