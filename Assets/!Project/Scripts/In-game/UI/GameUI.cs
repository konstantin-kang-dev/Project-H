using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    Canvas _canvas;

    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] ObjectivesUI _objectivesUI;
    [SerializeField] ChatUI _chatUI;
    [SerializeField] StaminaUI _staminaUI;

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

        IsInitialized = true;
    }
    void Update()
    {
        
    }
}
