using System;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    Canvas _canvas;

    [Header("Game UI")]
    [SerializeField] BasicWindowVisuals _gameUIVisuals;
    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] ObjectivesUI _objectivesUI;
    [SerializeField] ChatUI _chatUI;
    [SerializeField] StaminaUI _staminaUI;
    [SerializeField] LocationTitleUI _locationTitleUI;

    [Header("Windows")]
    [SerializeField] GameMenuUI _gameMenuUI;
    [SerializeField] GameplayUI _gameplayUI;
    [SerializeField] SettingsUI _settingsUI;

    public event Action<bool> OnGameplayUIFocusChange;

    public bool IsInitialized { get; private set; } = false;
    void Awake()
    {
        _canvas = GetComponent<Canvas>();

        Instance = this;
    }

    public void Init()
    {
        _inventoryUI.Init();
        _objectivesUI.Init();
        _chatUI.Init();
        _staminaUI.Init();
        _locationTitleUI.Init();

        WindowsNavigator.Instance.Clear();
        WindowsNavigator.Instance.CloseAll(true);
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameplayUI);

        _gameplayUI.OnVisibilityChange += HandleGameplayUIVisibilityChange;

        IsInitialized = true;

        Debug.Log($"[GameUI] Initialized.");
    }

    void HandleGameplayUIVisibilityChange(bool visible)
    {
        OnGameplayUIFocusChange?.Invoke(visible);
    }

    public void SetCanvasCamera(Camera camera)
    {
        _canvas.worldCamera = camera;
    }

    void Update()
    {
        
    }
}
