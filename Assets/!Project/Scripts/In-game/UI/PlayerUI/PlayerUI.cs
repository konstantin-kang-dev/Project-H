using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Image _avatar;
    [SerializeField] TextMeshProUGUI _playerTMP;
    Tween _fadeAnim;

    bool _isActive = false;
    public void SetData(string playerName, Sprite avatarSprite)
    {
        _playerTMP.text = playerName;
        if(avatarSprite != null)
        {
            _avatar.sprite = avatarSprite;
        }
    }

    public void SetVisibility(bool visible)
    {
        if(visible)
        {
            _fadeAnim = _canvasGroup.DOFade(1f, 0.3f);
        }
        else
        {
            _fadeAnim = _canvasGroup.DOFade(0f, 0.3f);
        }

        _isActive = visible;
    }

    private void Update()
    {
        if(_isActive)
        {
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform.position);
            }
        }

    }
}
