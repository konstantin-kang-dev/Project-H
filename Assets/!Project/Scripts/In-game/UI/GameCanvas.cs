using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    public static GameCanvas Instance { get; private set; }
    Canvas _canvas;

    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] ObjectivesUI _objectivesUI;
    [SerializeField] ChatUI _chatUI;

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

        IsInitialized = true;
    }
    void Update()
    {
        
    }
}
