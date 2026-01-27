using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] GameObject _activeVisuals;
    [SerializeField] GameObject _inactiveVisuals;
    bool _state = false;
    public bool State => _state;

    [SerializeField] bool _toggleOnClick = true;

    public event Action<ToggleButton> OnToggle;

    public Button Button { get; private set; }

    private void Awake()
    {
        Button = GetComponent<Button>();
        if (_toggleOnClick)
        {
            Button.onClick.AddListener(Toggle);
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

    void HandleStateChange(bool state)
    {
        if(_state)
        {
            _activeVisuals.SetActive(true);
            _inactiveVisuals.SetActive(false);
        }
        else
        {
            _activeVisuals.SetActive(false);
            _inactiveVisuals.SetActive(true);
        }

        OnToggle?.Invoke(this);
    }
}
