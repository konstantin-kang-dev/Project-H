using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ObjectiveUI: MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _descriptionTMP;
    [SerializeField] ParticleSystem _progressVfx;
    [SerializeField] ParticleSystem _completionVfx;
    public ObjectiveType ObjectiveType { get; private set; }

    public event Action<ObjectiveUI> OnDestroy;
    public void Init(ObjectiveType objectiveType, string description)
    {
        ObjectiveType = objectiveType;
    }

    public void HandleUpdateObjective(string description)
    {
        _descriptionTMP.text = description;
    }

    public void HandleCompleteObjective()
    {
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }
}