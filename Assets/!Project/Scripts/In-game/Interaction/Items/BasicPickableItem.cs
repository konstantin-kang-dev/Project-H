using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Modules.Rendering.Outline;
using System.Collections.Generic;
using UnityEngine;

public class BasicPickableItem : NetworkBehaviour, IPickable
{
    [field: SerializeField] public ItemConfig ItemConfig { get; private set; }
    public Transform Transform { get; private set; }
    public int ItemObjectId { get; private set; } = -1;

    [SerializeField] GameObject _container;
    protected NetworkTransform _netTransform;
    protected Rigidbody _rb;
    [SerializeField] protected Collider _physicsCollider;
    [SerializeField] protected Collider _triggerCollider;

    protected readonly SyncVar<bool> _isPickedUp = new SyncVar<bool>();
    public bool IsPickedUp => _isPickedUp.Value;
    protected readonly SyncVar<int> _picker = new SyncVar<int>();
    public int Picker => _picker.Value;
    Player _lastPicker;

    protected readonly SyncVar<bool> _interactionState = new SyncVar<bool>();

    [SerializeField] List<OutlineComponent> _outlines = new List<OutlineComponent>();
    void Awake()
    {
        if(ItemConfig == null)
        {
            throw new System.Exception($"[BasicPickableItem] Item config is not set.");
        }

        ItemConfig = ItemConfig.Clone();
        Transform = transform;
        _rb = GetComponent<Rigidbody>();
        _netTransform = GetComponent<NetworkTransform>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _interactionState.OnChange += CLIENT_HandleInteractState;

        ItemObjectId = ObjectId;

        _rb.isKinematic = true;
        SetColliders(true);
        SetHighlight(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _isPickedUp.Value = false;
        _picker.Value = -1;

        _rb.isKinematic = false;
        SetColliders(true);
        _netTransform.enabled = true;
    }

    void Update()
    {

    }

    [Server]
    public virtual void SERVER_PickUp(int playerObjectId)
    {
        if (!ServerManager.Objects.Spawned.TryGetValue(playerObjectId, out NetworkObject networkObject)) return;

        _isPickedUp.Value = true;
        _picker.Value = playerObjectId;

        _rb.isKinematic = true;
        SetColliders(false);
        _netTransform.enabled = false;
        Debug.Log($"[BasicPickableItem] Picked up by: {_picker.Value}");
    }

    [Server]
    public virtual void SERVER_Drop()
    {
        _isPickedUp.Value = false;
        _picker.Value = -1;

        _rb.isKinematic = false;
        _netTransform.enabled = true;
        SetColliders(true);

        _rb.AddForce(_rb.transform.forward * 100f);
    }

    [Server]
    public virtual void SERVER_Interact()
    {
        _interactionState.Value = !_interactionState.Value;
    }

    [Client]
    protected virtual void CLIENT_HandleInteractState(bool prev, bool next, bool asServer)
    {

    }

    public virtual void SetHighlight(bool value)
    {
        foreach (var outline in _outlines)
        {
            outline.enabled = value;
        }
    }
    public virtual void SetVisibility(bool value)
    {
        _container.SetActive(value);
    }

    public virtual void SetColliders(bool value)
    {
        _physicsCollider.enabled = value;
        _triggerCollider.enabled = value;
    }
}
