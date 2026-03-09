using GameAudio;
using Saves;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AMD;
using UnityEngine.NVIDIA;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.Experimental.Rendering.GraphicsStateCollection;

public class SettingsUI : BasicCustomWindow
{
    [Header("Navigation")]
    [SerializeField] ToggleGroup _navigationGroup;
    [SerializeField] Button _backBtn;

    [Header("Pages")]
    [SerializeField] Dictionary<int, BasicWindowVisuals> _pages = new Dictionary<int, BasicWindowVisuals>();
    BasicWindowVisuals _lastOpenedWindow = null;

    [Header("Audio Settings")]
    [SerializeField] RangeSelectorUI _globalVolumeSelector;
    [SerializeField] RangeSelectorUI _environmentVolumeSelector;
    [SerializeField] RangeSelectorUI _interfaceVolumeSelector;

    [Header("Graphics settings")]
    [SerializeField] ValueSelectorUI _resolutionSelector;
    [SerializeField] ValueSelectorUI _shadowsQualitySelector;
    [SerializeField] ValueSelectorUI _lightingQualitySelector;
    [SerializeField] CheckBox _antiAliasingCheckbox;
    [SerializeField] ValueSelectorUI _upscaleTypeSelector;
    [SerializeField] ValueSelectorUI _dlssQualitySelector;
    [SerializeField] ValueSelectorUI _fsrQualitySelector;

    [SerializeField] CheckBox _vsyncCheckbox;
    [SerializeField] RangeSelectorUI _maxFpsSelector;
    [SerializeField] CheckBox _fullscreenCheckbox;
    [SerializeField] CheckBox _bloomCheckbox;
    [SerializeField] CheckBox _motionBlurCheckbox;
    [SerializeField] CheckBox _vignetteCheckbox;

    [Header("Controls Settings")]
    [SerializeField] RangeSelectorUI _mouseSensitivitySelector;
    [SerializeField] InputSelector _moveForwardSelector;
    [SerializeField] InputSelector _moveBackwardSelector;
    [SerializeField] InputSelector _moveRightSelector;
    [SerializeField] InputSelector _moveLeftSelector;
    [SerializeField] InputSelector _sprintSelector;
    [SerializeField] InputSelector _jumpSelector;
    [SerializeField] InputSelector _crouchSelector;
    [SerializeField] InputSelector _interactSelector;

    private void OnEnable()
    {
        ApplySave(SaveManager.GameSave.SettingsSave);

        _globalVolumeSelector.OnValueChangedTrigger += CollectAudioValues;
        _environmentVolumeSelector.OnValueChangedTrigger += CollectAudioValues;
        _interfaceVolumeSelector.OnValueChangedTrigger += CollectAudioValues;

        _resolutionSelector.OnValueChangedTrigger += CollectGraphicsValues;
        _shadowsQualitySelector.OnValueChangedTrigger += CollectGraphicsValues;
        _lightingQualitySelector.OnValueChangedTrigger += CollectGraphicsValues;
        _antiAliasingCheckbox.OnValueChangedTrigger += CollectGraphicsValues;

        _upscaleTypeSelector.OnValueChangedTrigger += CollectGraphicsValues;
        _upscaleTypeSelector.OnValueChanged += HandleUpscaleTypeChange;
        _dlssQualitySelector.OnValueChangedTrigger += CollectGraphicsValues;
        _fsrQualitySelector.OnValueChangedTrigger += CollectGraphicsValues;

        _vsyncCheckbox.OnValueChangedTrigger += CollectGraphicsValues;
        _maxFpsSelector.OnValueChangedTrigger += CollectGraphicsValues;
        _fullscreenCheckbox.OnValueChangedTrigger += CollectGraphicsValues;
        _bloomCheckbox.OnValueChangedTrigger += CollectGraphicsValues;
        _motionBlurCheckbox.OnValueChangedTrigger += CollectGraphicsValues;
        _vignetteCheckbox.OnValueChangedTrigger += CollectGraphicsValues;

        _mouseSensitivitySelector.OnValueChangedTrigger += CollectControlsValues;
        _moveForwardSelector.OnValueChangedTrigger += CollectControlsValues;
        _moveBackwardSelector.OnValueChangedTrigger += CollectControlsValues;
        _moveRightSelector.OnValueChangedTrigger += CollectControlsValues;
        _moveLeftSelector.OnValueChangedTrigger += CollectControlsValues;
        _sprintSelector.OnValueChangedTrigger += CollectControlsValues;
        _jumpSelector.OnValueChangedTrigger += CollectControlsValues;
        _crouchSelector.OnValueChangedTrigger += CollectControlsValues;
        _interactSelector.OnValueChangedTrigger += CollectControlsValues;

        _backBtn.onClick.AddListener(HandleBackBtn);
        _navigationGroup.OnToggle += HandleNavigation;

    }

