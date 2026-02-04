using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LinearProgressBarVisuals : MonoBehaviour, IProgressBarVisuals
{
    [SerializeField] Transform _progressBlock;
    [SerializeField] float _smoothingDuration = 0.2f;

    Tween _growAnim;
    public void Init()
    {

    }

    public void UpdateProgress(float progress, bool doInstantly)
    {
        Vector3 targetLocalScale = new Vector3(progress,  _progressBlock.localScale.y, _progressBlock.localScale.z);

        if (doInstantly)
        {
            _progressBlock.localScale = targetLocalScale;
        }
        else
        {
            if (_growAnim != null) _growAnim.Kill();

            _growAnim = _progressBlock.DOScale(targetLocalScale, _smoothingDuration);
        }
    }
}