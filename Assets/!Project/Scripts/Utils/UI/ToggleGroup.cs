using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ToggleGroup : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<int, TextMeshProUGUI> _toggleTmps = new Dictionary<int, TextMeshProUGUI> ();
    [SerializeField] Dictionary<ToggleButton, int> _toggleButtons = new Dictionary<ToggleButton, int>();

    int _currentValue = 0;
    public int CurrentValue => _currentValue;

    public event Action<int> OnToggle;

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        if (_toggleButtons.Count == 0) throw new System.Exception($"[ToggleGroup] You forgot to set up toggle buttons.");

        foreach (var buttonBlock in _toggleButtons)
        {
            ToggleButton toggleButton = buttonBlock.Key;

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
        _toggleButtons.Keys.ToList()[0].SetState(true);
    }

    public void SetValue(int value)
    {
        if(value > _toggleButtons.Count - 1)
        {
            throw new System.Exception($"[ToggleGroup] Value is out of bounds.");
        }

        ToggleButton targetBtn = _toggleButtons.FirstOrDefault((x)=> x.Value == value).Key;
        targetBtn.SetState(true);
        HandleToggle(targetBtn);
    }

    public void SetButtonsVisibility(bool visible)
    {
        foreach (var button in _toggleButtons)
        {
            button.Key.SetVisibility(visible);
        }
    }

    void HandleToggle(ToggleButton toggleButton)
    {
        if (!IsInitialized) return;
        if (!toggleButton.State) return;

        DisableAllWithException(toggleButton);
        _currentValue = _toggleButtons[toggleButton];

        if (_toggleTmps.ContainsKey(_currentValue))
        {
            _toggleTmps[_currentValue].gameObject.SetActive(true);
        }

        OnToggle?.Invoke(_currentValue);
    }

    void DisableAllWithException(ToggleButton excToggleButton)
    {
        if (!IsInitialized) return;

        foreach (var buttonBlock in _toggleButtons)
        {
            ToggleButton toggleButton = buttonBlock.Key;
            if (toggleButton == excToggleButton) continue;

            if (_toggleTmps.ContainsKey(buttonBlock.Value))
            {
                _toggleTmps[buttonBlock.Value].gameObject.SetActive(false);
            }

            toggleButton.SetState(false);
        }
    }
}
