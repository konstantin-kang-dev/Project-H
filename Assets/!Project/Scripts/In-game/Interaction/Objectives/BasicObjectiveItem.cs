using FishNet.Object;
using Modules.Rendering.Outline;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using static System.Net.Mime.MediaTypeNames;

public class BasicObjectiveItem : BasicInteractable, IOutlinable
{
    [field: SerializeField] public List<OutlineComponent> Outlines { get; private set; } = new List<OutlineComponent>();

    [SerializeField] ParticleSystem _interactVfx;

    [SerializeField] protected Transform _model;
    public ObjectiveType ObjectiveType { get; private set; }

    public event Action<BasicObjectiveItem> OnObjectiveCollected;
    protected override void Awake()
    {
        base.Awake();
        SetHighlight(false);
    }

    public void SetObjectiveType(ObjectiveType objectiveType)
    {
        ObjectiveType = objectiveType;
    }

    public override void SetAppearance(bool value)
    {
        base.SetAppearance(value);

        if (value)
        {
            if(_interactVfx != null)
            {
                _interactVfx.Play();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void RPC_RequestInteract(int pickableObjectId)
    {
        if (_toggleInteractionState)
        {
            _interactState.Value = !_interactState.Value;
        }
        else
        {
            _interactState.Value = true;
        }

        if (_interactState.Value)
        {
            OnObjectiveCollected?.Invoke(this);
        }
    }

    [Client]
    protected override void HandleInteractStateChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        base.HandleInteractStateChange(prev, next, asServer);

        _model.gameObject.SetActive(!next);
        if (_collider != null) _collider.enabled = !next;

    }
    public virtual void SetHighlight(bool value)
    {
        foreach (var outline in Outlines)
        {
            outline.enabled = value;
        }
    }
}
