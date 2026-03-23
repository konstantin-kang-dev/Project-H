#if UNITY_EDITOR
#if UNITY_6000_0_OR_NEWER

using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.Rendering;

public class ftAPVSettings : EditorWindow
{
    public static ftAPVSettings instance;

    bool needsOnGUI = true;

    public void OnGUI()
    {
        instance = this;
        if (!ftRenderLightmap.showAPVSettings) Close();

        titleContent.text = "APV placement setings";

        this.minSize = new Vector2(512, 240);
        this.maxSize = new Vector2(512, 240);

        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (assembly.GetName().Name == "Unity.RenderPipelines.Core.Runtime")
            {
                var rtypes = assembly.GetTypes();
                for(int i=0; i<rtypes.Length; i++)
                {
                    if (rtypes[i].Name == "ProbeReferenceVolume")
                    {
                        var ltype = rtypes[i];
                        var instProp = ltype.GetProperty("instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty);
                        if (instProp == null)
                        {
                            Debug.LogError("Can't get ProbeReferenceVolume.instance");
                        }
                        else
                        {
                            var inst = instProp.GetValue(null, null);
                            if (inst == null)
                            {
                                Debug.LogError("ProbeReferenceVolume.instance is null");
                            }
                            else
                            {
                                var prop1 = ltype.GetProperty("isInitialized", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                                if (prop1 == null)
                                {
                                    Debug.LogError("Can't get ProbeReferenceVolume.isInitialized");
                                }
                                else
                                {
                                    if (!(bool)prop1.GetValue(inst, null))
                                    {
                                        needsOnGUI = true;
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
        }
                    
        foreach (var assembly in assemblies)
        {
            if (assembly.GetName().Name == "Unity.RenderPipelines.Core.Editor")
            {
                var rtypes = assembly.GetTypes();
                for(int i=0; i<rtypes.Length; i++)
                {
                    if (rtypes[i].Name == "ProbeVolumeLightingTab")
                    {
                        var ltype = rtypes[i];
                        var instField = ltype.GetField("instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        if (instField == null)
                        {
                            Debug.LogError("Can't get ProbeVolumeLightingTab.instance");
                        }
                        else
                        {
                            var inst = instField.GetValue(null);
                            if (inst == null)
                            {
                                //Debug.LogError("ProbeVolumeLightingTab.instance is null");
                                inst = System.Activator.CreateInstance(ltype);
                                var emethod = ltype.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                if (emethod == null)
                                {
                                    Debug.LogError("Can't get ProbeVolumeLightingTab.OnEnable");
                                }
                                else
                                {
                                    emethod.Invoke(inst, null);
                                    Debug.LogError("OnEnable called");
                                }

                                /*emethod = ltype.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                if (emethod == null)
                                {
                                    Debug.LogError("Can't get ProbeVolumeLightingTab.OnGUI");
                                }
                                else
                                {
                                    emethod.Invoke(inst, null);
                                }
                                return;*/

                                //EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting");
                                /*var lightingWindowType = typeof(Editor).Assembly.GetType("UnityEditor.LightingWindow");
                                if (lightingWindowType == null)
                                {
                                    Debug.LogError("Can't get LightingWindow");
                                }
                                else
                                {
                                    var w = EditorWindow.GetWindow(lightingWindowType);
                                    var m = lightingWindowType.GetMethod("SetActiveTab", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                    Debug.LogError(m);
                                }
                                return;*/
                            }
                            //else
                            {
                                var activeSetProp = ltype.GetProperty("activeSetEditor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                                if (activeSetProp == null)
                                {
                                    Debug.LogError("Can't get activeSetEditor");
                                }
                                else
                                {
                                    var activeSet = activeSetProp.GetValue(inst, null);
                                    if (activeSet == null)
                                    {
                                        Debug.LogError("ProbeVolumeLightingTab.activeSet is null");
                                    }
                                    else
                                    {
                                        if (needsOnGUI)
                                        {
                                            var ui = ltype.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                            if (ui == null)
                                            {
                                                Debug.LogError("Can't get OnGUI");
                                            }
                                            else
                                            {
                                                ui.Invoke(inst, null);
                                            }
                                            needsOnGUI = false;
                                            return;
                                        }

                                        var uiType = activeSet.GetType();
                                        var ui2 = uiType.GetMethod("ProbePlacementGUI", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                        if (ui2 == null)
                                        {
                                            Debug.LogError("Can't get ProbePlacementGUI");
                                        }
                                        else
                                        {
                                            ui2.Invoke(activeSet, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    public void OnDisable()
    {
        ftRenderLightmap.showAPVSettings = false;
    }
}

#endif
#endif