using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class LobbyPlayer : NetworkBehaviour
{
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Player");
    public string PlayerName => _playerName.Value;

    private readonly SyncVar<bool> _isReady = new SyncVar<bool>(false);
    private readonly SyncVar<int> _modelKey = new SyncVar<int>(0);
    public int ModelKey => _modelKey.Value;
    private readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public Vector3 LookPosition => _lookPosition.Value;

    [SerializeField] PlayerVisuals _playerVisuals;

    [SerializeField] LobbyPlayerUI _lobbyPlayerUI;
    public bool IsReady => _isReady.Value;

    public override void OnStartClient()
    {
        base.OnStartClient();

        int myClientId = base.IsClientStarted ? ClientManager.Connection.ClientId : -1;
        Debug.Log($"[CLIENT {myClientId}] LobbyPlayer spawned. Owner: {Owner.ClientId}, IsOwner: {base.IsOwner}, Scene: {gameObject.scene.name}");

        _modelKey.OnChange += CLIENT_OnPlayerModelChanged;
        _isReady.OnChange += CLIENT_OnPlayerReadyChanged;
        _lookPosition.OnChange += CLIENT_OnLookPositionUpdated;

        if (IsOwner)
        {
            LobbyManager.Instance.RegisterLocalLobbyPlayer(this);
            
            _lobbyPlayerUI.BindActionsToModelChangeButtons(() =>
            {
                int modelKey = _playerVisuals.GetNextModelKey(_modelKey.Value, false);
                RPC_RequestChangePlayerModel(modelKey);
            }, () =>
            {
                int modelKey = _playerVisuals.GetNextModelKey(_modelKey.Value, true);
                RPC_RequestChangePlayerModel(modelKey);
            });
        }
        else
        {
            _lobbyPlayerUI.SetButtonsVisibility(false);
        }

        if (IsServerStarted)
        {
            LobbyManager.Instance.RegisterLobbyPlayer(this);
        }

         _lobbyPlayerUI.SetNicknameText(_playerName.Value);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _playerName.Value = "Player";
        _isReady.Value = false;
        _modelKey.Value = _playerVisuals.GetRandomModelKey();

        NetworkPlayerData networkPlayerData = LobbyManager.Instance.ConnectedPlayers[Owner.ClientId];
        networkPlayerData.ModelKey = _modelKey.Value;
        LobbyManager.Instance.SERVER_UpdateNetworkPlayerData(Owner.ClientId, networkPlayerData);
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleMouseMove();
        }
    }

    void HandleMouseMove()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point;
            hitPoint.z = transform.position.z + 1.4f;
            _playerVisuals.AnimatorController.SetLookPosition(hitPoint);
            RPC_RequestSetLookPosition(hitPoint);
        }
    }

    [ServerRpc]
    void RPC_RequestChangePlayerModel(int modelKey)
    {
        _modelKey.Value = modelKey;
    }

    [Client]
    void CLIENT_OnPlayerModelChanged(int prev, int next, bool asServer)
    {
        if (asServer) return;

        _playerVisuals.ChangePlayerModel(next);

        if(IsServerStarted)
        {
            NetworkPlayerData networkPlayerData = LobbyManager.Instance.ConnectedPlayers[Owner.ClientId];
            networkPlayerData.ModelKey = _modelKey.Value;
            LobbyManager.Instance.SERVER_UpdateNetworkPlayerData(Owner.ClientId, networkPlayerData);
        }
    }

    [ServerRpc]
    public void RPC_RequestSetReady(bool ready)
    {
        SERVER_SetReady(ready);
    }
    [Server]
    public void SERVER_SetReady(bool ready)
    {
        _isReady.Value = ready;
        Debug.Log($"[LobbyPlayer] Changed ready state to: {ready}");
    }
    [Client] 
    void CLIENT_OnPlayerReadyChanged(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        _lobbyPlayerUI.SetReadyIconVisibility(next);

        if (IsServerStarted)
        {
            LobbyManager.Instance.SERVER_HandlePlayerReadyStateChange();
        }
    }

    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        _playerName.Value = name;
    }
    [ServerRpc]
    public void RPC_RequestSetLookPosition(Vector3 lookPosition)
    {
        _lookPosition.Value = lookPosition;
    }
    [Client]
    void CLIENT_OnLookPositionUpdated(Vector3 prev, Vector3 next, bool asServer)
    {
        if (asServer) return;

        if (_playerVisuals != null && _playerVisuals.AnimatorController != null)
        {
            _playerVisuals.AnimatorController.SetLookPosition(next);
        }
    }

    public override void OnDespawnServer(NetworkConnection connection)
    {
        base.OnDespawnServer(connection);

        if (IsServerStarted)
        {
            LobbyManager.Instance.UnregisterLobbyPlayer(this);
        }

        if (IsOwner)
        {
            LobbyManager.Instance.UnregisterLocalLobbyPlayer();
        }
    }
}