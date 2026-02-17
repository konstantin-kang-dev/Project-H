using FishNet.Component.Animating;
using FishNet.Component.Transforming;
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

    readonly SyncVar<bool> _isKnockedDown = new SyncVar<bool>();
    public bool IsKnockedDown => _isKnockedDown.Value;  

    public bool IsInvincible = false;

    NetworkTransform _networkTransform;
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
        _isKnockedDown.OnChange += HandleKnockedDownChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _networkTransform = GetComponent<NetworkTransform>();

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

    [Server]
    public void SERVER_SetKnockedDown(bool value)
    {
        _isKnockedDown.Value = value;
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

    [ServerRpc]
    public void RPC_RequestRevive()
    {
        SERVER_SetKnockedDown(false);
    }

    [Client]
    void HandleKnockedDownChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner)
        {
            PlayerController.HandleKnockDown(next);
        }
    }

    [Server]
    public void Teleport(Vector3 targetPos)
    {
        transform.position = targetPos;
        _networkTransform.Teleport();

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
