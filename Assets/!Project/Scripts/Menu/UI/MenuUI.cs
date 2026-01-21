using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuUI : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<MenuButtonType, Button> _menuButtons = new Dictionary<MenuButtonType, Button>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
