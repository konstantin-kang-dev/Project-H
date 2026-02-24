using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputSelector : MonoBehaviour
{
    [SerializeField] TMP_InputField _input;

    public string Value {  get; private set; } = string.Empty;
    bool _isFocused = false;

    public event Action<string> OnValueChanged;
    public event Action OnValueChangedTrigger;
    private void OnEnable()
    {
        _input.onSelect.AddListener(HandleInputFocus);
        _input.onDeselect.AddListener(HandleInputUnfocus);
    }

    private void OnDisable()
    {
        _input.onSelect.RemoveListener(HandleInputFocus);
        _input.onDeselect.RemoveListener(HandleInputUnfocus);
    }

    private void Update()
    {
        if (_isFocused)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None) continue;
                if (keyboard[key].wasPressedThisFrame)
                {
                    HandleKeyPressed(key);
                    break;
                }
            }
        }

    }
    void HandleInputFocus(string selectedValue)
    {
        _isFocused = true;
    }

    void HandleInputUnfocus(string selectedValue)
    {
        _isFocused = false;
    }

    void HandleKeyPressed(Key key)
    {
        SetValue(key.ToString());
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetValue(string value)
    {
        Value = value;

        _input.text = value;

        OnValueChanged?.Invoke(Value);
        OnValueChangedTrigger?.Invoke();
    }
}
