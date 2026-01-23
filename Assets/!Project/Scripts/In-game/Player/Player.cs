using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Player : NetworkBehaviour
{
    readonly SyncVar<string> _playerName = new SyncVar<string>();
    public string PlayerName => _playerName.Value;

    readonly SyncVar<int> _modelKey = new SyncVar<int>();
    public int ModelKey => _modelKey.Value;

    readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public Vector3 LookPosition => _lookPosition.Value;

    readonly SyncVar<float> _characterRotationY = new SyncVar<float>();
    public float CharacterRotationY => _characterRotationY.Value;

    readonly SyncVar<bool> _isWalking = new SyncVar<bool>();
    public bool IsWalking => _isWalking.Value;
    readonly SyncVar<bool> _isSprinting = new SyncVar<bool>();
    public bool IsSprinting => _isSprinting.Value;

    public bool IsInvincible = false;
    [SerializeField] PlayerController _playerController;
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

        Init();
        _modelKey.OnChange += HandleModelKeyChange;
        _lookPosition.OnChange += HandleLookPositionChange;
        _characterRotationY.OnChange += HandleCharacterRotationChange;
        _isWalking.OnChange += HandleWalkingStateChange;
        _isSprinting.OnChange += HandleSprintingStateChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public void Init()
    {
        _playerController.Init(this);

        IsInitialized = true;
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

    [Client]
    void HandleModelKeyChange(int prev, int next, bool asServer)
    {
        _playerController.ChangePlayerModel(next);
    }

    [ServerRpc]
    public void RPC_RequestSetLookPosition(Vector3 lookPosition)
    {
        _lookPosition.Value = lookPosition;
    }

    [Client]
    void HandleLookPositionChange(Vector3 prev, Vector3 next, bool asServer)
    {
        _playerController.SetLookPosition(next);
    }

    [ServerRpc]
    public void RPC_RequestSetCharacterRotation(Vector2 rotations)
    {
        _characterRotationY.Value = rotations.y;
    }

    [Client]
    void HandleCharacterRotationChange(float prev, float next, bool asServer)
    {
        _playerController.SetCharacterRotation(next);
    }

    [ServerRpc]
    public void RPC_RequestSetWalkingState(bool value)
    {
        _isWalking.Value = value;
    }

    [Client]
    void HandleWalkingStateChange(bool prev, bool next, bool asServer)
    {
        _playerController.SetWalkingState(next);
    }

    [ServerRpc]
    public void RPC_RequestSetSprintingState(bool value)
    {
        _isSprinting.Value = value;
    }

    [Client]
    void HandleSprintingStateChange(bool prev, bool next, bool asServer)
    {
        _playerController.SetSprintingState(next);
    }

    void Update()
    {
        
    }
}
