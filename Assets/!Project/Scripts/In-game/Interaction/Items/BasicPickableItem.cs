using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Modules.Rendering.Outline;
using System.Collections.Generic;
using UnityEngine;

public class BasicPickableItem : NetworkBehaviour, IPickable, IHintable, IOutlinable
{
    [field: SerializeField] public ItemConfig ItemConfig { get; private set; }
    public Transform Transform { get; private set; }
    public int ItemObjectId { get; private set; } = -1;

    [field: SerializeField] public Transform HintPoint {  get; private set; }
    [field: SerializeField] public InputActionType InputActionHint {  get; private set; }
    [field: SerializeField] public string HintText {  get; private set; }
    [field: SerializeField] public string RequirementsHintText { get; private set; }

    [SerializeField] GameObject _container;
    protected NetworkTransform _netTransform;

    protected Rigidbody _rb;
    [SerializeField] protected Collider _physicsCollider;
    [SerializeField] protected Collider _triggerCollider;

    protected readonly SyncVar<bool> _isPickedUp = new SyncVar<bool>();
    public bool IsPickedUp => _isPickedUp.Value;

    protected readonly SyncVar<bool> _interactionState = new SyncVar<bool>();

    [field: SerializeField] protected InteractableObjectAudioService _audioService;
    [field: SerializeField] public List<OutlineComponent> Outlines { get; private set; } = new List<OutlineComponent>();

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
        SetOutlineVisibility(false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _interactionState.OnChange += CLIENT_HandleInteractState;

        ItemObjectId = ObjectId;

        _rb.isKinematic = true;
        SetColliders(true);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _isPickedUp.Value = false;

        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
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

        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.None;
        SetColliders(false);
        _netTransform.enabled = false;
        RPC_NotifyPickUp();
    }

    [ObserversRpc]
    void RPC_NotifyPickUp()
    {
        _audioService.Play(InteractableObjectAudioType.PickUp);
    }

    [Server]
    public virtual void SERVER_Drop()
    {
        _isPickedUp.Value = false;

        _rb.isKinematic = false; 
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        if (asServer) return;

        if (next)
        {
            _audioService.Play(InteractableObjectAudioType.InteractionStateActive);
        }
        else
        {
            _audioService.Play(InteractableObjectAudioType.InteractionStateInactive);
        }
    }

    public virtual void SetOutlineVisibility(bool value)
    {
        foreach (var outline in Outlines)
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

    [ObserversRpc]
    void RPC_NotifyCollision(float speed)
    {
        float volume = Mathf.InverseLerp(0.5f, 5f, speed);
        _audioService.Play(InteractableObjectAudioType.DropCollision, volume);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServerStarted) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < 0.1f) return;
        RPC_NotifyCollision(speed);
    }
}
