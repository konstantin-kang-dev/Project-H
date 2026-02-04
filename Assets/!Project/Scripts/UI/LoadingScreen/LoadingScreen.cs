using DG.Tweening;
using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public bool IsOpened { get; private set; } = false;

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Transform _progressBarBlock;

    [SerializeField] ProgressBar _progressBar;

    public event Action OnComplete;
    public void SetVisibility(bool visible)
    {
        Sequence animation = DOTween.Sequence();

        if (visible)
        {
            Tween fadeTween = _canvasGroup.DOFade(1f, 0.5f);
            animation.Join(fadeTween);
            Tween progressBarSlideTween = _progressBarBlock.DOLocalMoveX(0f, 0.5f).From(50f);
            animation.Join(progressBarSlideTween);

            animation.onComplete += HandleStart;

            _progressBar.ResetProgress();
            _progressBar.OnFullFilled += HandleComplete;
            _canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Tween fadeTween = _canvasGroup.DOFade(0f, 0.5f);
            animation.Join(fadeTween);

            _progressBar.OnFullFilled -= HandleComplete;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetLoadingProgress(float progress)
    {
        _progressBar.SetProgress(progress);

    }

    void HandleStart()
    {
        SetLoadingProgress(0.15f);
    }

    void HandleComplete()
    {
        SetVisibility(false);
        OnComplete?.Invoke();
    }
}
