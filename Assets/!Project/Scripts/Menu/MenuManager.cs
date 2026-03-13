using Cysharp.Threading.Tasks;
using DG.Tweening;
using FishNet.Connection;
using GameAudio;
using Saves;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Inject] WindowsNavigator _menuPageNavigator;

    [SerializeField] MainMenuUI _menuUI;
    [SerializeField] LobbyUI _lobbyUI;
    [SerializeField] LobbiesOverviewUI _lobbiesOverviewUI;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public async void Init()
    {
        SaveManager.LoadAll();

        _menuUI.Init();
        _lobbiesOverviewUI.Init();

        BindNetworkEvents();
        _menuPageNavigator.OnWindowOpened += HandleMenuWindowOpen;

        GameDifficultyManager.Instance.Init();
        GlobalAudioManager.Instance.Play(SoundType.MenuAmbient);

        WindowsNavigator.Instance.Clear();
        WindowsNavigator.Instance.CloseAll(true);
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.MainMenu);

        await UniTask.WaitForSeconds(1f);
        LoadingManager.Instance.SetLoadingProgress(1f);
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
        _menuPageNavigator.OpenWindow(CustomWindowType.Lobby);
        LoadingManager.Instance.SetLoadingProgress(1f);
        

        Debug.Log($"[MenuManager] Joined lobby!");
    }

    void HandleQuitLobby()
    {
        NetworkGameManager.Instance.Disconnect();
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.MainMenu);
        LobbyManager.Instance.OnGameStarted -= HandleStartGame;
        Debug.Log($"[MenuManager] Quit lobby");
    }

    void HandleStartGame()
    {
        WindowsNavigator.Instance.Clear();
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "");
        Debug.Log($"[MenuManager] Started game!");
    }

    void HandleMenuWindowOpen(CustomWindowType windowType)
    {
        Vector3 startRot = Camera.main.transform.eulerAngles;

        switch (windowType)
        {
            case CustomWindowType.MainMenu:
                Camera.main.transform.DORotate(new Vector3(startRot.x, 0f, startRot.z), 1.5f).SetUpdate(UpdateType.Late).SetEase(Ease.OutQuart);
                break;
            case CustomWindowType.Lobby:
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

        LobbyManager.OnReady -= HandleLobbyManagerReady;
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnGameStarted -= HandleStartGame;
        }
    }
}
