using UnityEngine;

namespace Saves
{
    public class SettingsSave
    {
        public GraphicsSave GraphicsSave;
    }

    public enum GraphicsQuality
    {
        Low, Medium, High, Ultra, Crazy
    }
    public class GraphicsSave
    {
        public GraphicsQuality ShadowsQuality;
        public GraphicsQuality LightingQuality;
        public bool AntiAliasingEnabled;
        public bool VsyncEnabled;
        public int MaxFps;
        public bool FullscreenEnabled;
        public bool BloomEnabled;
        public bool MotionBlurEnabled;
        public bool VignetteEnabled;
    }

    public class AudioSave
    {
        public float GlobalVolume;
        public float AmbientVolume;
        public float InterfaceVolume;
    }
}

