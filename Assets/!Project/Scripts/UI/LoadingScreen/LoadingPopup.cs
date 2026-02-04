using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LoadingPopup : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Transform _container;
    [SerializeField] Transform _icon;

    Sequence _iconAnimation;

    private void Start()
    {
        _iconAnimation = DOTween.Sequence();

        Tween scaleOutTween = _icon.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.7f).From(Vector3.one);
        _iconAnimation.Append(scaleOutTween);

        Tween rotateTween = _icon.DORotate(new Vector3(0, 0, 60f), 1f).From(Vector3.zero);
        _iconAnimation.Append(rotateTween);

        Tween scaleInTween = _icon.DOScale(Vector3.one, 0.7f).From(new Vector3(1.2f, 1.2f, 1f));
        _iconAnimation.Append(scaleInTween);

        _iconAnimation.SetLoops(-1);
        _iconAnimation.SetAutoKill(false);
    }

    public void SetVisibility(bool visible)
    {
        Sequence animation = DOTween.Sequence();

        if (visible)
        {
            Tween fadeTween = _canvasGroup.DOFade(1f, 0.3f);
            animation.Join(fadeTween);

            Tween containerMoveTween = _container.DOLocalMoveY(0f, 0.3f).From(50f);
            animation.Join(containerMoveTween);
            _iconAnimation.Restart();
            _canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Tween fadeTween = _canvasGroup.DOFade(0f, 0.3f);
            animation.Join(fadeTween);

            Tween containerMoveTween = _container.DOLocalMoveY(-50f, 0.3f).From(0f);
            animation.Join(containerMoveTween);
            _iconAnimation.Pause();
            _canvasGroup.blocksRaycasts = false;

        }
    }
}