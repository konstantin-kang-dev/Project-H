using UnityEngine;

public class PopupsManager : MonoBehaviour
{
    public static PopupsManager Instance { get; private set; }
    [SerializeField] BasicPopup _popup;

    private void Awake()
    {
        _popup.SetVisibility(false, true);
        Instance = this;
    }

    public void ShowPopup(string message)
    {
        _popup.SetMessage(message);
        _popup.SetVisibility(true);
    }

}
