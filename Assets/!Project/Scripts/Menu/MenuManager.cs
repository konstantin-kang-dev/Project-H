using DG.Tweening;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] MainMenuUI _menuUI;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _menuUI.BindActionToMenuButton(MenuButtonType.CreateLobby, HandleCreateLobbyButton);
        _menuUI.BindActionToMenuButton(MenuButtonType.JoinLobby, HandleJoinLobbyButton);

        MenuPageNavigator.Instance.OnWindowOpened += HandleMainMenuWindowOpen;
        MenuPageNavigator.Instance.OnWindowOpened += HandleLobbyWindowOpen;
    }

    void HandleCreateLobbyButton()
    {
        LobbyManager.Instance.StartHost();

        MenuPageNavigator.Instance.OpenWindow(MenuWindowType.Lobby);
    }
    void HandleJoinLobbyButton()
    {
        LobbyManager.Instance.StartClient();

        MenuPageNavigator.Instance.OpenWindow(MenuWindowType.Lobby);
    }

    void HandleMainMenuWindowOpen(MenuWindowType windowType)
    {
        if (windowType != MenuWindowType.MainMenu) return;

        Vector3 startRot = Camera.main.transform.eulerAngles;
        Camera.main.transform.DORotate(new Vector3(startRot.x, 0f, startRot.z), 0.3f).SetUpdate(UpdateType.Late);
    }
    void HandleLobbyWindowOpen(MenuWindowType windowType)
    {
        if (windowType != MenuWindowType.Lobby) return;

        Vector3 startRot = Camera.main.transform.eulerAngles;
        Camera.main.transform.DORotate(new Vector3(startRot.x, 180f, startRot.z), 0.3f).SetUpdate(UpdateType.Late);
    }

    void Update()
    {
        
    }
}
