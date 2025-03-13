using UnityEngine;

public abstract class InitializeBase : MonoBehaviour
{
    private bool _isInitialized = false;

    protected virtual void Awake()
    {
        InitOnce();
    }

    protected void InitOnce()
    {
        if (_isInitialized)
            return;

        OnInit();
        _isInitialized = true;
    }

    protected abstract void OnInit();
}
