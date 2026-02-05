using FishNet.Managing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : MonoBehaviour, IMenuWindow
{
    [field: SerializeField] public MenuWindowType WindowType { get; private set; }
    [SerializeField] BasicWindowVisuals _visuals;

    [SerializeField] ToggleGroup _difficultyToggleGroup;

    [SerializeField] Button _backBtn;
    [SerializeField] ToggleButton _readyBtn;
    [SerializeField] Button _startBtn;
    void Start()
    {
        LobbyManager.Instance.OnLocalPlayerRegister += Init;
        LobbyManager.Instance.OnLocalPlayerUnregister += ResetEvents;
    }

    public void Init()
    {
        _difficultyToggleGroup.OnToggle += HandleDifficultyToggle;
        _difficultyToggleGroup.ResetValues();

        SetStartBtnVisibility(LobbyManager.Instance.IsServerStarted);
        SetStartBtnInteractable(false);

        if (LobbyManager.Instance.IsServerStarted)
        {
            LobbyManager.Instance.OnPlayersReady += HandlePlayersReadyChange;
        }

        _readyBtn.OnToggle += HandleClickReadyBtn;
    }

    void HandleDifficultyToggle(int value)
    {
        GameDifficultyConfig selectedConfig = GameDifficultyManager.Instance.GetConfigByIndex(value);
        if (selectedConfig != null)
        {
            GameDifficultyManager.Instance.SelectConfig(selectedConfig.DifficultyType);
        }
    }

    public void BindActionToBackBtn(UnityAction action)
    {
        _backBtn.onClick.AddListener(action);
    }
    public void BindActionToReadyBtn(UnityAction action)
    {
        
    }
    void HandleClickReadyBtn(ToggleButton toggleButton)
    {
        bool readyState = toggleButton.State;
        LobbyManager.Instance.ChangeReadyState(readyState);
    }
    public void BindActionToStartBtn(UnityAction action)
    {
        _startBtn.onClick.AddListener(action);
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
        ResetEvents();
    }

    void ResetEvents()
    {
        if (LobbyManager.Instance != null && LobbyManager.Instance.IsServerStarted)
        {
            LobbyManager.Instance.OnPlayersReady -= HandlePlayersReadyChange;
        }

        LobbyManager.Instance.OnLocalPlayerRegister -= Init;
    }
}
