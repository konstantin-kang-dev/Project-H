using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Door : BasicInteractable
{
    [SerializeField] bool IsLockedOnStart = false;

    readonly SyncVar<bool> _isLocked = new SyncVar<bool>();

    [SerializeField] DoorVisuals _doorVisuals;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _isLocked.Value = IsLockedOnStart;
    }

    public override bool CanInteract()
    {
        return !_isLocked.Value;
    }

    public override void Interact(Player player, IPickable pickableInHand)
    {
        bool isPickableCompatible = (RequiredItemsToInteract.Count > 0 && pickableInHand != null) || RequiredItemsToInteract.Count == 0;
        int pickableObjectId = pickableInHand == null ? -1 : pickableInHand.ItemObjectId;

        if (IsRequiredPickable(pickableInHand) && !CanInteract() && isPickableCompatible)
        {
            RPC_RequestInteract(player.ObjectId, pickableObjectId);
        }
        else if(!CanInteract())
        {
            Shake();
        }
        else
        {
            RPC_RequestInteract(player.ObjectId, pickableObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void RPC_RequestInteract(int playerObjectId, int pickableObjectId)
    {
        if(_isLocked.Value == false)
        {
            _interactState.Value = !_interactState.Value;
            return;
        }

        NetworkObject playerNetworkObject = GetNetworkObject(playerObjectId);
        if (playerNetworkObject != null)
        {
            Player player = playerNetworkObject.GetComponent<Player>();

            NetworkObject pickableNetworkObject = GetNetworkObject(pickableObjectId);
            if (pickableNetworkObject != null)
            {
                IPickable pickable = pickableNetworkObject.GetComponent<IPickable>();
                if (!IsRequiredPickable(pickable))
                {
                    Debug.LogError($"[BasicInteractable] Required items list doesn't contain Pickable object({pickable.ItemConfig.Type}).");
                    return;
                }

                _interactState.Value = !_interactState.Value;
                _isLocked.Value = false;
            }
            else
            {
                Debug.LogError($"[BasicInteractable] Pickable object({pickableObjectId}) doesn't exist in network.");
            }
        }
        else
        {
            Debug.LogError($"[BasicInteractable] Player object({playerObjectId}) doesn't exist in network.");
        }
    }

    protected bool IsRequiredPickable(IPickable pickable)
    {
        if (RequiredItemsToInteract.Count == 0) return true;
        if (RequiredItemsToInteract.Count > 0 && pickable == null) return false;

        return RequiredItemsToInteract.Any((x) => x == pickable.ItemConfig.Type);
    }

    public override void SetAppearance(bool value)
    {
        base.SetAppearance(value);

        _doorVisuals.SetState(value);
    }

    [Client]
    void HandleIsLockedChange(bool prev, bool next, bool asServer)
    {
        if (next)
        {

        }
        else
        {

        }
    }

    void Shake()
    {
        RPC_RequestShake();
    }

    [ServerRpc(RequireOwnership = false)]
    void RPC_RequestShake()
    {
        RPC_HandleClientsShake();
    }
    [ObserversRpc]
    void RPC_HandleClientsShake()
    {
        _doorVisuals.Shake();
    }
}