    void OnDisable()
    {
        _globalVolumeSelector.OnValueChangedTrigger -= CollectAudioValues;
        _environmentVolumeSelector.OnValueChangedTrigger -= CollectAudioValues;
        _interfaceVolumeSelector.OnValueChangedTrigger -= CollectAudioValues;

        _resolutionSelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _shadowsQualitySelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _lightingQualitySelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _antiAliasingCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;

        _upscaleTypeSelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _upscaleTypeSelector.OnValueChanged -= HandleUpscaleTypeChange;
        _dlssQualitySelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _fsrQualitySelector.OnValueChangedTrigger -= CollectGraphicsValues;

        _vsyncCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;
        _maxFpsSelector.OnValueChangedTrigger -= CollectGraphicsValues;
        _fullscreenCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;
        _bloomCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;
        _motionBlurCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;
        _vignetteCheckbox.OnValueChangedTrigger -= CollectGraphicsValues;

        _mouseSensitivitySelector.OnValueChangedTrigger -= CollectControlsValues;
        _moveForwardSelector.OnValueChangedTrigger -= CollectControlsValues;
        _moveBackwardSelector.OnValueChangedTrigger -= CollectControlsValues;
        _moveRightSelector.OnValueChangedTrigger -= CollectControlsValues;
        _moveLeftSelector.OnValueChangedTrigger -= CollectControlsValues;
        _sprintSelector.OnValueChangedTrigger -= CollectControlsValues;
        _jumpSelector.OnValueChangedTrigger -= CollectControlsValues;
        _crouchSelector.OnValueChangedTrigger -= CollectControlsValues;
        _interactSelector.OnValueChangedTrigger -= CollectControlsValues;

        _backBtn.onClick.RemoveListener(HandleBackBtn);
        _navigationGroup.OnToggle -= HandleNavigation;
    }

    void CollectAudioValues()
    {
        AudioSave audioSave = new AudioSave();
        audioSave.GlobalVolume = _globalVolumeSelector.Value;
        audioSave.EnvironmentVolume = _environmentVolumeSelector.Value;
        audioSave.InterfaceVolume = _interfaceVolumeSelector.Value;

        SaveManager.GameSave.SettingsSave.AudioSave = audioSave;
        SaveManager.SaveAll();
        GlobalAudioManager.Instance.ApplySave(audioSave);
    }

    void CollectGraphicsValues()
    {
        GraphicsSave graphicsSave = new GraphicsSave();

        Resolution resolution = GraphicsManager.AvailableResolutions[_resolutionSelector.SelectedValue];

        graphicsSave.ScreenWidth = resolution.width;
        graphicsSave.ScreenHeight = resolution.height;
        graphicsSave.ShadowsQuality = (GraphicsQuality)_shadowsQualitySelector.SelectedValue;
        graphicsSave.LightingQuality = (GraphicsQuality)_lightingQualitySelector.SelectedValue;
        graphicsSave.AntiAliasingEnabled = _antiAliasingCheckbox.Value;

        graphicsSave.UpscaleType = (UpscaleType)_upscaleTypeSelector.SelectedValue;
        graphicsSave.DLSSQuality = (DLSSQuality)_dlssQualitySelector.SelectedValue;
        graphicsSave.FSRQuality = (FSR2Quality)_fsrQualitySelector.SelectedValue;

        graphicsSave.VsyncEnabled = _vsyncCheckbox.Value;
        graphicsSave.MaxFps = _maxFpsSelector.Value;
        graphicsSave.FullscreenEnabled = _fullscreenCheckbox.Value;
        graphicsSave.BloomEnabled = _bloomCheckbox.Value;
        graphicsSave.MotionBlurEnabled = _motionBlurCheckbox.Value;
        graphicsSave.VignetteEnabled = _vignetteCheckbox.Value;

        SaveManager.GameSave.SettingsSave.GraphicsSave = graphicsSave;
        SaveManager.SaveAll();
        GraphicsManager.ApplySave(graphicsSave);
    }
    void CollectControlsValues()
    {
        ControlsSave controlsSave = new ControlsSave();
        controlsSave.MouseSensitivity = _mouseSensitivitySelector.Value;
        controlsSave.MoveForwardBind = _moveForwardSelector.Value;
        controlsSave.MoveBackwardBind = _moveBackwardSelector.Value;
        controlsSave.MoveRightBind = _moveRightSelector.Value;
        controlsSave.MoveLeftBind = _moveLeftSelector.Value;
        controlsSave.SprintBind = _sprintSelector.Value;
        controlsSave.JumpBind = _jumpSelector.Value;
        controlsSave.CrouchBind = _crouchSelector.Value;
        controlsSave.InteractBind = _interactSelector.Value;

        SaveManager.GameSave.SettingsSave.ControlsSave = controlsSave;
        SaveManager.SaveAll();
        GlobalInputManager.ApplySave(SaveManager.GameSave.SettingsSave.ControlsSave);

    }

