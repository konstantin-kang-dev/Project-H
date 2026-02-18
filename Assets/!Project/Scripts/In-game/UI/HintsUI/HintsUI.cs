using DG.Tweening;
using TMPro;
using UnityEngine;

public class HintsUI : MonoBehaviour
{
    public static HintsUI Instance;

    [SerializeField] Canvas _canvas;
    RectTransform _canvasRect;

    [SerializeField] RectTransform _hintBlock;
    [SerializeField] CanvasGroup _hintBlockCanvasGroup;
    [SerializeField] TextMeshProUGUI _hintTMP;

    [SerializeField] GameObject _requirementsHintBlock;
    [SerializeField] TextMeshProUGUI _requirementsHintTMP;

    bool _isHintActive = false;

    Sequence _showAnim;
    Sequence _hideAnim;
    void Awake()
    {
        _canvasRect = _canvas.GetComponent<RectTransform>();
        Instance = this;
    }

    void ShowHint(string hintText, string requirementsText = "")
    {
        _hintTMP.text = hintText;
        if (!string.IsNullOrEmpty(requirementsText))
        {
            _requirementsHintBlock.SetActive(true);
            _requirementsHintTMP.text = requirementsText;
        }
        else
        {
            _requirementsHintBlock.SetActive(false);
        }

        if (_isHintActive) return;

        if(_hideAnim != null)
        {
            _hideAnim.Kill();
            _hideAnim = null;
        }

        _showAnim = DOTween.Sequence();

        Tween fadeTween = _hintBlockCanvasGroup.DOFade(1f, 0.2f);
        _showAnim.Join(fadeTween);

        Tween scaleTween = _hintBlock.DOScaleX(1f, 0.2f).From(0f).SetEase(Ease.OutBack);
        _showAnim.Join(scaleTween);

        _isHintActive = true;
    }

    void HideHint()
    {
        if (!_isHintActive) return;

        if (_showAnim != null)
        {
            _showAnim.Kill();
            _showAnim = null;
        }

        _hideAnim = DOTween.Sequence();

        Tween fadeTween = _hintBlockCanvasGroup.DOFade(0f, 0.2f);
        _hideAnim.Join(fadeTween);

        Tween scaleTween = _hintBlock.DOScaleX(0f, 0.2f).SetEase(Ease.OutExpo);
        _hideAnim.Join(scaleTween);

        _isHintActive = false;
    }

    public void SetHint(IHintable hintable)
    {
        if (hintable == null)
        {
            HideHint();
            return;
        }

        Vector3 hintPos = hintable.HintPoint.position;
        string hintText = hintable.HintText;
        string requirementsText = hintable.RequirementsHintText;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(hintPos);

        //_hintBlock.anchoredPosition = screenPos - new Vector2(Screen.width / 2f, Screen.height / 2f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
     _canvasRect,
     screenPos,
     _canvas.worldCamera,
     out Vector2 localPoint
        );

        _hintBlock.anchoredPosition = localPoint;
        ShowHint(hintText, requirementsText);

    }
}
