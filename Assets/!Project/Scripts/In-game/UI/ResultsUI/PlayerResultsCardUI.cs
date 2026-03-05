using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerResultsCardUI : MonoBehaviour
{
    [SerializeField] GameObject _activeVisuals;
    [SerializeField] GameObject _inactiveVisuals;

    [SerializeField] Image _playerAvatar;
    [SerializeField] TextMeshProUGUI _playerName;
    public void Init(NetworkPlayerData networkPlayerData)
    {
        Sprite avatarSprite = NetworkRoomManager.Instance.GetPlayerAvatar(networkPlayerData.ClientId);

        if(avatarSprite != null)
        {
            _playerAvatar.sprite = avatarSprite;
        }

        _playerName.text = networkPlayerData.PlayerName;
        SetAppearance(true);
    }

    public void Clear()
    {
        SetAppearance(false);
    }

    public void SetAppearance(bool visible)
    {
        _activeVisuals.SetActive(visible);
        _inactiveVisuals.SetActive(!visible);
    }
}