    void ApplySave(SettingsSave settingsSave)
    {
        AudioSave audioSave = settingsSave.AudioSave;
        _globalVolumeSelector.SetValue(audioSave.GlobalVolume, true);
        _environmentVolumeSelector.SetValue(audioSave.EnvironmentVolume, true);
        _interfaceVolumeSelector.SetValue(audioSave.InterfaceVolume, true);

        GraphicsSave graphicsSave = settingsSave.GraphicsSave;
        _shadowsQualitySelector.SetValue((int)graphicsSave.ShadowsQuality);
        _lightingQualitySelector.SetValue((int)graphicsSave.LightingQuality);
        _antiAliasingCheckbox.SetValue(graphicsSave.AntiAliasingEnabled);
        
        _upscaleTypeSelector.SetValue((int)graphicsSave.UpscaleType);
        HandleUpscaleTypeChange(_upscaleTypeSelector.SelectedValue);
        _dlssQualitySelector.SetValue((int)graphicsSave.DLSSQuality);
        _fsrQualitySelector.SetValue((int)graphicsSave.FSRQuality); 

        _vsyncCheckbox.SetValue(graphicsSave.VsyncEnabled);
        _maxFpsSelector.SetValue(graphicsSave.MaxFps, true);
        _fullscreenCheckbox.SetValue(graphicsSave.FullscreenEnabled);
        _bloomCheckbox.SetValue(graphicsSave.BloomEnabled);
        _motionBlurCheckbox.SetValue(graphicsSave.MotionBlurEnabled);
        _vignetteCheckbox.SetValue(graphicsSave.VignetteEnabled);

        ControlsSave controlsSave = settingsSave.ControlsSave;
        _mouseSensitivitySelector.SetValue(controlsSave.MouseSensitivity);
        _moveForwardSelector.SetValue(controlsSave.MoveForwardBind);
        _moveBackwardSelector.SetValue(controlsSave.MoveBackwardBind);
        _moveRightSelector.SetValue(controlsSave.MoveRightBind);
        _moveLeftSelector.SetValue(controlsSave.MoveLeftBind);
        _sprintSelector.SetValue(controlsSave.SprintBind);
        _jumpSelector.SetValue(controlsSave.JumpBind);
        _crouchSelector.SetValue(controlsSave.CrouchBind);
        _interactSelector.SetValue(controlsSave.InteractBind);
    }

    public override void SetVisibility(bool visible, bool doInstantly)
    {
        base.SetVisibility(visible, doInstantly);
        if (visible)
        {
            _lastOpenedWindow = null;
            foreach (var page in _pages)
            {
                page.Value.ProcessOutAnimation(true);
            }
            HandleNavigation(0);
        }
    }

    void HandleNavigation(int value)
    {
        if (_lastOpenedWindow != null)
        {
            _lastOpenedWindow.ProcessOutAnimation();
        }

        if (_pages.ContainsKey(value))
        {
            _pages[value].ProcessInAnimation();
            _lastOpenedWindow = _pages[value];
        }
        else
        {
            _lastOpenedWindow = null;
        }
    }

    void HandleBackBtn()
    {
        WindowsNavigator.Instance.GoBack();
    }

    void HandleUpscaleTypeChange(int value)
    {
        switch (value)
        {
            case 0:
                _dlssQualitySelector.gameObject.SetActive(false);
                _fsrQualitySelector.gameObject.SetActive(false);
                break;
            case 1:
                _dlssQualitySelector.gameObject.SetActive(true);
                _fsrQualitySelector.gameObject.SetActive(false);
                break;
            case 2:
                _dlssQualitySelector.gameObject.SetActive(false);
                _fsrQualitySelector.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
