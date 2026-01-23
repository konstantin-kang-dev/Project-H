using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    Button _btn;
    [SerializeField] Transform _container;
    [SerializeField] Image _backgroundImage;
    [SerializeField] Image _borderImage;

    [SerializeField] TextMeshProUGUI _tmp;
    [SerializeField] Color _tmpHoverColor;
    Color _initialTmpColor;

    [SerializeField] float _hoverAnimationDuration = 0.15f;
    void Awake()
    {
        _btn = GetComponent<Button>();
        _initialTmpColor = _tmp.color;
    }

    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_btn.interactable) return;

        Sequence pointerDownAnim = DOTween.Sequence();

        Tween scaleTween = _container.DOScale(new Vector3(0.9f, 0.9f, 1), _hoverAnimationDuration);
        pointerDownAnim.Join(scaleTween);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Sequence pointerUpAnim = DOTween.Sequence();

        Tween scaleTween = _container.DOScale(new Vector3(1f, 1f, 1), _hoverAnimationDuration);
        pointerUpAnim.Join(scaleTween);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_btn.interactable) return;

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
