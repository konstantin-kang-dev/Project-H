using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] Image _backgroundImage;
    [SerializeField] Image _borderImage;

    [SerializeField] TextMeshProUGUI _tmp;
    [SerializeField] Color _tmpHoverColor;
    Color _initialTmpColor;

    [SerializeField] float _hoverAnimationDuration = 0.15f;
    void Awake()
    {
        _initialTmpColor = _tmp.color;
    }

    void Update()
    {

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sequence hoverEnterAnim = DOTween.Sequence();

        Tween backgroundFadeTween = _backgroundImage.DOFade(1f, _hoverAnimationDuration);
        hoverEnterAnim.Join(backgroundFadeTween);

        Tween tmpColorFade = _tmp.DOColor(_tmpHoverColor, _hoverAnimationDuration);
        hoverEnterAnim.Join(tmpColorFade);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Sequence hoverExitAnim = DOTween.Sequence();

        Tween backgroundFadeTween = _backgroundImage.DOFade(0f, _hoverAnimationDuration);
        hoverExitAnim.Join(backgroundFadeTween);

        Tween tmpColorFade = _tmp.DOColor(_initialTmpColor, _hoverAnimationDuration);
        hoverExitAnim.Join(tmpColorFade);
    }
}
