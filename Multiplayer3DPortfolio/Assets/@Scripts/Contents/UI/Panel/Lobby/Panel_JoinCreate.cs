using UnityEngine;
using UnityEngine.UI;

public class Panel_JoinCreate : UI_Base
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

        Bind<Button>(typeof(Buttons));


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
        Debug.Log("OnClickRefresh");
    }
}
