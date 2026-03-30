using Saves;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public class GraphicsManager
{
    public static Resolution[] AvailableResolutions { get; private set; } = new Resolution[0];
    public static GraphicsSave RelevantGraphicsSave = new GraphicsSave();
    private static Volume _globalVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        var seen = new HashSet<string>();
        var filtered = new List<Resolution>();
        foreach (var r in Screen.resolutions)
        {
            if (seen.Add($"{r.width}x{r.height}"))
                filtered.Add(r);
        }
        AvailableResolutions = filtered.ToArray();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _globalVolume = Object.FindFirstObjectByType<Volume>();
        ApplyPostProcessSettings(RelevantGraphicsSave);
        ApplyShadowSettings(RelevantGraphicsSave);
        ApplyLightingSettings(RelevantGraphicsSave);
    }

    public static void ApplySave(GraphicsSave save)
    {
        RelevantGraphicsSave = save;
        Screen.SetResolution(RelevantGraphicsSave.ScreenWidth, RelevantGraphicsSave.ScreenHeight,
            RelevantGraphicsSave.FullscreenEnabled ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        Debug.Log($"[GraphicsManager] Set resolution: {RelevantGraphicsSave.ScreenWidth}x{RelevantGraphicsSave.ScreenHeight} ({Screen.width}x{Screen.height}) isFullscreen: {Screen.fullScreen}");

        Application.targetFrameRate = RelevantGraphicsSave.MaxFps;
        QualitySettings.vSyncCount = RelevantGraphicsSave.VsyncEnabled ? 1 : 0;

        foreach (var camera in Camera.allCameras)
        {
            if (camera.TryGetComponent<HDAdditionalCameraData>(out HDAdditionalCameraData hdData))
            {
                ApplyCameraSettings(hdData);
            }
        }

        LocalVolumetricFog[] localVolumetricFogs = GameObject.FindObjectsByType<LocalVolumetricFog>(FindObjectsSortMode.None);
        foreach (var localVolumetricFog in localVolumetricFogs)
        {
            ApplyLocalVolumetricFogSettings(localVolumetricFog);
        }

        _globalVolume = Object.FindFirstObjectByType<Volume>();
        ApplyPostProcessSettings(RelevantGraphicsSave);
        ApplyShadowSettings(RelevantGraphicsSave);
        ApplyLightingSettings(RelevantGraphicsSave);
    }

    public static void ApplyCameraSettings(HDAdditionalCameraData hdData)
    {
        hdData.antialiasing = RelevantGraphicsSave.AntiAliasingEnabled
            ? HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing
            : HDAdditionalCameraData.AntialiasingMode.None;

        switch (RelevantGraphicsSave.UpscaleType)
        {
            case UpscaleType.None:
                hdData.allowDynamicResolution = false;
                hdData.allowDeepLearningSuperSampling = false;
                hdData.allowFidelityFX2SuperResolution = false;
                break;
            case UpscaleType.DLSS:
                hdData.allowDynamicResolution = true;
                hdData.allowDeepLearningSuperSampling = true;
                hdData.deepLearningSuperSamplingUseCustomQualitySettings = true;
                hdData.allowFidelityFX2SuperResolution = false;
                hdData.deepLearningSuperSamplingQuality = (uint)RelevantGraphicsSave.DLSSQuality;
                //Debug.Log($"[GraphicsManager] Enabled DLSS {hdData.deepLearningSuperSamplingQuality} ({RelevantGraphicsSave.DLSSQuality})");
                break;
            case UpscaleType.FSR:
                hdData.allowDynamicResolution = true;
                hdData.allowDeepLearningSuperSampling = false;
                hdData.allowFidelityFX2SuperResolution = true;
                hdData.fidelityFX2SuperResolutionUseCustomQualitySettings = true;
                hdData.fidelityFX2SuperResolutionQuality = (uint)RelevantGraphicsSave.FSRQuality;
                break;
        }
        Debug.Log($"[GraphicsManager] Enabled Upscale: {RelevantGraphicsSave.UpscaleType} FSR2 Quality: {hdData.fidelityFX2SuperResolutionQuality} DLSS Quality:{hdData.deepLearningSuperSamplingQuality}");
    }

    public static void ApplyLocalVolumetricFogSettings(LocalVolumetricFog localVolumetricFog)
    {
        localVolumetricFog.parameters.meanFreePath = RelevantGraphicsSave.LightingQuality switch
        {
            GraphicsQuality.Low => 400,
            GraphicsQuality.Medium => 300,
            GraphicsQuality.High => 250,
            GraphicsQuality.Ultra => 200,
            GraphicsQuality.Crazy => 170,
            _ => 250
        };
    }

    public static void ApplyPostProcessSettings(GraphicsSave save)
    {
        if (_globalVolume == null) return;

        if (_globalVolume.profile.TryGet<Bloom>(out var bloom))
            bloom.active = save.BloomEnabled;

        if (_globalVolume.profile.TryGet<MotionBlur>(out var motionBlur))
            motionBlur.active = save.MotionBlurEnabled;

        if (_globalVolume.profile.TryGet<Vignette>(out var vignette))
            vignette.active = save.VignetteEnabled;
    }
    public static void ApplyShadowSettings(GraphicsSave save)
    {
        int resolution = save.ShadowsQuality switch
        {
            GraphicsQuality.Low => 768,
            GraphicsQuality.Medium => 1024,
            GraphicsQuality.High => 1440,
            GraphicsQuality.Ultra => 2048,
            GraphicsQuality.Crazy => 4096,
            _ => 1440
        };

        float distance = save.ShadowsQuality switch
        {
            GraphicsQuality.Low => 25f,
            GraphicsQuality.Medium => 35f,
            GraphicsQuality.High => 45f,
            GraphicsQuality.Ultra => 55f,
            GraphicsQuality.Crazy => 65f,
            _ => 45f
        };

        foreach (var light in Object.FindObjectsByType<HDAdditionalLightData>(FindObjectsSortMode.None))
        {
            light.SetShadowResolutionOverride(true);
            light.SetShadowResolution(resolution);

            light.useContactShadow.level = save.ShadowsQuality switch
            {
                GraphicsQuality.Low => 0,
                GraphicsQuality.Medium => 0,
                GraphicsQuality.High => 1,
                GraphicsQuality.Ultra => 2,
                GraphicsQuality.Crazy => 2,
                _ => 1
            };
        }

        if (_globalVolume != null && _globalVolume.profile.TryGet<HDShadowSettings>(out var shadowSettings))
            shadowSettings.maxShadowDistance.value = distance;
    }
    public static void ApplyLightingSettings(GraphicsSave save)
    {
        if (_globalVolume == null) return;

        /*
        if (_globalVolume.profile.TryGet<GlobalIllumination>(out var gi))
        {
            gi.active = save.LightingQuality != GraphicsQuality.Low;
            gi.maxRaySteps = save.LightingQuality switch
            {
                GraphicsQuality.Low => 6,
                GraphicsQuality.Medium => 8,
                GraphicsQuality.High => 16,
                GraphicsQuality.Ultra => 16,
                GraphicsQuality.Crazy => 32,
                _ => 16
            };

        }*/

        if (_globalVolume.profile.TryGet<Fog>(out var fog))
        {
            fog.enableVolumetricFog.value = save.LightingQuality >= GraphicsQuality.Medium;

            fog.volumetricFogBudget = save.LightingQuality switch
            {
                GraphicsQuality.High => 0.12f,
                GraphicsQuality.Ultra => 0.18f,
                GraphicsQuality.Crazy => 0.27f,
                _ => 0.12f
            };

        }

        if (_globalVolume.profile.TryGet<ScreenSpaceAmbientOcclusion>(out var ao))
        {
            ao.active = save.LightingQuality >= GraphicsQuality.Medium;
            ao.intensity.overrideState = true;
            ao.intensity.value = 1f;
            ao.quality.overrideState = true;
            ao.quality.value = save.LightingQuality switch
            {
                GraphicsQuality.Low => 0,
                GraphicsQuality.Medium => 1,
                GraphicsQuality.High => 1,
                GraphicsQuality.Ultra => 1,
                GraphicsQuality.Crazy => 1,
                _ => 1
            };
        }
    }

    public static Resolution GetMaxResolution()
    {
        return AvailableResolutions[AvailableResolutions.Length - 1];
    }

    public static int GetMaxResolutionKey()
    {
        return AvailableResolutions.Length - 1;
    }
}