using DG.Tweening;
using UnityEngine;

public class DoorVisuals : MonoBehaviour
{
    [SerializeField] Transform _door;

    [SerializeField] float _openCloseDuration = 1f;

    Sequence _shakeAnim;
    void Awake()
    {
        _shakeAnim = DOTween.Sequence();
        _shakeAnim.Pause();
        _shakeAnim.SetAutoKill(false);

        Tween shakeTween = _door.DOShakeRotation(0.3f, new Vector3(0, 3f, 0), vibrato: 10)
        .SetEase(Ease.OutQuad);

        _shakeAnim.Join(shakeTween);

    }

    void Update()
    {
        
    }

    public void SetState(bool value)
    {
        if (value)
        { 
            _door.DORotate(new Vector3(0, -100, 0), _openCloseDuration).SetEase(Ease.OutBack);
        }
        else
        {
            _door.DORotate(new Vector3(0, 0, 0), _openCloseDuration).SetEase(Ease.InSine);
        }

    }

    public void Shake()
    {
        _shakeAnim.Restart();
    }
}
