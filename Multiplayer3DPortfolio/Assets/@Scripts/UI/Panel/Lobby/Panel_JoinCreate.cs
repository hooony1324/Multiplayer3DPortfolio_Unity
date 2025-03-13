using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Panel_JoinCreate : UI_Base
{
    enum Buttons
    {
        Btn_Create,
        Btn_Join,
        Btn_QuickJoin,
        Btn_Refresh,
    }
    
    protected override void OnInit()
    {
        base.OnInit();

        Bind<Button>(typeof(Buttons));


    }

    void OnClickCreate()
    {
        Managers.Lobby.CreateLobbyWithCode(10, "123456").Forget();
    }

    void OnClickJoin()
    {
        Debug.Log("OnClickJoin");
    }

    void OnClickQuickJoin()
    {
        Managers.Lobby.QuickJoinLobby().Forget();
    }

    void OnClickRefresh()
    {
        Debug.Log("OnClickRefresh");
    }
}
