// 확장 메서드 방식
using System;
using UnityEngine;

public static class EventBusExtensions
{
    public static void SubscribeEvent<T>(this MonoBehaviour component, Action<T> handler) 
        where T : struct, IEventData
    {
        var subscription = EventBus.Subscribe<T>(handler);
        component.StartCoroutine(AutoDisposeWhenDestroyed(component, subscription));
    }
    
    private static System.Collections.IEnumerator AutoDisposeWhenDestroyed(
        MonoBehaviour component, IDisposable subscription)
    {
        yield return new WaitUntil(() => component == null);
        subscription.Dispose();
    }
}