using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _ownerTMP;
    [SerializeField] TextMeshProUGUI _messageTMP;
    [SerializeField] Transform _container;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Image _bg;

    Sequence _appearanceAnim;
    public void Init(ChatMessageData messageData)
    {
        _ownerTMP.text = $"[{messageData.OwnerPlayerName}] : ";
        _messageTMP.text = messageData.Message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_container.GetComponent<RectTransform>());
        SetVisibility(false, true);
    }

    public void SetVisibility(bool visible, bool doInstantly = false)
    {
        if(_appearanceAnim != null)
        {
            _appearanceAnim.Kill();
        }

        _appearanceAnim = DOTween.Sequence();
        if (visible)
        {
            _container.gameObject.SetActive(true);
            Tween fadeInAnim = _canvasGroup.DOFade(1f, 0.2f).From(0f);
            _appearanceAnim.Join(fadeInAnim);

            Tween shineInAnim = _bg.DOFade(1f, 0.5f).From(0f);
            _appearanceAnim.Join(shineInAnim);
            Tween shineOutAnim = _bg.DOFade(0f, 0.5f);
            _appearanceAnim.Append(shineOutAnim);
        }
        else
        {
            Tween fadeOutAnim = _canvasGroup.DOFade(0f, 0.2f).OnComplete(() => _container.gameObject.SetActive(false));
            _appearanceAnim.Join(fadeOutAnim);
        }

        if (doInstantly)
        {
            _appearanceAnim.Complete();
            _appearanceAnim = null;
        }

        Debug.Log($"[ChatMessageUI] Set visibility: {visible}");
    }
}
