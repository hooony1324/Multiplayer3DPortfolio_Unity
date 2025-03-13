using System.Collections.Generic;
using UnityEngine;

public class Panel_Tabs : UI_Base
{
    enum GameObjects
    {
        Panel_TabButtons,
        Panel_TabPages,
    }

    private Transform tabButtonsPanel;
    private Transform tabPagesPanel;
    
    private List<TabButton> tabButtons = new List<TabButton>();
    private List<GameObject> tabPages = new List<GameObject>();
    
    private int currentTabIndex = -1;
    private bool isProcessingTabSelection = false;

    protected override void OnInit()
    {
        base.OnInit();

        Bind<GameObject>(typeof(GameObjects));

        // 계층 구조에서 패널들 찾기
        tabButtonsPanel = Get<GameObject>(GameObjects.Panel_TabButtons).transform;
        tabPagesPanel = Get<GameObject>(GameObjects.Panel_TabPages).transform;
        
        if (tabButtonsPanel == null)
        {
            Debug.LogError("Panel_TabButtons를 찾을 수 없습니다. 계층 구조를 확인하세요.");
            return;
        }
        
        if (tabPagesPanel == null)
        {
            Debug.LogError("Panel_TabPages를 찾을 수 없습니다. 계층 구조를 확인하세요.");
            return;
        }
        
        // 버튼 패널에서 모든 TabButton 컴포넌트 가져오기
        tabButtons.AddRange(tabButtonsPanel.GetComponentsInChildren<TabButton>());
        
        // 페이지 패널의 모든 자식 게임오브젝트 가져오기
        for (int i = 0; i < tabPagesPanel.childCount; i++)
        {
            tabPages.Add(tabPagesPanel.GetChild(i).gameObject);
        }
        
        InitializeTabs();
    }

    private void Start()
    {
        // 모든 페이지 초기 비활성화
        foreach (var page in tabPages)
        {
            page.SetActive(false);
        }
        
        // 첫 번째 탭이 이미 선택되어 있으면 해당 페이지 활성화
        if (currentTabIndex >= 0 && currentTabIndex < tabPages.Count)
        {
            tabPages[currentTabIndex].SetActive(true);
        }
    }

    private void InitializeTabs()
    {
        if (tabButtons.Count == 0)
        {
            Debug.LogWarning("탭 버튼이 없습니다. Panel_TabButtons 하위에 TabButton 컴포넌트가 있는지 확인하세요.");
            return;
        }

        if (tabPages.Count == 0)
        {
            Debug.LogWarning("탭 페이지가 없습니다. Panel_TabPages 하위에 페이지 오브젝트가 있는지 확인하세요.");
            return;
        }
        
        // 탭 버튼 수와 페이지 수가 일치하는지 확인
        if (tabButtons.Count != tabPages.Count)
        {
            Debug.LogWarning($"탭 버튼 수({tabButtons.Count})와 페이지 수({tabPages.Count})가 일치하지 않습니다.");
        }

        // 모든 탭 버튼 초기화
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i; // 클로저 문제 방지를 위한 지역 변수
            tabButtons[i].SetTabIndex(i);
            
            // 클릭 이벤트 등록
            tabButtons[i].OnTabClicked = () => {
                if (!isProcessingTabSelection)
                {
                    SwitchToTab(index);
                }
            };
        }

        // 첫 번째 탭 자동 선택 (페이지가 있는 경우)
        if (tabButtons.Count > 0 && tabPages.Count > 0)
        {
            SwitchToTab(0);
        }
    }

    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= Mathf.Min(tabButtons.Count, tabPages.Count))
            return;
            
        if (currentTabIndex == tabIndex)
            return;  // 이미 선택된 탭이면 아무것도 하지 않음
            
        isProcessingTabSelection = true;

        try
        {
            // 현재 활성화된 페이지 비활성화
            if (currentTabIndex >= 0 && currentTabIndex < tabPages.Count)
            {
                tabPages[currentTabIndex].SetActive(false);
            }

            // 새 페이지 활성화
            tabPages[tabIndex].SetActive(true);
            
            // 모든 탭 선택 해제
            foreach (TabButton button in tabButtons)
            {
                button.DeselectTab();
            }

            // 선택된 탭 활성화
            tabButtons[tabIndex].SelectTab();
            
            // 현재 인덱스 업데이트
            currentTabIndex = tabIndex;
        }
        finally
        {
            isProcessingTabSelection = false;
        }
    }
}
