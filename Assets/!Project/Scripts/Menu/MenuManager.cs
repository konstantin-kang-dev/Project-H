using DG.Tweening;
using FishNet.Connection;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Inject] MenuWindowNavigator _menuPageNavigator;

    [SerializeField] MainMenuUI _menuUI;
    [SerializeField] LobbyUI _lobbyUI;
    [SerializeField] LobbiesOverviewUI _lobbiesOverviewUI;

    [SerializeField] TMP_InputField _playerNameInput;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        SaveManager.LoadAll();

        _playerNameInput.text = SaveManager.GameData.PlayerName;

        _menuUI.Init();
        _lobbiesOverviewUI.Init();

        BindNetworkEvents();
        _menuPageNavigator.OnWindowOpened += HandleMenuWindowOpen;

        GameDifficultyManager.Instance.Init();
    }

    public void BindNetworkEvents()
    {
        Debug.Log($"[MenuManager] BindNetworkEvents");
        NetworkGameManager.Instance.OnLocalClientConnected += HandleJoinLobby;
        NetworkGameManager.Instance.OnLocalClientDisconnected += HandleQuitLobby;

        LobbyManager.OnReady += HandleLobbyManagerReady;
    }

    void HandleLobbyManagerReady()
    {
        LobbyManager.Instance.OnGameStarted += HandleStartGame;

    }

    void HandleJoinLobby()
    {
        _menuPageNavigator.OpenWindow(MenuWindowType.Lobby);
        LoadingManager.Instance.SetLoadingProgress(1f);

        Debug.Log($"[MenuManager] Joined lobby!");
    }

    void HandleQuitLobby()
    {
        NetworkGameManager.Instance.Disconnect();
        MenuWindowNavigator.Instance.OpenWindow(MenuWindowType.MainMenu);
        LobbyManager.Instance.OnGameStarted -= HandleStartGame;
        Debug.Log($"[MenuManager] Quit lobby");
    }

    void HandleStartGame()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "");
        Debug.Log($"[MenuManager] Started game!");
    }

    void HandleMenuWindowOpen(MenuWindowType windowType)
    {
        Vector3 startRot = Camera.main.transform.eulerAngles;

        switch (windowType)
        {
            case MenuWindowType.MainMenu:
                Camera.main.transform.DORotate(new Vector3(startRot.x, 0f, startRot.z), 1.5f).SetUpdate(UpdateType.Late).SetEase(Ease.OutQuart);
                break;
            case MenuWindowType.Lobby:
                Camera.main.transform.DORotate(new Vector3(startRot.x, 180f, startRot.z), 3.5f).SetUpdate(UpdateType.Late).SetEase(Ease.InOutSine);
                break;
            default:
                break;
        }

    }

    void Update()
    {
        
    }

    private void OnDestroy()
    {

        _menuPageNavigator.OnWindowOpened -= HandleMenuWindowOpen;

        NetworkGameManager.Instance.OnLocalClientConnected -= HandleJoinLobby;
        NetworkGameManager.Instance.OnLocalClientDisconnected -= HandleQuitLobby;
        
        if(LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnGameStarted -= HandleStartGame;
        }
    }
}
