using UnityEngine;

public interface IHintable
{
    Transform HintPoint { get; }
    InputActionType InputActionHint { get; }
    string HintText { get; }
    string RequirementsHintText { get; }
}
