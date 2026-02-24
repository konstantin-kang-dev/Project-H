using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValueSelectorUI : SerializedMonoBehaviour
{
    
    [SerializeField] Button _leftBtn;
    [SerializeField] Button _rightBtn;
    [SerializeField] TextMeshProUGUI _valueTmp;
    [SerializeField] Dictionary<int, string> _values = new Dictionary<int, string>();

    int _selectedValue = 0;

    public int SelectedValue => _selectedValue;

    public event Action<int> OnValueChanged;
    public event Action OnValueChangedTrigger;

    private void OnEnable()
    {
        _leftBtn.onClick.AddListener(HandleLeftBtn);
        _rightBtn.onClick.AddListener(HandleRightBtn);
    }

    private void OnDisable()
    {
        _leftBtn.onClick.RemoveListener(HandleLeftBtn);
        _rightBtn.onClick.RemoveListener(HandleRightBtn);
    }

    public void SetValues(Dictionary<int, string> values)
    {
        _values = values;
        SetValue(SelectedValue);
    }

    void HandleLeftBtn()
    {
        SetValue(ProjectUtils.GetNextIndex(_selectedValue, _values.Count - 1, false));
    }

    void HandleRightBtn()
    {
        SetValue(ProjectUtils.GetNextIndex(_selectedValue, _values.Count - 1, true));
    }

    public void SetValue(int value)
    {
        _selectedValue = value;

        _valueTmp.text = _values[_selectedValue];
        OnValueChanged?.Invoke(_selectedValue);
        OnValueChangedTrigger?.Invoke();
    }
}
