using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    public static ResultsUI Instance;

    [SerializeField] BasicWindowVisuals _windowVisuals;
    [SerializeField] List<PlayerResultsCardUI> _playersCards = new List<PlayerResultsCardUI>();

    [SerializeField] TextMeshProUGUI _titleTMP;
    [SerializeField] Color _titleWinColor;
    [SerializeField] Color _titleLoseColor;

    [SerializeField] TextMeshProUGUI _sessionTimeTMP;
    [SerializeField] TextMeshProUGUI _difficultyTMP;
    [SerializeField] List<GameObject> _difficultyImages = new List<GameObject>();

    [SerializeField] Button _continueBtn;

    public event Action OnContinueBtn;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetVisibility(false);
    }

    public void SetVisibility(
        bool visible,
        bool isWin = false,
        List<NetworkPlayerData> players = null,
        DifficultyType difficulty = 0,
        float sessionTime = 0
        )
    {
        if (visible)
        {
            _windowVisuals.ProcessInAnimation();

            _titleTMP.text = isWin ? "ESCAPED" : "FAILED";
            _titleTMP.color = isWin ? _titleWinColor : _titleLoseColor;

            _sessionTimeTMP.text = ProjectUtils.FormatTime(sessionTime);

            for (var i = 0; i < _difficultyImages.Count; i++)
            {
                _difficultyImages[i].SetActive((int)difficulty == i);
            }

            _difficultyTMP.text = ProjectUtils.GetDifficultyString(difficulty);

            for (var i = 0; i < players.Count; i++)
            {
                _playersCards[i].Clear();   
                _playersCards[i].Init(players[i]);   
            }

            _continueBtn.onClick.AddListener(HandleContinueBtn);

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            _windowVisuals.ProcessOutAnimation();
        }
    }

    void HandleContinueBtn()
    {
        OnContinueBtn?.Invoke();
    }

}
