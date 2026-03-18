using Coffee.UIExtensions;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ObjectiveUI: MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _descriptionTMP;
    [SerializeField] ProgressBar _progressBar;
    [SerializeField] UIParticle _initializationVfx;
    [SerializeField] UIParticle _progressVfx;
    [SerializeField] UIParticle _completionVfx;

    int _totalObjectivesAmount = 0;
    public ObjectiveType ObjectiveType { get; private set; }

    public event Action<ObjectiveUI> OnDestroy;
    public void Init(ObjectiveType objectiveType, int totalObjectivesAmount, string description)
    {
        ObjectiveType = objectiveType;

        _descriptionTMP.text = description;

        _initializationVfx.Play();

        _progressBar.SetProgress(0, true);
        _totalObjectivesAmount = totalObjectivesAmount;
    }

    public void HandleUpdateObjective(int collectedAmount, string description)
    {
        _descriptionTMP.text = description;

        _progressVfx.Play();

        float progress = (float)collectedAmount / (float)_totalObjectivesAmount;
        _progressBar.SetProgress(progress, false);
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