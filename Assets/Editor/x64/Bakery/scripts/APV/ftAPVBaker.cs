#if UNITY_EDITOR
#if NATIVE_ARRAYS

#if UNITY_6000_0_OR_NEWER
#if CORE_RP_1703
#define SUPPORTS_APV
#endif
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using Unity.Collections;

#if SUPPORTS_APV
public class ftAPVBaker : AdaptiveProbeVolumes.LightingBaker
{
    int bakedProbeCount;
    int _stepCount;
    
    public override ulong currentStep => (ulong)bakedProbeCount;
    public override ulong stepCount => (ulong)_stepCount;
    
    public NativeArray<SphericalHarmonicsL2> irradianceResults;
    public NativeArray<float> validityResults;
    public NativeArray<Vector4> masks;

    public override NativeArray<SphericalHarmonicsL2> irradiance => irradianceResults;
    public override NativeArray<float> validity => validityResults;
    public override NativeArray<Vector4> occlusion => masks;

    bool occlusionRequested = false;
    bool disposeIrradiance = false;
    public bool irradianceSet = false;
    public bool masksSet = false;
    bool disposeMasks = false;
    bool anyStepsDone = false;
    bool finishOnStep = false;

    public override void Initialize(bool bakeProbeOcclusion, NativeArray<Vector3> probePositions, NativeArray<uint> bakedRenderingLayerMasks)
    {
        Initialize(bakeProbeOcclusion, probePositions);
    }

