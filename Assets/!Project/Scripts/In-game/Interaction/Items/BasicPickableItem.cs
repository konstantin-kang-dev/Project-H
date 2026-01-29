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
    protected Collider _collider;

    protected readonly SyncVar<bool> _isPickedUp = new SyncVar<bool>();
    public bool IsPickedUp => _isPickedUp.Value;
    protected readonly SyncVar<int> _picker = new SyncVar<int>();
    public int Picker => _picker.Value;
    Player _lastPicker;

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
        _collider = GetComponent<Collider>();
        _netTransform = GetComponent<NetworkTransform>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _picker.OnChange += HandlePickerChange;
        ItemObjectId = ObjectId;

        _rb.isKinematic = true;
        _collider.enabled = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _isPickedUp.Value = false;
        _picker.Value = -1;

        _rb.isKinematic = false;
        _collider.enabled = true;
        _netTransform.enabled = true;
    }

    void Update()
    {

    }

    public virtual void PickUp(int playerObjectId)
    {
        RPC_RequestPickUp(playerObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void RPC_RequestPickUp(int playerObjectId)
    {
        if (!ServerManager.Objects.Spawned.TryGetValue(playerObjectId, out NetworkObject networkObject)) return;

        _isPickedUp.Value = true;
        _picker.Value = playerObjectId;

        _rb.isKinematic = true;
        _collider.enabled = false;
        _netTransform.enabled = false;
        Debug.Log($"[BasicPickableItem] Picked up by: {_picker.Value}");
    }

    protected virtual void HandlePickerChange(int prev, int next, bool asServer)
    {
        if (ClientManager.Objects.Spawned.TryGetValue(Picker, out NetworkObject networkObject))
        {
            _lastPicker = networkObject.GetComponent<Player>();
            if (_lastPicker == null) return;

            _collider.enabled = false;
            SetHighlight(false);
        }
        else if(_lastPicker != null)
        {
            SetVisibility(true);
            _collider.enabled = true;
        }
    }

    public virtual void Drop()
    {
        RPC_RequestDrop();
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void RPC_RequestDrop()
    {
        _isPickedUp.Value = false;
        _picker.Value = -1;

        _rb.isKinematic = false;
        _collider.enabled = true;
        _netTransform.enabled = true;
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
}
