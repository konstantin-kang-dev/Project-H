using UnityEngine;

public interface IHintable
{
    Transform HintPoint { get; }
    string HintText { get; }
    string RequirementsHintText { get; }
}
