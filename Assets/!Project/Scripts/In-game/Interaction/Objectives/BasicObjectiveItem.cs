using System;
using UnityEngine;
using UnityEngine.VFX;

public class BasicObjectiveItem : BasicInteractable
{
    [SerializeField] VisualEffect _interactVfx;

    [SerializeField] protected Transform _model;
    public ObjectiveType ObjectiveType { get; private set; }

    public event Action<BasicObjectiveItem> OnObjectiveCollected;
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

        _model.gameObject.SetActive(!next);

    }
}
