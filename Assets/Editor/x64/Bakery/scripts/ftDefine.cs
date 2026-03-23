#if UNITY_EDITOR

// Disable 'obsolete' warnings
#pragma warning disable 0618

using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Build;
using System.IO;

[InitializeOnLoad]
#if UNITY_2017_4_OR_NEWER
public class ftDefine : IActiveBuildTargetChanged
#else
public class ftDefine
#endif
{
    static void AddDefine()
    {
#if UNITY_2023_2_OR_NEWER
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
        var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
#endif
        if (!defines.Contains("BAKERY_INCLUDED"))
        {
            if (defines.Length > 0) defines += ";";
            defines += "BAKERY_INCLUDED";
            if (!defines.Contains("BAKERY_NOREIMPORT"))
            {
                defines += ";BAKERY_NOREIMPORT";
            }
#if UNITY_2023_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
#endif
        }
    }

    static void CreateAsmDefs()
    {
#if UNITY_2019_2_OR_NEWER
#if !NOASMDEFS
        // Fix for Unity 2018 not supporting modern asmdefs
        var path = ftLightmaps.GetEditorPath();
        var apvPath = path + "/scripts/APV/BakeryAPV";
        var tempEnding = "_asmdef.txt";
        var finEnding = ".asmdef";
        if (!File.Exists(apvPath + finEnding) && File.Exists(apvPath + tempEnding))
        {
            File.Copy(apvPath + tempEnding, apvPath + finEnding);
            File.Delete(apvPath + tempEnding);
        }
        var ecsPath = path + "/scripts/ECS/BakeryECS";
        if (!File.Exists(ecsPath + finEnding) && File.Exists(ecsPath + tempEnding))
        {
            File.Copy(ecsPath + tempEnding, ecsPath + finEnding);
            File.Delete(ecsPath + tempEnding);
        }
        var edPath = path + "scripts/BakeryEditorAssembly";
        if (File.Exists(edPath + tempEnding))
        {
            File.Copy(edPath + tempEnding, edPath + finEnding, true);
            File.Delete(edPath + tempEnding);
        }
#endif
#endif
    }

    static ftDefine()
    {
        AddDefine();
        CreateAsmDefs();
    }

#if UNITY_2017_4_OR_NEWER
    public int callbackOrder { get { return 0; } }
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        AddDefine();
    }
#endif
}

#endif
