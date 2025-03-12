using UnityEngine;
using UnityEngine.UI;

public class UI_Login : UI_Base
{
    enum Buttons
    {
        Button_Login_GameCenter,
        Button_Login_Facebook,
        Button_Login_Google,


        Button_Login_Guest,
    }

    void Start()
    {
        Bind<Button>(typeof(Buttons));

        Get<Button>(Buttons.Button_Login_Guest).onClick.AddListener(OnClickLogin_Guest);
    }

    void OnClickLogin_GameCenter()
    {
        Debug.Log("OnClickLogin_GameCenter");
    }

    void OnClickLogin_Guest()
    {
        Managers.Auth.SignUpAnonymously();
    }
}