using DG.Tweening;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] MenuUI _menuUI;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _menuUI.BindActionToMenuButton(MenuButtonType.CreateLobby, HandleCreateLobbyButton);
        _menuUI.BindActionToMenuButton(MenuButtonType.JoinLobby, HandleJoinLobbyButton);
    }

    void HandleCreateLobbyButton()
    {
        LobbyManager.Instance.StartHost();

        Vector3 startRot = Camera.main.transform.eulerAngles;
        Camera.main.transform.DORotate(new Vector3(startRot.x, 180f, startRot.z), 0.7f);
    }
    void HandleJoinLobbyButton()
    {
        LobbyManager.Instance.StartClient();
        Vector3 startRot = Camera.main.transform.eulerAngles;
        Camera.main.transform.DORotate(new Vector3(startRot.x, 180f, startRot.z), 0.7f);
    }

    void Update()
    {
        
    }
}
