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

    public void BindActionToMenuButton(MenuButtonType type, UnityAction action)
    {
        if (action == null) throw new Exception($"[MenuUI] Binding action is null");

        if (!_menuButtons.ContainsKey(type)) throw new Exception($"[MenuUI] Menu button {type} not found");

        Button button = _menuButtons[type];
        button.onClick.AddListener(action);
    }
    public void UnbindActionToMenuButton(MenuButtonType type, UnityAction action)
    {
        if (action == null) throw new Exception($"[MenuUI] Binding action is null");

        if (!_menuButtons.ContainsKey(type)) throw new Exception($"[MenuUI] Menu button {type} not found");

        Button button = _menuButtons[type];
        button.onClick.RemoveListener(action);
    }
}
