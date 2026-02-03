using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Player : NetworkBehaviour
{
    readonly SyncVar<bool> _isReadyToInit = new SyncVar<bool>();
    public bool IsReadyToInit => _isReadyToInit.Value;

    readonly SyncVar<string> _playerName = new SyncVar<string>();
    public string PlayerName => _playerName.Value;

    readonly SyncVar<int> _modelKey = new SyncVar<int>();
    public int ModelKey => _modelKey.Value;

    readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public Vector3 LookPosition => _lookPosition.Value;

    public bool IsInvincible = false;
    [field: SerializeField] public PlayerController PlayerController { get; private set; }
    [field: SerializeField] public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {

    }
    void Start()
    {

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            GameManager.Instance.RegisterLocalPlayer(this);
        }
        _isReadyToInit.OnChange += HandleIsReadyToInitChange;

        _lookPosition.OnChange += HandleLookPositionChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _modelKey.Value = -1;
        _isReadyToInit.Value = false;
    }

    public void Init()
    {
        PlayerController.Init(this);

        IsInitialized = true;
    }
    [Server]
    public void SERVER_SetReadyToInit(bool value)
    {
        _isReadyToInit.Value = value;
    }

    [Client]
    void HandleIsReadyToInitChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        if (next)
        {
            Init();
        }
    }

    [Server]
    public void SERVER_SetModelKey(int modelKey)
    {
        _modelKey.Value = modelKey;
    }

    [Server]
    public void SERVER_SetPlayerName(string playerName)
    {
        _playerName.Value = playerName;
    }

    [ServerRpc]
    public void RPC_RequestSetLookPosition(Vector3 lookPosition)
    {
        _lookPosition.Value = lookPosition;
    }

    [Client]
    void HandleLookPositionChange(Vector3 prev, Vector3 next, bool asServer)
    {
        if (asServer) return;

        PlayerController.SetLookPosition(next);
    }

    void Update()
    {
        
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _isReadyToInit.OnChange -= HandleIsReadyToInitChange;

        _lookPosition.OnChange -= HandleLookPositionChange;
    }
}
