using System;
using UnityEngine;
using UnityEngine.UI;

public class CheckBox : MonoBehaviour
{
    [SerializeField] Button _btn;
    [SerializeField] GameObject _activeStateAppearance;
    public bool Value { get; private set; } = false;

    public event Action<bool> OnValueChanged;
    public event Action OnValueChangedTrigger;
    private void OnEnable()
    {
        _btn.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        _btn.onClick.RemoveListener(HandleClick);
    }

    void HandleClick()
    {
        SetValue(!Value);
    }

    public void SetValue(bool state)
    {
        Value = state;

        if (_activeStateAppearance != null)
        {
            _activeStateAppearance.SetActive(Value);
        }

        OnValueChanged?.Invoke(Value);
        OnValueChangedTrigger?.Invoke();
    }
}
