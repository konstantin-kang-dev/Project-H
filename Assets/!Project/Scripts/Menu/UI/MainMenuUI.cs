using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : BasicCustomWindow
{
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
                    button.onClick.AddListener(HandleContactsButton);
                    break;
                case MenuButtonType.Settings:
                    button.onClick.AddListener(HandleSettingsButton);
                    break;
                case MenuButtonType.Quit:
                    button.onClick.AddListener(HandleQuitButton);
                    break;
            }
        }
    }

    void Update()
    {
        
    }

    void HandleCreateLobbyButton()
    {
        NetworkGameManager.Instance.CreateLobby();
    }
    void HandleJoinLobbyButton()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.LobbiesOverview);
    }
    void HandleContactsButton()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.Contacts);
    }
    void HandleSettingsButton()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.Settings);
    }
    void HandleQuitButton()
    {
        Application.Quit();
    }
}
