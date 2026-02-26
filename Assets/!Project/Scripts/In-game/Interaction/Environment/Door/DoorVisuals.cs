using DG.Tweening;
using UnityEngine;

public class DoorVisuals : MonoBehaviour
{
    [SerializeField] Transform _door;

    [SerializeField] float _openDuration = 1f;
    [SerializeField] float _closeDuration = 1f;

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
            _door.DOLocalRotate(new Vector3(0, -100, 0), _openDuration).SetEase(Ease.OutBack);
        }
        else
        {
            _door.DOLocalRotate(new Vector3(0, 0, 0), _closeDuration).SetEase(Ease.InSine);
        }

    }

    public void Shake()
    {
        _shakeAnim.Restart();
    }
}
