using UnityEngine;
using System.Collections;
using System;

[Serializable]
public enum LoadingWindowType
{
    Popup = 0,
    Screen = 1,
}

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField] LoadingPopup _loadingPopup;
    [SerializeField] LoadingScreen _loadingScreen;

    LoadingWindowType _openedLoadingWindow;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void ShowLoading(LoadingWindowType type)
    {
        switch (type)
        {
            case LoadingWindowType.Popup:
                _loadingPopup.SetVisibility(true);
                break;
            case LoadingWindowType.Screen:
                _loadingScreen.SetVisibility(true);
                break;
            default:
                break;
        }

        _openedLoadingWindow = type;
    }

    public void SetLoadingProgress(float progress)
    {
        switch (_openedLoadingWindow)
        {
            case LoadingWindowType.Popup:
                if(progress >= 1f)
                {
                    _loadingPopup.SetVisibility(false);
                }
                break;
            case LoadingWindowType.Screen:
                _loadingScreen.SetLoadingProgress(progress);
                break;
            default:
                break;
        }
    }


}