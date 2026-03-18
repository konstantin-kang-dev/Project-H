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
    [SerializeField] Image _hoverBg;
    [SerializeField] Image _defaultBg;

    [SerializeField] TextMeshProUGUI _tmp;
    [SerializeField] Color _tmpHoverColor;
    [SerializeField] Image _icon;

    Color _initialTmpColor;
    Color _initialIconColor;

    [SerializeField] float _hoverAnimationDuration = 0.15f;
    void Awake()
    {
        _btn = GetComponent<Button>();
        if(_tmp != null)
        {
            _initialTmpColor = _tmp.color;
        }

        if (_icon != null)
        {
            _initialIconColor = _icon.color;
        }
    }

    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_btn.interactable) return;

        Sequence pointerDownAnim = DOTween.Sequence();

        if(_container != null)
        {
            Tween scaleTween = _container.DOScale(new Vector3(0.9f, 0.9f, 1), _hoverAnimationDuration);
            pointerDownAnim.Join(scaleTween);
        }
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

        Tween backgroundFadeTween = _hoverBg.DOFade(1f, _hoverAnimationDuration);
        hoverEnterAnim.Join(backgroundFadeTween);

        Tween borderFadeTween = _defaultBg.DOFade(0f, _hoverAnimationDuration);
        hoverEnterAnim.Join(borderFadeTween);

        if(_tmp != null)
        {
            Tween tmpColorFade = _tmp.DOColor(_tmpHoverColor, _hoverAnimationDuration);
            hoverEnterAnim.Join(tmpColorFade);
        }


        if(_icon != null)
        {
            Tween iconColorFade = _icon.DOColor(_tmpHoverColor, _hoverAnimationDuration);
            hoverEnterAnim.Join(iconColorFade);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Sequence hoverExitAnim = DOTween.Sequence();

        Tween backgroundFadeTween = _hoverBg.DOFade(0f, _hoverAnimationDuration);
        hoverExitAnim.Join(backgroundFadeTween);

        Tween borderFadeTween = _defaultBg.DOFade(1f, _hoverAnimationDuration);
        hoverExitAnim.Join(borderFadeTween);

        if(_tmp != null)
        {
            Tween tmpColorFade = _tmp.DOColor(_initialTmpColor, _hoverAnimationDuration);
            hoverExitAnim.Join(tmpColorFade);
        }

        if (_icon != null)
        {
            Tween iconColorFade = _icon.DOColor(_initialTmpColor, _hoverAnimationDuration);
            hoverExitAnim.Join(iconColorFade);
        }
    }

}
