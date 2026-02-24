using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RangeSelectorUI : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] int _sliderMaxValue;
    [SerializeField] TMP_InputField _input;
    [SerializeField] string _inputPostfix;

    public int Value { get; private set; } = 0;
    public event Action<int> OnValueChanged;
    public event Action OnValueChangedTrigger;
    private void Awake()
    {
        _slider.maxValue = _sliderMaxValue;
    }
    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(HandleSliderValueChange);
        _input.onValueChanged.AddListener(HandleInputValueChange);
        _input.onSelect.AddListener(HandleInputFocus);
        _input.onDeselect.AddListener(HandleInputUnfocus);
    }

    private void OnDisable()
    {

        _slider.onValueChanged.RemoveListener(HandleSliderValueChange);
        _input.onValueChanged.RemoveListener(HandleInputValueChange);
        _input.onSelect.RemoveListener(HandleInputFocus);
    }

    void HandleSliderValueChange(float value)
    {
        SetValue(Mathf.RoundToInt(value));
    }

    void HandleInputFocus(string selectedValue)
    {
        _input.text = selectedValue.ToString();
    }

    void HandleInputUnfocus(string selectedValue)
    {
        _input.text = $"{Value}{_inputPostfix}";
    }

    void HandleInputValueChange(string value)
    {
        if (!_input.isFocused) return;

        if (int.TryParse(value, out int result))
        {
            SetValue(result);
        }
    }

    public void SetValue(int value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
        OnValueChangedTrigger?.Invoke();

        if (!_input.isFocused)
        {
            _input.text = $"{Value}{_inputPostfix}";
        }
        else
        {
            _slider.value = Value;
        }
    }
}
