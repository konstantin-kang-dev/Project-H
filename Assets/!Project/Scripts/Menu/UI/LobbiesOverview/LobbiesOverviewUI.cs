using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesOverviewUI : BasicCustomWindow
{
    [SerializeField] BasicWindowVisuals _visuals;
    [SerializeField] Button _backBtn;

    [SerializeField] Transform _scrollContainer;
    [SerializeField] LobbyCardUI _lobbyCardPrefab;
    [SerializeField] float _autoUpdateInterval = 1f;
    float _timer = 0f;

    bool _isVisible = false;

    [field: SerializeField] List<LobbyCardUI> _activeLobbyCardsUI = new List<LobbyCardUI>();

    void Awake()
    {
        for (int i = 0; i < _scrollContainer.childCount; i++)
        {
            Destroy(_scrollContainer.GetChild(i).gameObject);
        }
    }

    public void Init()
    {
        _backBtn.onClick.AddListener(HandleBackBtn);
    }

    public override void SetVisibility(bool visible, bool doInstantly)
    {
        base.SetVisibility(visible, doInstantly);

        if (visible)
        {
            LoadLobbies();
        }

        _timer = 0f;
        _isVisible = visible;
    }

    void FixedUpdate()
    {
        if (_isVisible)
        {
            _timer += Time.fixedDeltaTime;
            if(_timer >= _autoUpdateInterval)
            {
                _timer = 0f;
                LoadLobbies();
            }
        }
    }

    void LoadLobbies()
    {
        FirebaseManager.Instance.LoadLobbies(HandleLoadLobbies);
    }

    void HandleLoadLobbies(List<LobbyData> lobbiesData)
    {

        foreach (var lobbyCardUI in _activeLobbyCardsUI)
        {
            Destroy(lobbyCardUI.gameObject);
        }
        _activeLobbyCardsUI.Clear();

        Debug.Log($"[LobbiesOverview] HandleLoadLobbies 1");
        foreach (var lobbyData in lobbiesData)
        {
            SpawnLobbyCard(lobbyData);
            Debug.Log($"[LobbiesOverview] HandleLoadLobbies 2");
        }

        Debug.Log($"[LobbiesOverview] Updated lobbies list({_activeLobbyCardsUI.Count}).");

    }

    void SpawnLobbyCard(LobbyData lobbyData)
    {
        LobbyCardUI lobbyCardUI = Instantiate(_lobbyCardPrefab, _scrollContainer);
        lobbyCardUI.Init(lobbyData);
        _activeLobbyCardsUI.Add(lobbyCardUI);
    }

    void HandleBackBtn()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.MainMenu);
    }
}
