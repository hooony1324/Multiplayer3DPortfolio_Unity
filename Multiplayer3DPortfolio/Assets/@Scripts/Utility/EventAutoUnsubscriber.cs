using UnityEngine;

/// <summary>
/// 컴포넌트 파괴 시 자동으로 이벤트 구독을 해제하는 헬퍼 컴포넌트
/// </summary>
internal class EventAutoUnsubscriber : MonoBehaviour
{
    private int _instanceId;
    
    public void Initialize(int instanceId)
    {
        _instanceId = instanceId;
    }
    
    private void OnDestroy()
    {
        EventBus.Unsubscribe(_instanceId);
    }
}