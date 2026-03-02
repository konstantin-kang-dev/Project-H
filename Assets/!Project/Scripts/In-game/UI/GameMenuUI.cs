using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMenuUI : BasicCustomWindow
{
    [Header("Buttons")]
    [SerializeField] Button _continueBtn;
    [SerializeField] Button _settingsBtn;
    [SerializeField] Button _quitToMenuBtn;

    void OnEnable()
    {
        GlobalInputManager.Input.OnEscPressed += HandleEscPressed;

        _continueBtn.onClick.AddListener(HandleContinueBtn);
        _settingsBtn.onClick.AddListener(HandleSettingsBtn);
        _quitToMenuBtn.onClick.AddListener(HandleQuitToMenuBtn);
    }

    private void OnDisable()
    {
        GlobalInputManager.Input.OnEscPressed -= HandleEscPressed;

        _continueBtn.onClick.RemoveListener(HandleContinueBtn);
        _settingsBtn.onClick.RemoveListener(HandleSettingsBtn);
        _quitToMenuBtn.onClick.RemoveListener(HandleQuitToMenuBtn);
    }

    protected override void BindControls()
    {
        
    }

    protected override void UnbindControls()
    {
        
    }

    void HandleEscPressed()
    {
        if (!IsVisible)
        {
            WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameMenu);
        }
        else
        {
            WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameplayUI);
        }
        Debug.Log($"[GameMenuUI] HandleEscPressed IsVisible: {IsVisible}");
    }

    void HandleContinueBtn()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameplayUI);
    }

    void HandleSettingsBtn()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.Settings);
    }
    
    void HandleQuitToMenuBtn()
    {
        //TODO
    }
}