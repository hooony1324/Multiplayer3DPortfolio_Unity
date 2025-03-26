using UnityEngine;

public class RateLimitVisibility : MonoBehaviour
{
    private UI_Base _targetUI;
    private LobbyManager.RequestType _requestType;

    public void SetInfo(GameObject targetObject, LobbyManager.RequestType requestType)
    {
        _targetUI = targetObject.GetOrAddComponent<UI_Base>();
        _requestType = requestType;

        Managers.Lobby.GetRateLimit(_requestType).OnCooldownStateChanged += UpdateVisibility;
    }

    private void OnDestroy()
    {
        Managers.Lobby.GetRateLimit(_requestType).OnCooldownStateChanged -= UpdateVisibility;
    }

    private void UpdateVisibility(bool isCoolingDown)
    {
        if (isCoolingDown)
            _targetUI.Hide();
        else
            _targetUI.Show();
    }
}
