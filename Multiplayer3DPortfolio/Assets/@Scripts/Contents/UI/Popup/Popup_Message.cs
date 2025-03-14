using TMPro;
using UnityEngine;

public class Popup_Message : UI_Base
{
    private GameObject _screenBlocker;
    private TextMeshProUGUI _messageText;

    protected override void OnInit()
    {
        _screenBlocker = Util.FindChild(gameObject, "ScreenBlocker");
        GameObject messageText = Util.FindChild(gameObject, "MessageText", true);
        _messageText = messageText.GetComponent<TextMeshProUGUI>();

        BindEvent(_screenBlocker, OnClickScreenBlocker);

    }

    public void SetInfo(string message)
    {
        _messageText.text = message;
    }

    public void OnClickScreenBlocker()
    {
        gameObject.SetActive(false);
    }
}
