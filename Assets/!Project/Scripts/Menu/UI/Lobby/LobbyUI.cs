using FishNet.Managing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : BasicCustomWindow
{
    [SerializeField] BasicWindowVisuals _visuals;

    [SerializeField] ChatUI _lobbyChatUI;

    [SerializeField] ToggleGroup _difficultyToggleGroup;

    [SerializeField] Button _backBtn;
    [SerializeField] ToggleButton _readyBtn;
    [SerializeField] Button _startBtn;
    void Start()
    {
        LobbyManager.OnReady += Init;
        LobbyManager.OnClear += ResetEvents;
    }

    public void Init()
    {
        if (LobbyManager.Instance.IsServerStarted)
        {
            _difficultyToggleGroup.ResetValues();
            _difficultyToggleGroup.OnToggle += HandleDifficultyToggle;
        }
        else
        {
            _difficultyToggleGroup.SetButtonsVisibility(false);
        }

        SetStartBtnVisibility(LobbyManager.Instance.IsServerStarted);
        SetStartBtnInteractable(false);

        LobbyManager.Instance.OnLobbyDataUpdated += HandleUpdateLobbyData;

        if (LobbyManager.Instance.IsServerStarted)
        {
            LobbyManager.Instance.OnPlayersReady += HandlePlayersReadyChange;
        }

        _readyBtn.OnToggle += HandleClickReadyBtn;
        _backBtn.onClick.AddListener(HandleLobbyBackBtn);
        _startBtn.onClick.AddListener(HandleLobbyStartBtn);

        _lobbyChatUI.Init();

        Debug.Log($"[LobbyUI] Initialized");
    }
    protected override void BindControls()
    {

    }

    protected override void UnbindControls()
    {

    }

    void HandleDifficultyToggle(int value)
    {
        GameDifficultyConfig selectedConfig = GameDifficultyManager.Instance.GetConfigByIndex(value);
        if (selectedConfig != null)
        {
            GameDifficultyManager.Instance.SelectConfig(selectedConfig.DifficultyType);
        }

        LobbyManager.Instance.SERVER_UpdateDifficulty((DifficultyType)value);
    }

    void HandleUpdateLobbyData(LobbyData lobbyData)
    {
        if(!LobbyManager.Instance.IsServerStarted)
        {
            _difficultyToggleGroup.SetValue((int)lobbyData.ChosenDifficulty);
        }
    }

    void HandleClickReadyBtn(ToggleButton toggleButton)
    {
        bool readyState = toggleButton.State;
        LobbyManager.Instance.ChangeReadyState(readyState);
    }
    void HandleLobbyBackBtn()
    {
        NetworkGameManager.Instance.Disconnect();
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.MainMenu);
    }
    void HandleLobbyStartBtn()
    {
        LobbyManager.Instance.RPC_RequestStartGame();
    }

    public void SetStartBtnInteractable(bool interactable)
    {
        _startBtn.interactable = interactable;
    }
    public void SetStartBtnVisibility(bool visibility)
    {
        _startBtn.gameObject.SetActive(visibility);
    }

    void HandlePlayersReadyChange(bool allReady)
    {
        SetStartBtnInteractable(allReady);
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        LobbyManager.OnReady -= Init;
        LobbyManager.OnClear -= ResetEvents;
        ResetEvents();
    }

    void ResetEvents()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnLobbyDataUpdated -= HandleUpdateLobbyData;
            LobbyManager.Instance.OnPlayersReady -= HandlePlayersReadyChange;
        }

        if (GameDifficultyManager.Instance != null)
            _difficultyToggleGroup.OnToggle -= HandleDifficultyToggle;

        _readyBtn.OnToggle -= HandleClickReadyBtn;
        _backBtn.onClick.RemoveListener(HandleLobbyBackBtn);
        _startBtn.onClick.RemoveListener(HandleLobbyStartBtn);

        Debug.Log($"[LobbyUI] Reset events");
    }
}
