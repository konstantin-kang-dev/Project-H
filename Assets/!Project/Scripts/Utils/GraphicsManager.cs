using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using Saves;

public class GraphicsManager
{
    public static Resolution[] AvailableResolutions { get; private set; } = new Resolution[0];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        AvailableResolutions = Screen.resolutions;
    }

    public static void ChangeGraphics(GraphicsSave graphicsSave)
    {

    }
}