using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowsNavigator : SerializedMonoBehaviour
{
    public static WindowsNavigator Instance;

    [SerializeField] GameObject _inputBlocker;

    [SerializeField] Dictionary<CustomWindowType, ICustomWindow> _menuWindows = new Dictionary<CustomWindowType, ICustomWindow>();
    ICustomWindow _openedWindow;
    Stack<CustomWindowType> _windowsHistory = new Stack<CustomWindowType>();

    public event Action<CustomWindowType> OnWindowOpened;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach (var window in _menuWindows)
        {
            if (window.Value.WindowType == CustomWindowType.MainMenu) continue;
            window.Value.SetVisibility(false, true);
        }

        OpenWindow(CustomWindowType.MainMenu);
    }

    void Update()
    {
        
    }

    public void OpenWindow(CustomWindowType type)
    {
        if (!_menuWindows.ContainsKey(type)) throw new Exception($"[WindowsNavigator] Window {type} not found");

        if (_openedWindow != null)
        {
            _windowsHistory.Push(_openedWindow.WindowType);
            _openedWindow.SetVisibility(false, false);
        }

        ICustomWindow nextWindow = _menuWindows[type];

        nextWindow.SetVisibility(true, false);

        _openedWindow = nextWindow;
        OnWindowOpened?.Invoke(type);

        Debug.Log($"[WindowsNavigator] Opened window: {type}");
    }

    public void GoBack()
    {
        if (_windowsHistory.Count == 0) return;

        var prevType = _windowsHistory.Pop();
        Debug.Log($"[WindowsNavigator] Go back to window: {prevType}");

        OpenWindow(prevType);
    }

    public void Clear()
    {
        foreach (var window in _windowsHistory)
        {
            _menuWindows[window].Clear();
        }
        _windowsHistory.Clear();
    }

    public void SetInputBlocker(bool active)
    {
        _inputBlocker.SetActive(active);
    }
}
