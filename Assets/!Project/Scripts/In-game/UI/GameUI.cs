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

    [Header("Windows")]
    [SerializeField] GameMenuUI _gameMenuUI;
    [SerializeField] SettingsUI _settingsUI;

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
        _settingsUI.SetVisibility(false, true);

        IsInitialized = true;
    }

    void Update()
    {
        
    }
}
