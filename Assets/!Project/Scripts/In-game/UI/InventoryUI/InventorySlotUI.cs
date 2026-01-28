using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] RectTransform _container;
    [SerializeField] Image _slotBgImage;
    [SerializeField] Image _itemImage;

    Sequence _selectAnim;
    Sequence _deselectAnim;
    private void Awake()
    {
        _selectAnim = DOTween.Sequence();
        _selectAnim.Pause();
        _selectAnim.SetAutoKill(false);

        Tween moveUpTween = _container.DOAnchorPosY(15f, 0.2f).SetEase(Ease.InOutSine);
        _selectAnim.Join(moveUpTween);

        Tween bgFadeInTween = _slotBgImage.DOFade(1f, 0.2f);
        _selectAnim.Join(bgFadeInTween);

        _deselectAnim = DOTween.Sequence();
        _deselectAnim.Pause();
        _deselectAnim.SetAutoKill(false);

        Tween moveDownTween = _container.DOAnchorPosY(0f, 0.2f).SetEase(Ease.InOutSine);
        _deselectAnim.Join(moveDownTween);

        Tween bgFadeOutTween = _slotBgImage.DOFade(0.2f, 0.2f);
        _deselectAnim.Join(bgFadeOutTween);

    }

    public void Select()
    {
        _selectAnim.Restart();
    }

    public void Deselect()
    {
        _deselectAnim.Restart();
    }
}
