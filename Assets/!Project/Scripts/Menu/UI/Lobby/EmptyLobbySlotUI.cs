using UnityEngine;
using UnityEngine.UI;

public class EmptyLobbySlotUI : MonoBehaviour
{
    [SerializeField] Button _btn;

    private void OnEnable()
    {
        _btn.onClick.AddListener(HandleBtnClick);
    }

    private void OnDisable()
    {

        _btn.onClick.RemoveListener(HandleBtnClick);
    }

    void HandleBtnClick() 
    {

    }
}
