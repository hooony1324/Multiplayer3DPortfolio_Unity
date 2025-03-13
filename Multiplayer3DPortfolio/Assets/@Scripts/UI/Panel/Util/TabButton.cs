using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour
{
    private Button button;
    private Image image;
    private int tabIndex;
    private bool isSelected = false;
    
    // 원래 색상 저장용
    private Color originalColor;
    // 어둡게 만들 비율 (0.8 = 80% 밝기)
    private const float DESELECT_DARKNESS = 0.8f;

    // 탭이 클릭됐을 때 실행할 액션
    public Action OnTabClicked { get; set; }

    private void Awake()
    {
        // 컴포넌트 참조 가져오기
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        
        // 컴포넌트 검증
        if (button == null)
        {
            Debug.LogError($"[{gameObject.name}] Button 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        
        if (image == null)
        {
            Debug.LogError($"[{gameObject.name}] Image 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        
        // 원래 색상 저장
        originalColor = image.color;
        
        // 클릭 이벤트 연결
        button.onClick.AddListener(() => {
            OnTabClicked?.Invoke();
        });
    }

    public void SetTabIndex(int index)
    {
        tabIndex = index;
    }

    public void SelectTab()
    {
        if (isSelected) return;
        
        isSelected = true;
        if (image != null)
        {
            // 원래 색상으로 복원
            image.color = originalColor;
        }
    }

    public void DeselectTab()
    {
        if (!isSelected) return;
        
        isSelected = false;
        if (image != null)
        {
            // 원래 색상보다 어둡게 설정
            image.color = new Color(
                originalColor.r * DESELECT_DARKNESS,
                originalColor.g * DESELECT_DARKNESS,
                originalColor.b * DESELECT_DARKNESS,
                originalColor.a
            );
        }
    }
} 