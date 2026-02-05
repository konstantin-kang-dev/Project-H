using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MenuManager : MonoBehaviour
{
    [Inject] MenuPageNavigator _menuPageNavigator;

    [SerializeField] MainMenuUI _menuUI;
    [SerializeField] LobbyUI _lobbyUI;

    [SerializeField] TMP_InputField _playerNameInput;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        SaveManager.LoadAll();

        _playerNameInput.onValueChanged.AddListener(HandlePlayerNameInput);

        _menuUI.BindActionToMenuButton(MenuButtonType.CreateLobby, HandleCreateLobbyButton);
        _menuUI.BindActionToMenuButton(MenuButtonType.JoinLobby, HandleJoinLobbyButton);

        _lobbyUI.BindActionToBackBtn(HandleLobbyBackBtn);
        _lobbyUI.BindActionToStartBtn(HandleLobbyStartBtn);

        LobbyManager.Instance.OnClientConnected += HandleJoinLobby;
        LobbyManager.Instance.OnGameStarted += HandleStartGame;
        LobbyManager.Instance.OnClientConnectionLost += HandleLobbyBackBtn;

        _menuPageNavigator.OnWindowOpened += HandleMenuWindowOpen;

        GameDifficultyManager.Instance.Init();
    }

    void HandleCreateLobbyButton()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Popup);
        NetworkLobbyManager.Instance.CreateLobby();
    }
    void HandleJoinLobbyButton()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Popup);
    }

    void HandleLobbyBackBtn()
    {
        LobbyManager.Instance.StopConnection();
        _menuPageNavigator.OpenWindow(MenuWindowType.MainMenu);
    }
    void HandleLobbyStartBtn()
    {
        LobbyManager.Instance.StartGame();
    }

    void HandleJoinLobby()
    {
        _menuPageNavigator.OpenWindow(MenuWindowType.Lobby);
        LoadingManager.Instance.SetLoadingProgress(1f);
        Debug.Log($"[MenuManager] Joined lobby!");
    }

    void HandleStartGame()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen);
        Debug.Log($"[MenuManager] Started game!");
    }

    void HandlePlayerNameInput(string value)
    {
        SaveManager.GameData.PlayerName = value;
        SaveManager.SaveAll();
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

        LobbyManager.Instance.OnClientConnected -= HandleJoinLobby;
        LobbyManager.Instance.OnGameStarted -= HandleStartGame;
        LobbyManager.Instance.OnClientConnectionLost -= HandleLobbyBackBtn;

        _playerNameInput.onValueChanged.RemoveListener(HandlePlayerNameInput);
    }
}
