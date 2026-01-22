using FishNet.Managing;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour, IMenuWindow
{
    [field: SerializeField] public MenuWindowType WindowType { get; private set; }
    [SerializeField] BasicWindowVisuals _visuals;

    [SerializeField] Button _backBtn;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _backBtn.onClick.AddListener(HandleClickBackBtn);
    }

    void HandleClickBackBtn()
    {
        if (LobbyManager.Instance.IsServerStarted)
        {
            
        }
        MenuPageNavigator.Instance.OpenWindow(MenuWindowType.MainMenu);
    }

    public void SetVisibility(bool visible, bool doInstantly = false)
    {
        if(visible)
        {
            _visuals.ProcessInAnimation(doInstantly);
        }
        else
        {
            _visuals.ProcessOutAnimation(doInstantly);
        }
    }

    void Update()
    {
        
    }
}
