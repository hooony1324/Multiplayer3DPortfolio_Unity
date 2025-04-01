using TMPro;
using UnityEngine;
using static Define;
public class Panel_CreateLobby : UI_Base
{
    enum TMP_InputFields
    {
        InputField_LobbyName,
        InputField_LobbyPassword,
    }

    enum GameObjects
    {
        Button_CreateLobby,
    }


    protected override void OnInit()
    {
        Bind<TMP_InputField>(typeof(TMP_InputFields));
        Bind<GameObject>(typeof(GameObjects));


        GameObject createLobbyButton = Get<GameObject>(GameObjects.Button_CreateLobby);
        RateLimiter rateLimiter = Managers.Lobby.GetRateLimiter(ERequestType.Query);
        createLobbyButton.SetRateLimited(rateLimiter);
        BindEvent(createLobbyButton, OnClickCreateLobby);
    }

    void OnClickCreateLobby()
    {
        string lobbyName = Get<TMP_InputField>(TMP_InputFields.InputField_LobbyName).text;
        string lobbyPassword = Get<TMP_InputField>(TMP_InputFields.InputField_LobbyPassword).text;

        Managers.Lobby.CreateLobby(lobbyName, false, null, 4);
    }
}
