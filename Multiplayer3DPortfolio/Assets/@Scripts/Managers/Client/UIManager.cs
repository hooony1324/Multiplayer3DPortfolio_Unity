using UnityEngine;

public class UIManager : MonoBehaviour
{
    // R3 이벤트 기반 통신 알아보기
    public void ShowPopupMessage(string message)
    {
        EventBus.Publish(new PopupMessageEvent(message));
    }
}


public struct PopupMessageEvent : IEventData
{
    public string Message { get; }
    public PopupMessageEvent(string message)
    {
        Message = message;
    }
}
