using Modules.Rendering.Outline;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class BasicObjectiveItem : BasicInteractable, IOutlinable
{
    [field: SerializeField] public List<OutlineComponent> Outlines { get; private set; } = new List<OutlineComponent>();

    [SerializeField] VisualEffect _interactVfx;

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

    protected override void HandleInteractStateChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        base.HandleInteractStateChange(prev, next, asServer);

        if (next)
        {
            OnObjectiveCollected?.Invoke(this);
        }

        gameObject.SetActive(!next);

    }
    public virtual void SetHighlight(bool value)
    {
        foreach (var outline in Outlines)
        {
            outline.enabled = value;
        }
    }
}
