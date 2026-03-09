using System;
using UnityEngine;
using UnityEngine.AMD;
using UnityEngine.NVIDIA;

namespace Saves
{
    [Serializable]
    public class SettingsSave
    {
        public AudioSave AudioSave;
        public GraphicsSave GraphicsSave;
        public ControlsSave ControlsSave;
    }

    [Serializable]
    public enum GraphicsQuality
    {
        Low, Medium, High, Ultra, Crazy
    }

    [Serializable]
    public enum UpscaleType
    {
        None = 0,
        DLSS = 1,
        FSR = 2,
    }

    [Serializable]
    public class GraphicsSave
    {
        public int ScreenWidth;
        public int ScreenHeight;
        public GraphicsQuality ShadowsQuality;
        public GraphicsQuality LightingQuality;
        public bool AntiAliasingEnabled;
        public UpscaleType UpscaleType;
        public DLSSQuality DLSSQuality;
        public FSR2Quality FSRQuality;

        public bool VsyncEnabled;
        public int MaxFps;
        public bool FullscreenEnabled;
        public bool BloomEnabled;
        public bool MotionBlurEnabled;
        public bool VignetteEnabled;

        public GraphicsSave()
        {
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            ShadowsQuality = GraphicsQuality.Medium;
            LightingQuality = GraphicsQuality.Medium;
            AntiAliasingEnabled = true;
            UpscaleType = UpscaleType.None;
            DLSSQuality = DLSSQuality.MaximumQuality;
            FSRQuality = FSR2Quality.Quality;

            VsyncEnabled = false;
            MaxFps = 500;
            FullscreenEnabled = true;
            BloomEnabled = true;
            MotionBlurEnabled = true;
            VignetteEnabled = true;
        }
    }

    [Serializable]
    public class AudioSave
    {
        public int GlobalVolume;
        public int EnvironmentVolume;
        public int InterfaceVolume;

        public AudioSave()
        {
            GlobalVolume = 50;
            EnvironmentVolume = 50;
            InterfaceVolume = 50;
        }
    }

    [Serializable]
    public class ControlsSave
    {
        public int MouseSensitivity;
        public string MoveForwardBind;
        public string MoveBackwardBind;
        public string MoveRightBind;
        public string MoveLeftBind;
        public string SprintBind;
        public string JumpBind;
        public string CrouchBind;
        public string InteractBind;

        public ControlsSave()
        {
            MouseSensitivity = 50;
            MoveForwardBind = "W";
            MoveBackwardBind = "S";
            MoveRightBind = "D";
            MoveLeftBind = "A";
            SprintBind = "LeftShift";
            JumpBind = "Space";
            CrouchBind = "LeftCtrl";
            InteractBind = "E";
        }
    }
}

