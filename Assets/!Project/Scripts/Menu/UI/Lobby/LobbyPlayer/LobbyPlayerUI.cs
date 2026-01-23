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
    [SerializeField] Image _readyIcon;

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

    public void SetNicknameText(string nickname)
    {
        _nicknameTmp.text = nickname;
    }

    public void SetReadyIconVisibility(bool visible)
    {
        _readyIcon.gameObject.SetActive(visible);
    }
}