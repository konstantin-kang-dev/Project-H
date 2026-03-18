using System;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public float Progress { get; private set; } = 0f;
    public int ProgressPercent => Mathf.RoundToInt(Progress * 100);
    public bool IsFullfilled => Progress == 1;

    IProgressBarVisuals _progressBarVisuals;

    public event Action<float> OnProgressChanged;
    public event Action OnFullFilled;

    private void Awake()
    {
        if(transform.TryGetComponent<IProgressBarVisuals>(out _progressBarVisuals))
        {
            _progressBarVisuals.Init();
        }
    }

    public void SetProgress(float progress, bool doInstantly)
    {
        Progress = progress;
        Progress = Mathf.Clamp(Progress, 0f, 1f);

        OnProgressChanged?.Invoke(Progress);

        _progressBarVisuals?.UpdateProgress(Progress, doInstantly);
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
