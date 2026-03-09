using FishNet.Object.Synchronizing;
using UnityEngine;

public class Safe : BasicInteractable
{
    [SerializeField] ObjectiveType _requiredObjectiveToCollect;

    [SerializeField] DoorVisuals _doorVisuals;

    readonly SyncVar<bool> _isLocked = new SyncVar<bool>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        HintText = $"Collect all {ProjectUtils.CamelCaseToSpaced(_requiredObjectiveToCollect.ToString())}";
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        _isLocked.Value = true;

        ObjectivesManager.Instance.OnObjectiveCompleted += HandleObjectiveCompleted;
    }

    public override void Interact(IPickable pickableInHand)
    {

    }

    void HandleObjectiveCompleted(ObjectiveType type)
    {
        if(type == _requiredObjectiveToCollect)
        {
            _interactState.Value = true;
            Debug.Log($"[Safe] Unlocked! Objective collected: {type}");
        }
    }

    public override void SetAppearance(bool value)
    {
        base.SetAppearance(value);

        _doorVisuals.SetState(value);
        _collider.enabled = !value;
    }
}
