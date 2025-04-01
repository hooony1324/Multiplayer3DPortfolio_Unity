using System;
using R3;
using UnityEngine;

public abstract class ReactiveMonoBehaviour : MonoBehaviour
{
    private CompositeDisposable _disposables;
    protected CompositeDisposable Disposables => _disposables ??= new CompositeDisposable();

    protected virtual void OnDestroy()
    {
        _disposables?.Dispose();
        _disposables = null;
    }

    // 자동 구독을 위한 메서드
    protected IDisposable Subscribe<T>(Observable<T> observable, Action<T> onNext)
    {
        var disposable = observable.Subscribe(onNext);
        Disposables.Add(disposable);
        return disposable;
    }
}