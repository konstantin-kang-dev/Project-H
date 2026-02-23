using Saves;
using UnityEngine;
using UnityEngine.Rendering;

public class SettingsUI : MonoBehaviour
{
    [Header("Graphics settings")]
    [SerializeField] ValueSelectorUI _shadowsQualitySelector;
    [SerializeField] ValueSelectorUI _lightingQualitySelector;
    [SerializeField] CheckBox _antiAliasingCheckbox;
    [SerializeField] CheckBox _vsyncCheckbox;
    [SerializeField] RangeSelectorUI _maxFpsSelector;
    [SerializeField] CheckBox _fullscreenCheckbox;
    [SerializeField] CheckBox _bloomCheckbox;
    [SerializeField] CheckBox _motionBlurCheckbox;
    [SerializeField] CheckBox _vignetteCheckbox;
    private void OnEnable()
    {
        ApplySave(SaveManager.GameSave.SettingsSave);

        _shadowsQualitySelector.OnValueChanged += IntTrigger;
        _lightingQualitySelector.OnValueChanged += IntTrigger;
        _antiAliasingCheckbox.OnValueChanged += BoolTrigger;
        _vsyncCheckbox.OnValueChanged += BoolTrigger;
        _maxFpsSelector.OnValueChanged += IntTrigger;
        _fullscreenCheckbox.OnValueChanged += BoolTrigger;
        _bloomCheckbox.OnValueChanged += BoolTrigger;
        _motionBlurCheckbox.OnValueChanged += BoolTrigger;
        _vignetteCheckbox.OnValueChanged += BoolTrigger;
    }

    void IntTrigger(int value)
    {
        CollectValues();
    }

    void BoolTrigger(bool value)
    {
        CollectValues(); 
    }

    void CollectValues()
    {
        GraphicsSave graphicsSave = new GraphicsSave();
        graphicsSave.ShadowsQuality = (GraphicsQuality)_shadowsQualitySelector.SelectedValue;
        graphicsSave.LightingQuality = (GraphicsQuality)_lightingQualitySelector.SelectedValue;
        graphicsSave.AntiAliasingEnabled = _antiAliasingCheckbox.Value;
        graphicsSave.VsyncEnabled = _vsyncCheckbox.Value;
        graphicsSave.MaxFps = _maxFpsSelector.Value;
        graphicsSave.FullscreenEnabled = _fullscreenCheckbox.Value;
        graphicsSave.BloomEnabled = _bloomCheckbox.Value;
        graphicsSave.MotionBlurEnabled = _motionBlurCheckbox.Value;
        graphicsSave.VignetteEnabled = _vignetteCheckbox.Value;

        SaveManager.GameSave.SettingsSave.GraphicsSave = graphicsSave;
        SaveManager.SaveAll();
    }

    void ApplySave(SettingsSave settingsSave)
    {
        GraphicsSave graphicsSave = settingsSave.GraphicsSave;

        _shadowsQualitySelector.SetValue((int)graphicsSave.ShadowsQuality);
        _lightingQualitySelector.SetValue((int)graphicsSave.LightingQuality);
        _antiAliasingCheckbox.SetValue(graphicsSave.AntiAliasingEnabled);
        _vsyncCheckbox.SetValue(graphicsSave.VsyncEnabled);
        _maxFpsSelector.SetValue(graphicsSave.MaxFps);
        _fullscreenCheckbox.SetValue(graphicsSave.FullscreenEnabled);
        _bloomCheckbox.SetValue(graphicsSave.BloomEnabled);
        _motionBlurCheckbox.SetValue(graphicsSave.MotionBlurEnabled);
        _vignetteCheckbox.SetValue(graphicsSave.VignetteEnabled);
    }
}
