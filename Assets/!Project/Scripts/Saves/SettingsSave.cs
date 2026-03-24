using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AMD;
using UnityEngine.NVIDIA;
using static UnityEngine.LightProbeProxyVolume;

namespace Saves
{
    [Serializable]
    public class SettingsSave
    {
        public AudioSave AudioSave;
        public GraphicsSave GraphicsSave;
        public ControlsSave ControlsSave;

        public SettingsSave()
        {
            AudioSave = new AudioSave();
            GraphicsSave = new GraphicsSave();
            ControlsSave = new ControlsSave();
        }
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
        public int ResolutionMode;
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
            
            ResolutionMode = GraphicsManager.GetMaxResolutionKey();
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

        public Dictionary<InputActionType, string> ActionsBinds { get; private set; } = new Dictionary<InputActionType, string>();

        public ControlsSave()
        {
            MouseSensitivity = 50;

            ActionsBinds[InputActionType.MoveForward] = "W";
            ActionsBinds[InputActionType.MoveBackward] = "S";
            ActionsBinds[InputActionType.MoveRight] = "D";
            ActionsBinds[InputActionType.MoveLeft] = "A";
            ActionsBinds[InputActionType.Sprint] = "LeftShift";
            ActionsBinds[InputActionType.Jump] = "Space";
            ActionsBinds[InputActionType.Crouch] = "LeftCtrl";
            ActionsBinds[InputActionType.Interact] = "E";
        }

        public string GetBind(InputActionType actionType)
        {
            if (!ActionsBinds.ContainsKey(actionType)) return "";

            return ActionsBinds[actionType];
        }

        public void SetBind(InputActionType actionType, string bind)
        {
            ActionsBinds[actionType] = bind;
        }
    }
}

