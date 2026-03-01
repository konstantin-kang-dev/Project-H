using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMenuUI : BasicCustomWindow
{
    [Header("Buttons")]
    [SerializeField] Button _continueBtn;
    [SerializeField] Button _settingsBtn;
    [SerializeField] Button _quitToMenuBtn;

    [Header("Windows")]
    [SerializeField] SettingsUI _settingsUI;

    private void Awake()
    {
        _windowVisuals = GetComponent<BasicWindowVisuals>();
    }

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

    void HandleEscPressed()
    {
        if (!IsVisible) SetVisibility(true, false);
    }

    void HandleContinueBtn()
    {
        SetVisibility(false, false);
    }

    void HandleSettingsBtn()
    {
        void HandleVisibilityChange(bool visible)
        {
            if (!visible)
            {
                SetVisibility(true, false);
                _settingsUI.OnVisibilityChange -= HandleVisibilityChange;
            }
        }

        _settingsUI.OnVisibilityChange += HandleVisibilityChange;
        _settingsUI.SetVisibility(true, false);
    }
    
    void HandleQuitToMenuBtn()
    {
        //TODO
    }
}