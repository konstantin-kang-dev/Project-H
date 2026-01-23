using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuPageNavigator : SerializedMonoBehaviour
{
    [SerializeField] GameObject _inputBlocker;

    [SerializeField] Dictionary<MenuWindowType, IMenuWindow> _menuWindows = new Dictionary<MenuWindowType, IMenuWindow>();
    IMenuWindow _openedWindow;

    public event Action<MenuWindowType> OnWindowOpened;
    private void Awake()
    {

    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach (var window in _menuWindows)
        {
            if (window.Value.WindowType == MenuWindowType.MainMenu) continue;
            window.Value.SetVisibility(false, true);
        }

        OpenWindow(MenuWindowType.MainMenu);
    }

    void Update()
    {
        
    }

    public void OpenWindow(MenuWindowType type)
    {
        if (!_menuWindows.ContainsKey(type)) throw new System.Exception($"[MenuPageNavigator] Window with type: {type} not found");

        if(_openedWindow != null)
        {
            _openedWindow.SetVisibility(false, false);
        }

        _openedWindow = _menuWindows[type];
        _openedWindow.SetVisibility(true, false);

        OnWindowOpened?.Invoke(type);
    }

    public void SetInputBlocker(bool active)
    {
        _inputBlocker.SetActive(active);
    }
}
