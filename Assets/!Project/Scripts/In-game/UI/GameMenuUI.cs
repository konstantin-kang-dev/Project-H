using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameMenuUI : BasicCustomWindow
{
    [Header("Buttons")]
    [SerializeField] Button _continueBtn;
    [SerializeField] Button _settingsBtn;
    [SerializeField] Button _quitToMenuBtn;

    void OnEnable()
    {
        //GlobalInputManager.Input.OnEscPressed += HandleEscPressed;

        _continueBtn.onClick.AddListener(HandleContinueBtn);
        _settingsBtn.onClick.AddListener(HandleSettingsBtn);
        _quitToMenuBtn.onClick.AddListener(HandleQuitToMenuBtn);
    }

    private void OnDisable()
    {
        //GlobalInputManager.Input.OnEscPressed -= HandleEscPressed;

        _continueBtn.onClick.RemoveListener(HandleContinueBtn);
        _settingsBtn.onClick.RemoveListener(HandleSettingsBtn);
        _quitToMenuBtn.onClick.RemoveListener(HandleQuitToMenuBtn);
    }

    protected override void BindControls()
    {
        GlobalInputManager.Input.OnEscPressed += HandleEscPressed;
    }

    protected override void UnbindControls()
    {
        GlobalInputManager.Input.OnEscPressed -= HandleEscPressed;
    }

    void HandleEscPressed()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameplayUI);
    }


    void HandleContinueBtn()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameplayUI);
    }

    void HandleSettingsBtn()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.Settings);
    }
    
    async void HandleQuitToMenuBtn()
    {
        NetworkGameManager.Instance.Disconnect();
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "Loading Menu");

        await UniTask.WaitForSeconds(0.5f);
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}