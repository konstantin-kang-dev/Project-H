using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyPlayerUI : MonoBehaviour
{
    [SerializeField] List<Button> _playerModelChangeBtns = new List<Button>();
    [SerializeField] TextMeshProUGUI _nicknameTmp;

    [SerializeField] Image _profileAvatar;
    [SerializeField] Image _profileBg;
    [SerializeField] Color _profileReadyBgColor;
    [SerializeField] Color _profileNotReadyBgColor;

    public void BindActionsToModelChangeButtons(UnityAction leftBtnAction, UnityAction rightBtnAction)
    {
        _playerModelChangeBtns[0].onClick.AddListener(leftBtnAction);
        _playerModelChangeBtns[1].onClick.AddListener(rightBtnAction);
    }

    public void SetButtonsVisibility(bool visible)
    {
        foreach (var btn in _playerModelChangeBtns)
        {
            btn.gameObject.SetActive(visible);
        }
    }

    public void SetAvatarSprite(Sprite sprite)
    {
        if(sprite != null)
        {
            _profileAvatar.sprite = sprite;
        }
    }

    public void SetNicknameText(string nickname)
    {
        _nicknameTmp.text = nickname;
    }

    public void SetReadyAppearance(bool isReady)
    {
        _profileBg.color = isReady ? _profileReadyBgColor : _profileNotReadyBgColor;
    }
}