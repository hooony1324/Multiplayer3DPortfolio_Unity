using UnityEngine;

public class Managers : MonoBehaviour
{

    private static Managers _instance;
    private bool _isInitialized = false;
    public static Managers Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<Managers>();
                if (_instance == null)
                {
                    _instance = new GameObject("@Managers").AddComponent<Managers>();
                }
            }
            return _instance;
        }
    }
    
    private InputManager _inputManager;


    public static InputManager InputManager => Instance._inputManager;


    private void Awake()
    {
        if (_isInitialized)
            return;

        InitManagers();
    }

    private void InitManagers()
    {
        _inputManager = transform.FindComponentInChildren<InputManager>("InputManager");
        _inputManager?.Init();


        _isInitialized = true;
    }

}
