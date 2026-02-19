using FishNet.Object;
using FishNet.Object.Synchronizing;
using Modules.Rendering.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicInteractable : NetworkBehaviour, IInteractable, IHintable
{
    [field: SerializeField] public InteractableConfig Config { get; private set; }
    public Transform Transform { get; private set; }

    [field: SerializeField] public Transform HintPoint { get; private set; }
    [field: SerializeField] public string HintText { get; private set; }
    [field: SerializeField] public string RequirementsHintText { get; private set; }

    public bool InteractionState => _interactState.Value;
    [field: SerializeField] public ItemType RequiredItemToInteract { get; private set; } = ItemType.None;

    protected readonly SyncVar<bool> _interactState = new SyncVar<bool>();

    [SerializeField] protected bool _toggleInteractionState = true;

    [SerializeField] EnhancedAudio _interactionAS1;
    [SerializeField] EnhancedAudio _interactionAS2;


    public event Action<IInteractable, bool> OnInteractStateChange;

    protected virtual void Awake()
    {
        if(RequiredItemToInteract != ItemType.None)
        {
            RequirementsHintText = ProjectUtils.CamelCaseToSpaced(RequiredItemToInteract.ToString());
        }

        Transform = transform;
    }

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

    public virtual void Interact(IPickable pickableInHand)
    {
        bool isPickableCompatible = (RequiredItemToInteract != ItemType.None && pickableInHand != null) || RequiredItemToInteract == ItemType.None;
        int pickableObjectId = pickableInHand == null ? -1 : pickableInHand.ItemObjectId;

        if(CanInteract() && isPickableCompatible)
        {
            RPC_RequestInteract(pickableObjectId);
        }
        else
        {

        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void RPC_RequestInteract(int pickableObjectId)
    {
        if (_toggleInteractionState)
        {
            _interactState.Value = !_interactState.Value;
        }
        else
        {
            _interactState.Value = true;
        }
    }

    [Client]
    protected virtual void HandleInteractStateChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        SetAppearance(next);

        OnInteractStateChange?.Invoke(this, next);
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
}
