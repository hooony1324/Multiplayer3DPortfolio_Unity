using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventData {}

public static class EventBus
{
    // 이벤트 타입별 Subject 저장소
    private static readonly Dictionary<Type, object> eventStreams = new Dictionary<Type, object>();
    
    // 각 MonoBehaviour의 구독 목록 저장
    private static readonly Dictionary<int, List<IDisposable>> componentSubscriptions = 
        new Dictionary<int, List<IDisposable>>();

    public static Subject<T> GetEvent<T>() where T : struct, IEventData
    {
        Type eventType = typeof(T);
        
        if (!eventStreams.TryGetValue(eventType, out var subject))
        {
            subject = new Subject<T>();
            eventStreams[eventType] = subject;
        }
        
        return (Subject<T>)subject;
    }

    public static void Publish<T>(T eventData) where T : struct, IEventData
    {
        GetEvent<T>().OnNext(eventData);
    }

    public static IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEventData
    {
        return GetEvent<T>().Subscribe(handler);
    }
    
    /// <summary>
    /// MonoBehaviour에 자동으로 연결된 이벤트 구독
    /// </summary>
    public static void Subscribe<T>(MonoBehaviour component, Action<T> handler) where T : struct, IEventData
    {
        if (component == null) return;
        
        // 구독 생성
        var subscription = Subscribe<T>(handler);
        
        // 구독 목록 가져오기
        int instanceId = component.GetInstanceID();
        if (!componentSubscriptions.TryGetValue(instanceId, out var subscriptions))
        {
            subscriptions = new List<IDisposable>();
            componentSubscriptions[instanceId] = subscriptions;
            
            // 자동 해제를 위한 헬퍼 생성 (별도 클래스로 분리됨)
            var helper = component.gameObject.AddComponent<EventAutoUnsubscriber>();
            helper.Initialize(instanceId);
            helper.hideFlags = HideFlags.HideInInspector;
        }
        
        // 구독 목록에 추가
        subscriptions.Add(subscription);
    }
    
    /// <summary>
    /// 특정 컴포넌트에 연결된 모든 구독 해제
    /// </summary>
    public static void Unsubscribe(int instanceId)
    {
        if (componentSubscriptions.TryGetValue(instanceId, out var subscriptions))
        {
            foreach (var subscription in subscriptions)
            {
                subscription.Dispose();
            }
            componentSubscriptions.Remove(instanceId);
        }
    }

    /// <summary>
    /// 모든 이벤트 스트림 제거 (예: 씬 전환 시)
    /// </summary>
    public static void Reset()
    {
        foreach (var eventStream in eventStreams.Values)
        {
            if (eventStream is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        eventStreams.Clear();
        
        // 모든 구독 해제
        foreach (var subscriptionList in componentSubscriptions.Values)
        {
            foreach (var subscription in subscriptionList)
            {
                subscription.Dispose();
            }
        }
        componentSubscriptions.Clear();
    }
}