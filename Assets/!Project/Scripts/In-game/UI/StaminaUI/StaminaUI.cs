using DG.Tweening;
using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] ProgressBar _progressBar;
    [SerializeField] CanvasGroup _canvasGroup;

    bool _isVisible = false;

    Tween _fadeAnim;
    private void Awake()
    {
        Player.OnLocalPlayerInitialized += ConnectToPlayer;
    }

    public void Init()
    {
        _progressBar.OnFullFilled += HandleProgressBarFullfilled;
        _progressBar.ResetProgress();
        _progressBar.SetProgress(1f, true);

    }

    void ConnectToPlayer(Player player)
    {
        player.PlayerController.PlayerStaminaService.OnStaminaUpdate += HandleUpdateStamina;
    }

    void HandleUpdateStamina(float stamina)
    {
        if (stamina == 1 && _progressBar.IsFullfilled) return;

        SetVisibility(true);
        _progressBar.SetProgress(stamina, false);
    }

    void HandleProgressBarFullfilled()
    {
        SetVisibility(false);
    }

    void SetVisibility(bool value)
    {
        if (value == _isVisible) return;

        if(_fadeAnim != null)
        {
            _fadeAnim.Kill();
            _fadeAnim = null;
        }

        if (value)
        {
            _fadeAnim = _canvasGroup.DOFade(1f, 0.7f);

        }
        else
        {
            _fadeAnim = _canvasGroup.DOFade(0f, 0.7f);
        }

        _isVisible = value;
    }

}
