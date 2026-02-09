using UnityEngine;

public class InteractableShelf : BasicInteractable
{
    [SerializeField] InteractableShelfVisuals _shelfVisuals;
    public override void SetAppearance(bool value)
    {
        base.SetAppearance(value);

        _shelfVisuals.HandleStateChange(value);
    }
}
