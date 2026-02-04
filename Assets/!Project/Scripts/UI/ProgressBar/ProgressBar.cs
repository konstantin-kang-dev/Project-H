using System;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public float Progress { get; private set; } = 0f;
    public int ProgressPercent => Mathf.RoundToInt(Progress * 100);

    IProgressBarVisuals _progressBarVisuals;

    public event Action<float> OnProgressChanged;
    public event Action OnFullFilled;

    private void Start()
    {
        if(transform.TryGetComponent<IProgressBarVisuals>(out _progressBarVisuals))
        {
            _progressBarVisuals.Init();
        }
    }

    public void SetProgress(float progress)
    {
        Progress = progress;
        OnProgressChanged?.Invoke(progress);

        _progressBarVisuals?.UpdateProgress(progress, false);
        if(Progress >= 1f)
        {
            OnFullFilled?.Invoke();
        }
    }

    public void ResetProgress()
    {
        Progress = 0f;
        OnProgressChanged?.Invoke(Progress);

        _progressBarVisuals?.UpdateProgress(Progress, true);
    }
}
