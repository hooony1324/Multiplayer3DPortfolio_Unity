using System;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class RateLimiter
{
    public event Action<bool> OnCooldownStateChanged;
    private readonly int _cooldownMS;
    private readonly int _maxRequests;
    private int _remainingRequests;
    private bool _isCoolingDown;

    public bool TaskQueued { get; private set; } = false;

    public bool IsCoolingDown
    {
        get => _isCoolingDown;
        private set
        {
            if (_isCoolingDown != value)
            {
                _isCoolingDown = value;
                OnCooldownStateChanged?.Invoke(value);
            }
        }
    }

    public RateLimiter(int maxRequests, float cooldownSeconds, int pingBuffer = 100)
    {
        _maxRequests = maxRequests;
        _remainingRequests = maxRequests;
        _cooldownMS = Mathf.CeilToInt(cooldownSeconds * 1000) + pingBuffer;
    }


    public async UniTask QueueUntilCooldown()
    {
        // 요청 가능 횟수 감소
        _remainingRequests--;
        
        // 요청 횟수가 소진되었고 아직 쿨다운 중이 아니면 쿨다운 시작
        if (_remainingRequests <= 0 && !IsCoolingDown)
        {
            IsCoolingDown = true;
            TaskQueued = true;
            
            // 쿨다운 시간만큼 대기
            await UniTask.Delay(_cooldownMS);
            
            // 쿨다운 완료 후 요청 횟수 초기화
            _remainingRequests = _maxRequests;
            IsCoolingDown = false;
            TaskQueued = false;
            
            return;
        }
        
        // 이미 쿨다운 중이고 요청 횟수가 소진되었으면 쿨다운이 끝날 때까지 대기
        if (IsCoolingDown && _remainingRequests <= 0)
        {
            while (IsCoolingDown)
            {
                await UniTask.Delay(10);
            }
        }
    }
}

public static class RateLimiterExtensions
{
    public static void SetRateLimited(this GameObject gameObject, RateLimiter rateLimiter)
    {
        var visibility = gameObject.GetOrAddComponent<RateLimitVisibility>();
        visibility.SetInfo(rateLimiter);
    }
}
