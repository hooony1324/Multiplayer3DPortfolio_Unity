using TMPro;
using UnityEngine;

public class Popup_Message : UI_Base
{
    private GameObject _screenBlocker;
    private TextMeshProUGUI _messageText;

    protected override void OnInit()
    {
        _screenBlocker = transform.FindChild<GameObject>("ScreenBlocker");
        _messageText = transform.FindChild<TextMeshProUGUI>("MessageText");

        BindEvent(_screenBlocker, OnClickScreenBlocker);

    }

    public void SetInfo(string message)
    {
        _messageText.text = message;
    }

    private void OnClickScreenBlocker()
    {
        gameObject.SetActive(false);
    }
}
