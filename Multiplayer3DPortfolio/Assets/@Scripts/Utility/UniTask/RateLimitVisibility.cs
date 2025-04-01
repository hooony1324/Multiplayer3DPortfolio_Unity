using UnityEngine;

public class RateLimitVisibility : MonoBehaviour
{
    private UI_Base _targetUI;

    private RateLimiter _rateLimiter;
    public void SetInfo(RateLimiter rateLimiter)
    {
        _targetUI = gameObject.GetOrAddComponent<UI_Base>();

        _rateLimiter = rateLimiter;
        _rateLimiter.OnCooldownStateChanged -= UpdateVisibility;
        _rateLimiter.OnCooldownStateChanged += UpdateVisibility;
    }

    private void OnDestroy()
    {
        _rateLimiter.OnCooldownStateChanged -= UpdateVisibility;
    }

    private void UpdateVisibility(bool isCoolingDown)
    {
        if (isCoolingDown)
            _targetUI.Hide();
        else
            _targetUI.Show();
    }
}
