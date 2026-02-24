using System;
using UnityEngine;

namespace Saves
{
    [Serializable]
    public class SettingsSave
    {
        public AudioSave AudioSave;
        public GraphicsSave GraphicsSave;
        public ControlsSave ControlsSave;
    }

    public enum GraphicsQuality
    {
        Low, Medium, High, Ultra, Crazy
    }

    [Serializable]
    public class GraphicsSave
    {
        public int ScreenWidth;
        public int ScreenHeight;
        public GraphicsQuality ShadowsQuality;
        public GraphicsQuality LightingQuality;
        public bool AntiAliasingEnabled;
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

