using UnityEngine;
using R3;

public readonly struct PopupMessageEvent
{
    public readonly string Message;
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