    public override void Initialize(bool bakeProbeOcclusion, NativeArray<Vector3> probePositions)
    {
        bakedProbeCount = 0;
        var positions = probePositions;
        _stepCount = positions.Length;

        anyStepsDone = false;
        finishOnStep = false;
        irradianceSet = false;
        masksSet = false;

        Debug.Log("Bakery APV baker initialized");

        irradianceResults = new NativeArray<SphericalHarmonicsL2>(positions.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        validityResults = new NativeArray<float>(positions.Length, Allocator.Persistent);//, NativeArrayOptions.UninitializedMemory);

        occlusionRequested = bakeProbeOcclusion;
        if (bakeProbeOcclusion)
        {
            masks = new NativeArray<Vector4>(positions.Length, Allocator.Persistent);
        }

        ftAPV.receivedPositions = probePositions;
        ftAPV.positionsUpdated = true;
    }

    public override bool Step()
    {
        anyStepsDone = true;
        if (finishOnStep) Finish();
        return true;
    }

    public void Finish()
    {
        if (!anyStepsDone)
        {
            finishOnStep = true;
            return;
        }

        if (!irradianceSet && !masksSet)
        {
            AdaptiveProbeVolumes.Cancel(); // will cause a console error because nobody tested this function? at least it does the job
        }

        if (!irradianceSet)
        {
            irradianceResults = new NativeArray<SphericalHarmonicsL2>(_stepCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            disposeIrradiance = true;
            irradianceSet = true;
        }
        if (!masksSet && occlusionRequested)
        {
            masks = new NativeArray<Vector4>(_stepCount, Allocator.Persistent);
            disposeMasks = true;
            masksSet = true;
        }

        Debug.Log("APV bake finished for "+irradianceResults.Length+" SH probes, "+masks.Length+" occlusion probes, "+validityResults.Length+" validity results.");

        bakedProbeCount = _stepCount;
    }

    public override void Dispose()
    {
        if (disposeIrradiance) irradianceResults.Dispose();
        if (disposeMasks) masks.Dispose();
        validityResults.Dispose();
        disposeIrradiance = disposeMasks = false;
    }
}


public class ftAPVSkyBaker : AdaptiveProbeVolumes.SkyOcclusionBaker
{
    int bakedProbeCount;
    int _stepCount;
    public NativeArray<Vector4> skylightL1;
    public NativeArray<Vector3> skylightDir;

    public bool skyDataSet = false;
    public bool disposeData = false;
    
    public override ulong currentStep => (ulong)bakedProbeCount;
    public override ulong stepCount => (ulong)_stepCount;
    public override NativeArray<Vector4> occlusion => skylightL1;
    public override NativeArray<Vector3> shadingDirections => skylightDir;

    public override void Initialize(ProbeVolumeBakingSet bakingSet, NativeArray<Vector3> probePositions)
    {
        bakedProbeCount = 0;
        var positions = probePositions;
        _stepCount = positions.Length;

        Debug.Log("Bakery APV baker (sky occlusion) initialized");

        ftAPV.skylightRequested = true;
        skyDataSet = false;
    }

    public override bool Step()
    {
        return true;
    }

    public void Finish()
    {
        if (!skyDataSet)
        {
            skylightL1 =  new NativeArray<Vector4>(_stepCount, Allocator.Persistent);
            skylightDir = new NativeArray<Vector3>(_stepCount, Allocator.Persistent);
            disposeData = true;
            skyDataSet = true;
        }

        Debug.Log("APV (sky occlusion) bake finished for "+skylightL1.Length+" SH probes, "+skylightDir.Length+" directions.");

        bakedProbeCount = _stepCount;
    }

    public override void Dispose()
    {
        if (disposeData)
        {
            skylightL1.Dispose();
            skylightDir.Dispose();
        }
        disposeData = false;
    }
}
#endif

public class ftAPV
{
#if SUPPORTS_APV
    static ftAPVBaker baker;
    static ftAPVSkyBaker bakerSky;
#endif

    public static NativeArray<Vector3> receivedPositions;
    public static bool positionsUpdated;
    public static bool skylightRequested;
    public static float minVoxelSize = 0.5f;

#if SUPPORTS_APV
    static ProbeVolumeBakingSet CreateOrReplaceAsset(ProbeVolumeBakingSet src, string path)
    {
        var dest = AssetDatabase.LoadAssetAtPath<ProbeVolumeBakingSet>(path);
        if (dest == null)
        {
            AssetDatabase.CreateAsset(src, path);
            dest = src;
        }
        else
        {
            var guids = new List<string>();
            foreach(var guid in dest.sceneGUIDs)
            {
                guids.Add(guid);
            }
            for(int i=0; i<guids.Count; i++)
            {
                dest.RemoveScene(guids[i]);
            }

            //EditorUtility.CopySerialized(src, dest); // will break ProbeReferenceVolume
            var bset = dest;
            var bakingSet = src;
            bset.minDistanceBetweenProbes = bakingSet.minDistanceBetweenProbes;
            bset.minRendererVolumeSize = bakingSet.minRendererVolumeSize;
            bset.probeOffset = bakingSet.probeOffset;
            bset.renderersLayerMask = bakingSet.renderersLayerMask;
            bset.simplificationLevels = bakingSet.simplificationLevels;
            bset.skyOcclusion = bakingSet.skyOcclusion;

            EditorUtility.SetDirty(dest);
        }
        return dest;
    }

    static string GenerateLightingDataAssetName()
    {
        var sceneCount = SceneManager.sceneCount;
        var assetName = "";
        var assetNameHashPart = "";
        for(int i=0; i<sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            if (i == 0)
            {
                assetName += s.name;
            }
            else
            {
                assetNameHashPart += s.name;
                if (i < sceneCount - 1) assetNameHashPart += "__";
            }
        }
        assetName += "_" + assetNameHashPart.GetHashCode();
        return assetName;
    }

    static bool Setup(bool skyOcclusion)
    {
        if (ProbeReferenceVolume.instance == null)
        {
            Debug.LogError("ProbeReferenceVolume.instance is null");
            return false;
        }

        var bakingSet = ProbeReferenceVolume.instance.currentBakingSet;

        // Create our own baking set, keep all placement settings
        var bset = ScriptableObject.CreateInstance<ProbeVolumeBakingSet>();
        bset.name = "BakeryBakingSet_" + GenerateLightingDataAssetName();
        //bset.hideFlags = HideFlags.NotEditable | HideFlags.HideAndDontSave;
        
        var setDefFunc = bset.GetType().GetMethod("SetDefaults", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (setDefFunc == null)
        {
            Debug.LogError("Can't get ProbeVolumeBakingSet.SetDefaults()");
            return false;
        }
        setDefFunc.Invoke(bset, null);

        if (bakingSet != null)
        {
            bset.minDistanceBetweenProbes = bakingSet.minDistanceBetweenProbes;
            bset.minRendererVolumeSize = bakingSet.minRendererVolumeSize;
            bset.probeOffset = bakingSet.probeOffset;
            bset.renderersLayerMask = bakingSet.renderersLayerMask;
            bset.simplificationLevels = bakingSet.simplificationLevels;
        }

        // Patch baking settings
        bset.skyOcclusion = skyOcclusion;

        minVoxelSize = bset.minDistanceBetweenProbes;

        // Save the baking set
        var mainScene = SceneManager.GetActiveScene();
        string path = "Assets";
        if (!string.IsNullOrEmpty(mainScene.path))
        {
            path = (System.IO.Path.GetDirectoryName(mainScene.path) + "/" + mainScene.name);
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        }
        path += "/" + bset.name + ".asset";

        bset = CreateOrReplaceAsset(bset, path);
        //AssetDatabase.CreateAsset(bset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Add active scene
        var sceneCount = EditorSceneManager.sceneCount;
        for(int j=0; j<sceneCount; j++)
        {
            var scene = EditorSceneManager.GetSceneAt(j);
            if (!scene.isLoaded) continue;
            
            var sceneToAddGUID = AssetDatabase.AssetPathToGUID(scene.path);

            if (!bset.TryAddScene(sceneToAddGUID))
            {
                bool solved = false;
                var otherSetsGUIDs = AssetDatabase.FindAssets("t:" + typeof(ProbeVolumeBakingSet).Name);
                foreach (var otherSetsGUID in otherSetsGUIDs)
                {
                    var otherSetPath = AssetDatabase.GUIDToAssetPath(otherSetsGUID);
                    var otherSet = AssetDatabase.LoadAssetAtPath<ProbeVolumeBakingSet>(otherSetPath);
                    if (otherSet != null)
                    {
                        foreach(var sceneGUID in otherSet.sceneGUIDs)
                        {
                            if (sceneGUID == sceneToAddGUID)
                            {
                                if (EditorUtility.DisplayDialog("Bakery", "The scene " + scene.path + " is part of the Baking Set " + otherSetPath + ". Do you want to move the scene to a new set?", "OK", "Cancel"))
                                {
                                    otherSet.RemoveScene(sceneGUID);
                                    if (!bset.TryAddScene(sceneToAddGUID))
                                    {
                                        Debug.LogError("Scene  " + scene.path + " was removed from Baking Set " + otherSetPath + ", but TryAddScene still failed.");
                                        return false;
                                    }
                                    else
                                    {
                                        Debug.Log("Scene " + scene.name + " moved to a new Baking Set");
                                        solved = true;
                                        break;
                                    }

                                }
                                else
                                {
                                    Debug.LogError("Scene  " + scene.path + " is alreasy used in Baking Set " + otherSetPath + ". Delete the Baking Set or remove the scene from it.");
                                    return false;
                                }
                            }
                        }
                        if (solved) break;
                    }
                }
                if (!solved)
                {
                    Debug.LogError("ProbeVolumeBakingSet.TryAddScene() failed for unknown reason with scene " + scene.path);
                    return false;
                }
            }
        }

        // Set as active
        ProbeReferenceVolume.instance.SetActiveBakingSet(bset);

        return true;
    }

#endif

    public static bool Run(bool skyOcclusion)
    {
#if SUPPORTS_APV

        if (!Setup(skyOcclusion)) return false;

        baker = new ftAPVBaker();
        bakerSky = new ftAPVSkyBaker();
        positionsUpdated = false;
        skylightRequested = false;
        AdaptiveProbeVolumes.SetLightingBakerOverride(baker);
        AdaptiveProbeVolumes.SetSkyOcclusionBakerOverride(skyOcclusion ? bakerSky : null);
        if (!AdaptiveProbeVolumes.BakeAsync())
        {
            Debug.LogError("AdaptiveProbeVolumes.BakeAsync() failed");
            return false;
        }
#endif
        return true;
    }

    public static bool IsBaking()
    {
#if SUPPORTS_APV
        return AdaptiveProbeVolumes.isRunning;
#else
        return false;
#endif
    }

    public static void SetSHs(NativeArray<SphericalHarmonicsL2> shs)
    {
#if SUPPORTS_APV
        baker.irradianceResults = shs;
        baker.irradianceSet = true;
#endif
    }

    public static void SetMasks(NativeArray<Vector4> masks)
    {
#if SUPPORTS_APV
        baker.masks = masks;
        baker.masksSet = true;
#endif
    }

    public static void SetSkyOcclusion(NativeArray<Vector4> skylightL1, NativeArray<Vector3> skylightDir)
    {
#if SUPPORTS_APV
        bakerSky.skylightL1 = skylightL1;
        bakerSky.skylightDir = skylightDir;
        bakerSky.skyDataSet = true;
#endif
    }

    public static void Finish()
    {
#if SUPPORTS_APV
        baker.Finish();
        bakerSky.Finish();
        Cleanup();
#endif
    }

    public static void Cleanup()
    {
#if SUPPORTS_APV
        AdaptiveProbeVolumes.SetLightingBakerOverride(null);
        AdaptiveProbeVolumes.SetSkyOcclusionBakerOverride(null);
#endif
    }

    public static bool AnyAPVsInScene()
    {
#if SUPPORTS_APV
        var anyAPVs = GameObject.FindObjectsOfType<ProbeVolume>();
        foreach(var apv in anyAPVs)
        {
            if (apv.enabled && apv.gameObject.activeInHierarchy) return true;
        }
#endif
        return false;
    }
}

#endif
#endif