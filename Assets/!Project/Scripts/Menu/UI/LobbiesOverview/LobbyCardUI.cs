using UnityEngine;
using System.Collections;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class LobbyCardUI : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Dictionary<DifficultyType, GameObject> _difficultyTexts = new Dictionary<DifficultyType, GameObject>();
    [SerializeField] TextMeshProUGUI _hostNameTMP;
    [SerializeField] TextMeshProUGUI _playersTMP;
    [SerializeField] Button _btn;
    [SerializeField] Image _bg;

    public void Init(LobbyData lobbyData)
    {
        foreach (var textBlock in _difficultyTexts)
        {
            if (textBlock.Value.activeInHierarchy) textBlock.Value.SetActive(false);
        }

        _difficultyTexts[lobbyData.ChosenDifficulty].SetActive(true);
        _hostNameTMP.text = lobbyData.HostName;
        _playersTMP.text = $"{lobbyData.CurrentPlayers}/{lobbyData.MaxPlayers}";

        _btn.onClick.AddListener(() =>
        {
            NetworkGameManager.Instance.JoinLobby(lobbyData);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _bg.DOFade(0.3f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _bg.DOFade(0.1f, 0.15f);
    }
}