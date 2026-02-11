using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : SerializedMonoBehaviour, IMenuWindow
{
    [field: SerializeField] public MenuWindowType WindowType { get; }

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] BasicWindowVisuals _visuals;

    [SerializeField] Dictionary<MenuButtonType, Button> _menuButtons = new Dictionary<MenuButtonType, Button>();
    void Start()
    {
        
    }

    public void Init()
    {
        foreach (var buttonBlock in _menuButtons)
        {
            Button button = buttonBlock.Value;

            switch (buttonBlock.Key)
            {
                case MenuButtonType.CreateLobby:
                    button.onClick.AddListener(HandleCreateLobbyButton);
                    break;
                case MenuButtonType.JoinLobby:
                    button.onClick.AddListener(HandleJoinLobbyButton);
                    break;
                case MenuButtonType.Contacts:
                    break;
                case MenuButtonType.Settings:
                    break;
                case MenuButtonType.Quit:
                    break;
            }
        }
    }

    void Update()
    {
        
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

    void HandleCreateLobbyButton()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Popup);
        NetworkGameManager.Instance.CreateLobby();
    }
    void HandleJoinLobbyButton()
    {
        MenuWindowNavigator.Instance.OpenWindow(MenuWindowType.LobbiesOverview);
    }
}
