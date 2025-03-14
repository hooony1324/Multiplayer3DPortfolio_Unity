using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 버튼 클릭 시 애니메이션을 적용하는 컴포넌트
/// </summary>
[RequireComponent(typeof(UI_EventHandler))]
public class ButtonAnimator : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float scaleAmount = 0.9f;
    [SerializeField] private float darkenAmount = 0.8f;
    [SerializeField] private Ease easeType = Ease.OutQuad;
    
    
    private RectTransform targetRect;
    private Image targetImage;
    
    // 원본 값
    private Vector3 originalScale;
    private Color originalColor;
    
    // 현재 실행 중인 트윈
    private Tween scaleTween;
    private Tween colorTween;
    
    private UI_EventHandler eventHandler;
    private bool isInitialized = false;
    
    private void Awake()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        if (isInitialized) return;
        
        // 컴포넌트 참조 가져오기
        eventHandler = GetComponent<UI_EventHandler>();
        targetRect = GetComponent<RectTransform>();
        targetImage = GetComponent<Image>();
        
        // 원본 값 저장
        originalScale = targetRect.localScale;
        originalColor = targetImage.color;
            
        isInitialized = true;
    }
    
    private void OnEnable()
    {
        Initialize();
        
        // 이벤트 구독
        if (eventHandler != null)
        {
            eventHandler.OnPointerDownHandler += OnButtonDown;
            eventHandler.OnPointerUpHandler += OnButtonUp;
        }
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (eventHandler != null)
        {
            eventHandler.OnPointerDownHandler -= OnButtonDown;
            eventHandler.OnPointerUpHandler -= OnButtonUp;
        }
        
        // 실행 중인 트윈 정리
        KillAllTweens();
    }

    // 버튼을 눌렀을 때
    private void OnButtonDown()
    {
        // 애니메이션 시작을 프레임 끝으로 지연
        if (!isInitialized) return;
        
        // 실행 중인 트윈 중지
        KillAllTweens();
        
        // 트윈 실행을 다음 프레임으로 지연
        StartCoroutine(DelayedAnimation(true));
    }
    
    // 버튼에서 손을 뗐을 때
    private void OnButtonUp()
    {
        if (!isInitialized) return;
        
        // 실행 중인 트윈 중지
        KillAllTweens();
        
        // 트윈 실행을 다음 프레임으로 지연
        StartCoroutine(DelayedAnimation(false));
    }
    
    private System.Collections.IEnumerator DelayedAnimation(bool isDown)
    {
        // 한 프레임 대기
        yield return null;
        
        if (isDown)
        {
            // 1. 버튼 크기 축소
            if (targetRect != null)
            {
                scaleTween = targetRect.DOScale(originalScale * scaleAmount, duration)
                    .SetEase(easeType)
                    .SetUpdate(true); // 타임스케일 무시 (옵션)
            }
            
            // 2. 버튼 색상 어둡게
            if (targetImage != null)
            {
                Color darkerColor = new Color(
                    originalColor.r * darkenAmount,
                    originalColor.g * darkenAmount,
                    originalColor.b * darkenAmount,
                    originalColor.a
                );
                
                colorTween = DOTween.To(() => targetImage.color, 
                    x => targetImage.color = x, 
                    darkerColor, 
                    duration)
                    .SetEase(easeType)
                    .SetUpdate(true); // 타임스케일 무시 (옵션)
            }
        }
        else
        {
            // 1. 버튼 크기 복원
            if (targetRect != null)
            {
                scaleTween = targetRect.DOScale(originalScale, duration)
                    .SetEase(easeType)
                    .SetUpdate(true);
            }
            
            // 2. 버튼 색상 복원
            if (targetImage != null)
            {
                colorTween = DOTween.To(() => targetImage.color, 
                    x => targetImage.color = x, 
                    originalColor, 
                    duration)
                    .SetEase(easeType)
                    .SetUpdate(true);
            }
        }
    }
    
    // 모든 트윈 중지
    private void KillAllTweens()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
            scaleTween = null;
        }
        
        if (colorTween != null && colorTween.IsActive())
        {
            colorTween.Kill();
            colorTween = null;
        }
    }
    
    private void OnDestroy()
    {
        KillAllTweens();
    }
}
