using DG.Tweening;
using ModestTree;
using System;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public bool IsOpened { get; private set; } = false;

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Transform _progressBarBlock;
    [SerializeField] TextMeshProUGUI _messageTMP;
    string _defaultMessage = "Connecting to the server...";

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

            SetLoadingProgress(0f, true);

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

    public void SetMessage(string message)
    {
        if (message.IsEmpty())
        {
            _messageTMP.text = _defaultMessage;
        }
        else
        {
            _messageTMP.text = message;
        }
    }

    public void SetLoadingProgress(float progress, bool doInstantly)
    {
        _progressBar.SetProgress(progress, doInstantly);

    }

    void HandleStart()
    {
        SetLoadingProgress(0.15f, false);
    }

    void HandleComplete()
    {
        SetVisibility(false);
        OnComplete?.Invoke();
    }
}
