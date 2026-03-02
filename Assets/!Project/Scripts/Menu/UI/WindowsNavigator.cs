using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowsNavigator : SerializedMonoBehaviour
{
    public static WindowsNavigator Instance;

    [SerializeField] GameObject _inputBlocker;

    [SerializeField] Dictionary<CustomWindowType, ICustomWindow> _availableWindows = new Dictionary<CustomWindowType, ICustomWindow>();
    ICustomWindow _openedWindow;
    Stack<CustomWindowType> _windowsHistory = new Stack<CustomWindowType>();

    public event Action<CustomWindowType> OnWindowOpened;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void CloseAll(bool doInstantly)
    {
        foreach (var window in _availableWindows)
        {
            ICustomWindow customWindow = window.Value;

            customWindow.SetVisibility(false, doInstantly);
        }
    }

    public void OpenWindow(CustomWindowType type)
    {
        if (!_availableWindows.ContainsKey(type)) throw new Exception($"[WindowsNavigator] Window {type} not found");

        if (_openedWindow != null)
        {
            _windowsHistory.Push(_openedWindow.WindowType);
            _openedWindow.SetVisibility(false, false);
        }

        ICustomWindow nextWindow = _availableWindows[type];

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
        foreach (var window in _availableWindows)
        {
            window.Value.Clear();
        }

        _windowsHistory.Clear();
        _openedWindow = null;
    }

    public void SetInputBlocker(bool active)
    {
        _inputBlocker.SetActive(active);
    }
}
