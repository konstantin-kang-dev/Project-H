using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] RectTransform _container;
    [SerializeField] CanvasGroup _canvasGroup;

    [SerializeField] Image _itemImage;
    Sequence _selectAnim;
    Sequence _deselectAnim;

    public IPickable Item { get; private set; }
    private void Awake()
    {

    }

    public void Select()
    {
        if(_deselectAnim != null)
        {
            _deselectAnim.Kill();
            _deselectAnim = null;
        }

        _selectAnim = DOTween.Sequence();

        Tween moveUpTween = _container.DOAnchorPosY(15f, 0.2f).SetEase(Ease.InOutSine);
        _selectAnim.Join(moveUpTween);

        Tween fadeInTween = _canvasGroup.DOFade(1f, 0.2f);
        _selectAnim.Join(fadeInTween);
    }

    public void Deselect()
    {
        if (_selectAnim != null)
        {
            _selectAnim.Kill();
            _selectAnim = null;
        }

        _deselectAnim = DOTween.Sequence();

        Tween moveDownTween = _container.DOAnchorPosY(0f, 0.2f).SetEase(Ease.InOutSine);
        _deselectAnim.Join(moveDownTween);

        Tween fadeOutTween = _canvasGroup.DOFade(0.3f, 0.2f);
        _deselectAnim.Join(fadeOutTween);
    }

    public void SetItem(IPickable item)
    {
        Item = item;
        _itemImage.sprite = Item.ItemConfig.InventoryIcon;
        _itemImage.gameObject.SetActive(true);
    }

    public void Clear()
    {
        Item = null;

        _itemImage.sprite = null;
        _itemImage.gameObject.SetActive(false);
    }
}
