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
            GraphicsQuality.Low => 256,
            GraphicsQuality.Medium => 512,
            GraphicsQuality.High => 1024,
            GraphicsQuality.Ultra => 2048,
            GraphicsQuality.Crazy => 4096,
            _ => 1024
        };

        float distance = save.ShadowsQuality switch
        {
            GraphicsQuality.Low => 25f,
            GraphicsQuality.Medium => 50f,
            GraphicsQuality.High => 100f,
            GraphicsQuality.Ultra => 200f,
            GraphicsQuality.Crazy => 400f,
            _ => 100f
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

        if (_globalVolume.profile.TryGet<GlobalIllumination>(out var gi))
        {
            gi.active = save.LightingQuality != GraphicsQuality.Low;
            gi.fullResolutionSS.overrideState = true;
            gi.fullResolutionSS.value = save.LightingQuality >= GraphicsQuality.High;
            gi.maxRaySteps = save.LightingQuality switch
            {
                GraphicsQuality.Low => 8,
                GraphicsQuality.Medium => 16,
                GraphicsQuality.High => 32,
                GraphicsQuality.Ultra => 64,
                GraphicsQuality.Crazy => 96,
                _ => 32
            };

        }

        if (_globalVolume.profile.TryGet<Fog>(out var fog))
        {
            fog.volumetricFogBudget = save.LightingQuality switch
            {
                GraphicsQuality.Low => 0.2f,
                GraphicsQuality.Medium => 0.3f,
                GraphicsQuality.High => 0.4f,
                GraphicsQuality.Ultra => 0.5f,
                GraphicsQuality.Crazy => 0.6f,
                _ => 0.4f
            };

            fog.sliceDistributionUniformity.value = save.LightingQuality switch
            {
                GraphicsQuality.Low => 0.25f,
                GraphicsQuality.Medium => 0.4f,
                GraphicsQuality.High => 0.5f,
                GraphicsQuality.Ultra => 0.6f,
                GraphicsQuality.Crazy => 0.8f,
                _ => 0.5f
            };

        }

        if (_globalVolume.profile.TryGet<ScreenSpaceAmbientOcclusion>(out var ao))
        {
            ao.active = save.LightingQuality != GraphicsQuality.Low;
            ao.intensity.overrideState = true;
            ao.intensity.value = 1f;
            ao.quality.overrideState = true;
            ao.quality.value = save.LightingQuality switch
            {
                GraphicsQuality.Low => 0,
                GraphicsQuality.Medium => 0,
                GraphicsQuality.High => 1,
                GraphicsQuality.Ultra => 1,
                GraphicsQuality.Crazy => 2,
                _ => 1
            };
        }
    }
}