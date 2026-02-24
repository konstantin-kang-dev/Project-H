using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ToggleGroup : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<int, TextMeshProUGUI> _toggleTmps = new Dictionary<int, TextMeshProUGUI> ();
    [SerializeField] List<ToggleButton> _toggleButtons = new List<ToggleButton>();

    int _currentValue = 0;
    public int CurrentValue => _currentValue;

    public event Action<int> OnToggle;

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        if (_toggleButtons.Count == 0) throw new System.Exception($"[ToggleGroup] You forgot to set up toggle buttons.");

        foreach (var button in _toggleButtons)
        {
            ToggleButton toggleButton = button;

            toggleButton.OnToggle += HandleToggle;
        }

    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if(IsInitialized) return;

        IsInitialized = true;

    }

    public void ResetValues()
    {
        _toggleButtons.ToList()[0].SetState(true);
    }

    public void SetValue(int value)
    {
        if(value > _toggleButtons.Count - 1)
        {
            throw new System.Exception($"[ToggleGroup] Value is out of bounds.");
        }

        ToggleButton targetBtn = _toggleButtons.FirstOrDefault((x)=> _toggleButtons.IndexOf(x) == value);
        targetBtn.SetState(true);
        HandleToggle(targetBtn);
    }

    public void SetButtonsVisibility(bool visible)
    {
        foreach (var button in _toggleButtons)
        {
            button.SetVisibility(visible);
        }
    }

    void HandleToggle(ToggleButton toggleButton)
    {
        if (!IsInitialized) return;
        if (!toggleButton.State) return;

        DisableAllWithException(toggleButton);
        _currentValue = _toggleButtons.IndexOf(toggleButton);

        if (_toggleTmps.ContainsKey(_currentValue))
        {
            _toggleTmps[_currentValue].gameObject.SetActive(true);
        }

        OnToggle?.Invoke(_currentValue);
    }

    void DisableAllWithException(ToggleButton excToggleButton)
    {
        if (!IsInitialized) return;

        for (int i = 0; i < _toggleButtons.Count; i++)
        {
            ToggleButton toggleButton = _toggleButtons[i];
            if (toggleButton == excToggleButton) continue;

            if (_toggleTmps.ContainsKey(i))
            {
                _toggleTmps[i].gameObject.SetActive(false);
            }

            toggleButton.SetState(false);
        }
    }
}
