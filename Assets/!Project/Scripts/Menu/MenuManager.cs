using DG.Tweening;
using UnityEngine;
using Zenject;

public class MenuManager : MonoBehaviour
{
    [Inject] MenuPageNavigator _menuPageNavigator;

    [SerializeField] MainMenuUI _menuUI;
    [SerializeField] LobbyUI _lobbyUI;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _menuUI.BindActionToMenuButton(MenuButtonType.CreateLobby, HandleCreateLobbyButton);
        _menuUI.BindActionToMenuButton(MenuButtonType.JoinLobby, HandleJoinLobbyButton);

        _lobbyUI.BindActionToBackBtn(HandleLobbyBackBtn);
        _lobbyUI.BindActionToStartBtn(HandleLobbyStartBtn);

        LobbyManager.Instance.OnClientConnectionLost += HandleLobbyBackBtn;

        _menuPageNavigator.OnWindowOpened += HandleMenuWindowOpen;
    }

    void HandleCreateLobbyButton()
    {
        LobbyManager.Instance.StartHost();

        _menuPageNavigator.OpenWindow(MenuWindowType.Lobby);
    }
    void HandleJoinLobbyButton()
    {
        LobbyManager.Instance.StartClient();

        _menuPageNavigator.OpenWindow(MenuWindowType.Lobby);
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

        LobbyManager.Instance.OnClientConnectionLost -= HandleLobbyBackBtn;
    }
}
