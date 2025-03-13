using UnityEngine;
using UnityEngine.UI;

public class Panel_LobbyRooms : UI_Base
{
    [SerializeField] private GameObject _roomSlotPrefab;

    enum GameObjects
    {
        Content,
    }

    private Transform _content;

    protected override void OnInit()
    {
        base.OnInit();

        Bind<GameObject>(typeof(GameObjects));

        _content = Get<GameObject>(GameObjects.Content).transform;

        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(_roomSlotPrefab, _content);
            //
        }
    }
}
