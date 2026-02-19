using Coffee.UIExtensions;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ObjectiveUI: MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _descriptionTMP;
    [SerializeField] UIParticle _initializationVfx;
    [SerializeField] UIParticle _progressVfx;
    [SerializeField] UIParticle _completionVfx;
    public ObjectiveType ObjectiveType { get; private set; }

    public event Action<ObjectiveUI> OnDestroy;
    public void Init(ObjectiveType objectiveType, string description)
    {
        ObjectiveType = objectiveType;

        _descriptionTMP.text = description;

        _initializationVfx.Play();
    }

    public void HandleUpdateObjective(string description)
    {
        _descriptionTMP.text = description;

        _progressVfx.Play();
    }

    public void HandleCompleteObjective()
    {
        _descriptionTMP.fontStyle = FontStyles.Strikethrough;

        Color color = Color.gray;
        color.a = 0.3f;

        _descriptionTMP.color = color;

        _completionVfx.Play();
    }
}