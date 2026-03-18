using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LinearProgressBarVisuals : MonoBehaviour, IProgressBarVisuals
{
    [SerializeField] RectTransform _progressBlock;
    [SerializeField] float _smoothingDuration = 0.2f;
    float _initialWidth = 0f;

    Tween _growAnim;

    void Awake()
    {
        _initialWidth = _progressBlock.rect.width;
    }
    void OnDestroy()
    {
        UpdateProgress(1f, true);
    }
    public void Init()
    {

    }

    public void UpdateProgress(float progress, bool doInstantly)
    {
        float targetWidth = _initialWidth * progress;
        //Vector3 targetLocalScale = new Vector3(progress,  _progressBlock.localScale.y, _progressBlock.localScale.z);
        
        if (doInstantly)
        {
            //_progressBlock.localScale = targetLocalScale;
            _progressBlock.sizeDelta = new Vector2(targetWidth, _progressBlock.sizeDelta.y);
        }
        else
        {
            if (_growAnim != null) _growAnim.Kill();

            //_growAnim = _progressBlock.DOScale(targetLocalScale, _smoothingDuration);
            _growAnim = _progressBlock.DOSizeDelta(new Vector2(targetWidth, _progressBlock.sizeDelta.y), _smoothingDuration);
        }
    }
}