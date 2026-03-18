using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] List<GameObject> _activeVisuals;
    [SerializeField] List<GameObject> _inactiveVisuals;
    bool _state = false;
    public bool State => _state;
    bool _isVisible = true;

    [SerializeField] bool _toggleOnClick = true;
    [SerializeField] bool _clickOnlyActivates = false;

    public event Action<ToggleButton> OnToggle;

    public Button Button { get; private set; }

    private void Awake()
    {
        Button = GetComponent<Button>();
        if (_toggleOnClick)
        {
            Button.onClick.AddListener(HandleButtonClick);
        }
    }

    void HandleButtonClick()
    {
        if (_clickOnlyActivates)
        {
            if (!_state) Toggle();
        }
        else
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        _state = !_state;
        HandleStateChange(_state);
    }

    public void SetState(bool state)
    {
        _state = state;
        HandleStateChange(_state);
    }

    public void SetVisibility(bool visible)
    {
        _isVisible = visible;
        if (visible)
        {
            HandleStateChange(_state);
        }
        else
        {
            foreach (var go in _activeVisuals)
            {
                go.SetActive(false);
            }

            foreach (var go in _inactiveVisuals)
            {
                go.SetActive(false);
            }
        }
    }

    void HandleStateChange(bool state)
    {
        if (_isVisible)
        {
            if (_state)
            {
                foreach (var go in _activeVisuals)
                {
                    go.SetActive(true);
                }

                foreach (var go in _inactiveVisuals)
                {
                    go.SetActive(false);
                }
            }
            else
            {
                foreach (var go in _activeVisuals)
                {
                    go.SetActive(false);
                }

                foreach (var go in _inactiveVisuals)
                {
                    go.SetActive(true);
                }
            }
        }

        OnToggle?.Invoke(this);
    }
}
