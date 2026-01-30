using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicInteractable : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public List<ItemType> RequiredItemsToInteract { get; private set; } = new List<ItemType>();

    protected readonly SyncVar<bool> _interactState = new SyncVar<bool>();

    [SerializeField] EnhancedAudio _interactionAS1;
    [SerializeField] EnhancedAudio _interactionAS2;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _interactState.OnChange += HandleInteractStateChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _interactState.Value = false;
    }

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual void Interact(Player player, IPickable pickableInHand)
    {
        bool isPickableCompatible = (RequiredItemsToInteract.Count > 0 && pickableInHand != null) || RequiredItemsToInteract.Count == 0;
        int pickableObjectId = pickableInHand == null ? -1 : pickableInHand.ItemObjectId;

        if(CanInteract() && player != null && isPickableCompatible)
        {
            RPC_RequestInteract(player.ObjectId, pickableObjectId);
        }
        else
        {

        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void RPC_RequestInteract(int playerObjectId, int pickableObjectId)
    {
        NetworkObject playerNetworkObject = GetNetworkObject(playerObjectId);
        if (playerNetworkObject != null)
        {
            Player player = playerNetworkObject.GetComponent<Player>();

            _interactState.Value = true;
        }
        else
        {
            Debug.LogError($"[BasicInteractable] Player object({playerObjectId}) doesn't exist in network.");
        }
    }

    [Client]
    protected virtual void HandleInteractStateChange(bool prev, bool next, bool asServer)
    {
        SetAppearance(next);
    }

    
    protected NetworkObject GetNetworkObject(int objectId)
    {
        Dictionary<int, NetworkObject> allNetworkObjects = ServerManager.Objects.Spawned;

        if (allNetworkObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            return networkObject;
        }

        return null;
    }


    public virtual void SetHighlight(bool value)
    {
        throw new System.NotImplementedException();
    }

    public virtual void SetAppearance(bool value)
    {
        if (value)
        {
            if(_interactionAS1 != null)
            {
                _interactionAS1.Play();
            }
        }
        else
        {
            if(_interactionAS2 != null)
            {
                _interactionAS2.Play();
            }

        }
    }

    [Server]
    public virtual void ResetAll()
    {
        _interactState.Value = false;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
