using UnityEngine;
using Unity.Services.Core;
using System;
public class Managers : MonoBehaviour
{
    private static Managers _instance;
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

    private GameManager _gameManager;
    private InputManager _inputManager;
    private AStarManager _aStarManager;
    private LobbyManager _lobbyManager;
    private AuthManager _authManager;
    private SceneManagerEx _sceneManagerEx;

    
    private UIManager _uiManager;

    public static InputManager Input => Instance._inputManager;
    public static AStarManager AStar => Instance._aStarManager;
    public static LobbyManager Lobby => Instance._lobbyManager;
    public static AuthManager Auth => Instance._authManager;
    public static SceneManagerEx Scene => Instance._sceneManagerEx;
    public static UIManager UI => Instance._uiManager;
    public static GameManager Game => Instance._gameManager;
    async void Awake()
    {
        _inputManager = transform.FindChild<InputManager>("InputManager");
        _aStarManager = transform.FindChild<AStarManager>("AStarManager");
        _lobbyManager = transform.FindChild<LobbyManager>("LobbyManager");
        _authManager = transform.FindChild<AuthManager>("AuthManager");
        _sceneManagerEx = transform.FindChild<SceneManagerEx>("SceneManagerEx");
        _gameManager = transform.FindChild<GameManager>("GameManager");
        
        _uiManager = transform.FindChild<UIManager>("UIManager");

        try
        {
            await UnityServices.InitializeAsync();

            Debug.Log("UnityServices.InitializeAsync()");
            
            _authManager.SetupEvents();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        DontDestroyOnLoad(gameObject);
    }

}
