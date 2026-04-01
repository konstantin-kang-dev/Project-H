using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicPopup : MonoBehaviour, IPopup
{
    [SerializeField] BasicWindowVisuals _visuals;
    [SerializeField] TextMeshProUGUI _messageTMP;
    [SerializeField] Button _continueBtn;
    public bool IsVisible { get; private set; } = false;

    public void SetVisibility(bool visible, bool doInstantly = false)
    {
        if (visible)
        {
            _visuals.ProcessInAnimation(doInstantly);
        }
        else
        {
            _visuals.ProcessOutAnimation(doInstantly);
        }
    }
    public void SetMessage(string message)
    {
        _messageTMP.text = message;
    }

    void Awake()
    {
        _continueBtn.onClick.AddListener(()=> SetVisibility(false));
    }
}
