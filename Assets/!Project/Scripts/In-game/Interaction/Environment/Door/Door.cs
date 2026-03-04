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

        _isLocked.OnChange += HandleIsLockedChange;
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

    public override void Interact(IPickable pickableInHand)
    {
        bool isPickableCompatible = (RequiredItemToInteract != ItemType.None && pickableInHand != null) || RequiredItemToInteract == ItemType.None;
        int pickableObjectId = pickableInHand == null ? -1 : pickableInHand.ItemObjectId;

        if (IsRequiredPickable(pickableInHand) && !CanInteract() && isPickableCompatible)
        {
            RPC_RequestInteract(pickableObjectId);
        }
        else if(!CanInteract())
        {
            Shake();
        }
        else
        {
            RPC_RequestInteract(pickableObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void RPC_RequestInteract(int pickableObjectId)
    {
        if(_isLocked.Value == false)
        {
            _interactState.Value = !_interactState.Value;
            return;
        }

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

    protected bool IsRequiredPickable(IPickable pickable)
    {
        if (RequiredItemToInteract == ItemType.None) return true;
        if (RequiredItemToInteract != ItemType.None && pickable == null) return false;

        return RequiredItemToInteract == pickable.ItemConfig.Type;
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
            RequirementsHintText = string.Empty;
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