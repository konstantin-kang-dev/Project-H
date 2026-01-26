using DG.Tweening;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BasicWindowVisuals : MonoBehaviour
{
    CanvasGroup _canvasGroup;
    [SerializeField] WindowAnimationType _inAnimationType;
    [SerializeField] WindowAnimationType _outAnimationType;
    [SerializeField] float _inAnimationDuration = 0.3f;
    [SerializeField] float _inAnimationDelay = 0f;
    [SerializeField] float _outAnimationDuration = 0.3f;
    [SerializeField] float _outAnimationDelay = 0f;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    public void ProcessInAnimation(bool doInstantly = false)
    {
        Sequence sequence = GetSequence(_inAnimationType, WindowAnimationDirection.In);
        sequence.SetDelay(_inAnimationDelay);

        sequence.Play(); 
        if (doInstantly)
        {
            sequence.Complete(true);
        }
    }

    public void ProcessOutAnimation(bool doInstantly = false)
    {
        Sequence sequence = GetSequence(_outAnimationType, WindowAnimationDirection.Out);
        sequence.SetDelay(_outAnimationDelay);

        sequence.Play();
        if(doInstantly)
        {
            sequence.Complete(true);
        }
    }

    Sequence GetSequence(WindowAnimationType animationType, WindowAnimationDirection animationDirection)
    {
        Sequence resultSequence = DOTween.Sequence();
        resultSequence.Pause();

        _canvasGroup.interactable = false;

        resultSequence.OnComplete(() =>
        {
            _canvasGroup.interactable = true;
        });

        switch (animationType)
        {
            case WindowAnimationType.Fade:

                switch (animationDirection)
                {
                    case WindowAnimationDirection.In:
                        
                        Tween groupFadeIn = _canvasGroup.DOFade(1, _inAnimationDuration).From(0);
                        resultSequence.Join(groupFadeIn);

                        break;
                    case WindowAnimationDirection.Out:
                        Tween groupFadeOut = _canvasGroup.DOFade(0, _outAnimationDuration).From(1);
                        resultSequence.Join(groupFadeOut);

                        break;
                    default:
                        break;
                }

                break;
            case WindowAnimationType.SwipeLeft:


                switch (animationDirection)
                {
                    case WindowAnimationDirection.In:

                        Tween groupSwipeLeft1 = _canvasGroup.transform.DOLocalMoveX(0f, _inAnimationDuration).From(200f);
                        resultSequence.Join(groupSwipeLeft1);

                        Tween groupFadeIn = _canvasGroup.DOFade(1, _inAnimationDuration).From(0);
                        resultSequence.Join(groupFadeIn);

                        break;
                    case WindowAnimationDirection.Out:

                        Tween groupSwipeLeft2 = _canvasGroup.transform.DOLocalMoveX(-200f, _outAnimationDuration).From(0);
                        resultSequence.Join(groupSwipeLeft2);

                        Tween groupFadeOut = _canvasGroup.DOFade(0, _outAnimationDuration).From(1);
                        resultSequence.Join(groupFadeOut);

                        break;
                    default:
                        break;
                }

                break;
            case WindowAnimationType.SwipeRight:

                switch (animationDirection)
                {
                    case WindowAnimationDirection.In:

                        Tween groupSwipeRight1 = _canvasGroup.transform.DOLocalMoveX(0f, _inAnimationDuration).From(-200f);
                        resultSequence.Join(groupSwipeRight1);

                        Tween groupFadeIn = _canvasGroup.DOFade(1, _inAnimationDuration).From(0);
                        resultSequence.Join(groupFadeIn);

                        break;
                    case WindowAnimationDirection.Out:

                        Tween groupSwipeRight2 = _canvasGroup.transform.DOLocalMoveX(200f, _outAnimationDuration).From(0);
                        resultSequence.Join(groupSwipeRight2);

                        Tween groupFadeOut = _canvasGroup.DOFade(0, _outAnimationDuration).From(1);
                        resultSequence.Join(groupFadeOut);

                        break;
                    default:
                        break;
                }

                break;
            case WindowAnimationType.SwipeDown:

                switch (animationDirection)
                {
                    case WindowAnimationDirection.In:

                        Tween groupSwipeDown1 = _canvasGroup.transform.DOLocalMoveY(200f, _inAnimationDuration).From(0);
                        resultSequence.Join(groupSwipeDown1);

                        Tween groupFadeIn = _canvasGroup.DOFade(1, _inAnimationDuration).From(0);
                        resultSequence.Join(groupFadeIn);

                        break;
                    case WindowAnimationDirection.Out:

                        Tween groupSwipeDown2 = _canvasGroup.transform.DOLocalMoveY(-200f, _outAnimationDuration).From(0);
                        resultSequence.Join(groupSwipeDown2);

                        Tween groupFadeOut = _canvasGroup.DOFade(0, _outAnimationDuration).From(1);
                        resultSequence.Join(groupFadeOut);

                        break;
                    default:
                        break;
                }

                break;
            case WindowAnimationType.PopUp:
                break;
            default:
                break;
        }

        return resultSequence;
    }

}