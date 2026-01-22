using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Player");
    private readonly SyncVar<bool> _isReady = new SyncVar<bool>(false);
    private readonly SyncVar<int> _modelKey = new SyncVar<int>(0);

    [SerializeField] PlayerVisuals _playerVisuals;

    [SerializeField] LobbyPlayerUI _lobbyPlayerUI;
    public bool IsReady => _isReady.Value;

    public override void OnStartClient()
    {
        base.OnStartClient();

        int myClientId = base.IsClientStarted ? ClientManager.Connection.ClientId : -1;
        Debug.Log($"[CLIENT {myClientId}] LobbyPlayer spawned. Owner: {Owner.ClientId}, IsOwner: {base.IsOwner}, Scene: {gameObject.scene.name}");

        _modelKey.OnChange += OnPlayerModelChanged;

        _playerVisuals.Init();

        if (IsOwner)
        {
            _lobbyPlayerUI.BindActionsToModelChangeButtons(() =>
            {
                ChangePlayerModel(false);
            }, () =>
            {
                ChangePlayerModel(true);
            });
        }
        else
        {
            _lobbyPlayerUI.SetButtonsVisibility(false);
        }

         _lobbyPlayerUI.SetNicknameText(_playerName.Value);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _playerName.Value = "Player";
        _isReady.Value = false;
        _modelKey.Value = _playerVisuals.GetRandomModelKey();

    }

    [ServerRpc]
    void ChangePlayerModel(bool goForward)
    {
        _modelKey.Value = _playerVisuals.GetNextModelKey(_modelKey.Value, goForward);
    }

    [Client]
    void OnPlayerModelChanged(int prev, int next, bool asServer)
    {
        _playerVisuals.ChangePlayerModel(next);
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        _isReady.Value = ready;
    }

    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        _playerName.Value = name;
    }
}