using FishNet.Object;
using FishNet.Object.Synchronizing;

public class LobbyPlayer : NetworkBehaviour
{
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Player");
    private readonly SyncVar<bool> _isReady = new SyncVar<bool>(false);

    public bool IsReady => _isReady.Value;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _playerName.Value = "Player";
        _isReady.Value = false;
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