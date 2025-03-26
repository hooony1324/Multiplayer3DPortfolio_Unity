using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
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

        BindEvent(Get<GameObject>(GameObjects.Button_CreateLobby), OnClickCreateLobby);
    }

    void OnClickCreateLobby()
    {
        string lobbyName = Get<TMP_InputField>(TMP_InputFields.InputField_LobbyName).text;
        string lobbyPassword = Get<TMP_InputField>(TMP_InputFields.InputField_LobbyPassword).text;

        // Create Lobby
        if (string.IsNullOrEmpty(lobbyPassword))
            Managers.Lobby.CreateLobbyWithPassword(4, lobbyName);
        else
            Managers.Lobby.CreateLobbyWithPassword(4, lobbyName, lobbyPassword);
    }
}
