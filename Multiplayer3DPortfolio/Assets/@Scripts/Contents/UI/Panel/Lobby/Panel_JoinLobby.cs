using UnityEngine;
using UnityEngine.UI;
using static Define;

public class Panel_JoinLobby : UI_Base
{
    enum Buttons
    {
        Btn_Join,
        Btn_QuickJoin,
        Btn_Refresh,
    }
    
    protected override void OnInit()
    {
        base.OnInit();

        Bind<GameObject>(typeof(Buttons));

        //Get<GameObject>(Buttons.Btn_Join).onClick.AddListener(OnClickJoin);
        //Get<GameObject>(Buttons.Btn_QuickJoin).onClick.AddListener(OnClickQuickJoin);
        GameObject refreshButton = Get<GameObject>(Buttons.Btn_Refresh);
        BindEvent(refreshButton, OnClickRefresh);

        RateLimiter rateLimiter = Managers.Lobby.GetRateLimiter(ERequestType.Query);
        refreshButton.SetRateLimited(rateLimiter);
    }

    void OnClickJoin()
    {
        Debug.Log("OnClickJoin");
    }

    void OnClickQuickJoin()
    {
        
    }

    void OnClickRefresh()
    {
        Managers.Lobby.RefreshLobbies();
    }
}
