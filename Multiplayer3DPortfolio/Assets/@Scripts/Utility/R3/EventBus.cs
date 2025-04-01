using R3;
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, object> _subjects = new();

    public static void Subscribe<T>(Action<T> action)
    {
        var subject = GetOrCreateSubject<T>();
        subject.Subscribe(action);
    }

    public static void Publish<T>(T message)
    {
        if (_subjects.TryGetValue(typeof(T), out var subject))
        {
            ((Subject<T>)subject).OnNext(message);
        }
    }

    private static Subject<T> GetOrCreateSubject<T>()
    {
        var type = typeof(T);
        if (!_subjects.TryGetValue(type, out var subject))
        {
            subject = new Subject<T>();
            _subjects[type] = subject;
        }
        return (Subject<T>)subject;
    }
}