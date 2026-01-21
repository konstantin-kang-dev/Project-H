using DG.Tweening;
using UnityEngine;

public class SwingingLamp : MonoBehaviour
{
    [SerializeField] Vector3 _startRot;
    [SerializeField] Vector3 _endRot;
    [SerializeField] float _oneWaySwingDuration = 2f;
    void Start()
    {
        Sequence sequence = DOTween.Sequence();

        Tween forwardTween = transform
            .DORotate(_endRot, _oneWaySwingDuration)
            .SetEase(Ease.InOutQuad);

        sequence.Append(forwardTween);

        Tween backwardTween = transform
            .DORotate(_startRot, _oneWaySwingDuration)
            .SetEase(Ease.InOutQuad);

        sequence.Append(backwardTween);
        sequence.Append(forwardTween);

        sequence.SetLoops(-1);

    }

    void Update()
    {
        
    }
}
