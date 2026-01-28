using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    public static GameCanvas Instance { get; private set; }
    Canvas _canvas;

    [SerializeField] InventoryUI _inventoryUI;

    public bool IsInitialized { get; private set; } = false;
    void Awake()
    {
        _canvas = GetComponent<Canvas>();

        Instance = this;
    }

    public void SetCamera(Camera camera)
    {
        _canvas.worldCamera = camera;
    }

    public void Init()
    {
        _inventoryUI.Init();

        IsInitialized = true;
    }
    void Update()
    {
        
    }
}
