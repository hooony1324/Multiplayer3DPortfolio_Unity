using UnityEngine;

public struct PopupMessageEvent : IEventData
{
    public string Message { get; }
    public PopupMessageEvent(string message)
    {
        Message = message;
    }
}


public class UIManager : MonoBehaviour
{
    public void ShowPopupMessage(string message)
    {
        EventBus.Publish(new PopupMessageEvent(message));
    }
}
