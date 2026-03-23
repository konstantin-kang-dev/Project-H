using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ftDetectSettings
{
    [DllImport ("frender", CallingConvention=CallingConvention.Cdecl)]
    public static extern System.IntPtr RunLocalProcess([MarshalAs(UnmanagedType.LPWStr)]string commandline, bool setWorkDir);

    [DllImport ("frender", CallingConvention=CallingConvention.Cdecl)]
    public static extern bool IsProcessFinished(System.IntPtr proc);

    [DllImport ("frender", CallingConvention=CallingConvention.Cdecl)]
    public static extern int GetProcessReturnValueAndClose(System.IntPtr proc);

    [DllImport ("simpleProgressBar", CallingConvention=CallingConvention.Cdecl)]
    public static extern int simpleProgressBarShow(string header, string msg, float percent, float step, bool onTop);

    [DllImport ("simpleProgressBar", CallingConvention=CallingConvention.Cdecl)]
    public static extern bool simpleProgressBarCancelled();

    [DllImport ("simpleProgressBar", CallingConvention=CallingConvention.Cdecl)]
    public static extern void simpleProgressBarEnd();

    static IEnumerator progressFunc;
    static int lastReturnValue = -1;
    static bool userCanceled = false;

    static bool runsRTX6, runsRTX9, runsNonRTX, runsOptix5, runsOptix6, runsOptix7, runsOIDN, runsOIDN2;

    const string progressHeader = "Detecting compatible configuration";

    static void ShowProgress(string msg, float percent)
    {
        simpleProgressBarShow(progressHeader, msg, percent, 0, true);
    }

    static void ValidateFileAttribs(string file)
    {
        if (File.Exists(file))
        {
            var attribs = File.GetAttributes(file);
            if ((attribs & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(file, attribs & ~FileAttributes.ReadOnly);
            }
        }
    }

#if BAKERY_TOOLSMENU
    [MenuItem("Tools/Bakery/Utilities/Detect optimal settings", false, 54)]
#else
    [MenuItem("Bakery/Utilities/Detect optimal settings", false, 54)]
#endif
    public static void DetectCompatSettings()
    {
        var bakeryPath = ftLightmaps.GetEditorPath();
        ValidateFileAttribs(bakeryPath+"/hwtestdata/image.lz4");
        ValidateFileAttribs(bakeryPath+"/hwtestdata/light_HDR.lz4");

        progressFunc = DetectCoroutine();
        EditorApplication.update += DetectUpdate;
    }

    static IEnumerator DetectCoroutine()
    {
        float stages = 8;
        float step = 1.0f / stages;
        float progress = 0;
        IEnumerator crt;

        ShowProgress("Testing: RTX ray-tracing (OptiX 6.0)", progress);
        crt = ProcessCoroutine("ftraceRTX.exe /sun hwtestdata light 4 0 0 direct0.bin");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsRTX6 = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: RTX ray-tracing (OptiX 9.0)", progress);
        crt = ProcessCoroutine("ftraceRTX9.exe /sun hwtestdata light 4 0 0 direct0.bin");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsRTX9 = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: non-RTX ray-tracing", progress);
        crt = ProcessCoroutine("ftrace.exe /sun hwtestdata light 4 0 0 direct0.bin");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsNonRTX = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: OptiX 5.1 denoiser", progress);
        crt = ProcessCoroutine("denoiserLegacy c hwtestdata/image.lz4 hwtestdata/image.lz4 16 0");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsOptix5 = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: OptiX 6.0 denoiser", progress);
        crt = ProcessCoroutine("denoiser c hwtestdata/image.lz4 hwtestdata/image.lz4 16 0");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsOptix6 = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: OptiX 7.2 denoiser", progress);
        crt = ProcessCoroutine("denoiser72 c hwtestdata/image.lz4 hwtestdata/image.lz4 16 0");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsOptix7 = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: OpenImageDenoise", progress);
        crt = ProcessCoroutine("denoiserOIDN c hwtestdata/image.lz4 hwtestdata/image.lz4 16 0");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsOIDN = lastReturnValue==0;
        progress += step;

        ShowProgress("Testing: OpenImageDenoise2 (CUDA)", progress);
        crt = ProcessCoroutine("denoiserOIDN2 c hwtestdata/image.lz4 hwtestdata/image.lz4 16 0");
        while (crt.MoveNext()) yield return null;
        if (userCanceled) yield break;
        runsOIDN2 = lastReturnValue==0;
        progress += step;

        if (!runsRTX6 && !runsRTX9 && !runsNonRTX)
        {
            ShowProgress("Checking hardware/driver capabilities", progress);
            crt = ProcessCoroutine("ftraceRTX9 hwtest");
            while (crt.MoveNext()) yield return null;
            if (userCanceled) yield break;
            simpleProgressBarEnd();
            
            const int HWTEST_FLAG_NOHW = 2;
            const int HWTEST_FLAG_NEWDRIVER = 4;
            const int HWTEST_FLAG_NOSM = 8;
            //const int HWTEST_FLAG_UNKNOWN  = 16;
            const int HWTEST_FLAG_NOCUDA  = 32;

            const int allMask = 2|4|8|16|32;

            int hwtestResult = lastReturnValue;

            string msg = "Both RTX (old and new) and non-RTX lightmappers failed to run.";

            if ((hwtestResult & HWTEST_FLAG_NOHW) != 0 ||
                (hwtestResult & ~allMask) != 0)
            {
                msg += "Make sure you are using NVIDIA GPU and the drivers are up to date. If you are using pre-RTX (< 2xxx) hardware, please try installing an older driver (e.g. 581.80).";
            }
            else
            {
                if ((hwtestResult & HWTEST_FLAG_NOSM) != 0)
                {
                    msg += "\n\nAt least one device has compute capability < 5.2.";
                }

                if ((hwtestResult & HWTEST_FLAG_NOCUDA) != 0)
                {
                    msg += "\n\nCUDA 12.0 is not supported.";
                }

                if ((hwtestResult & HWTEST_FLAG_NEWDRIVER) != 0)
                {
                    msg += "\n\nDriver version is >= 590. Nvidia removed support for OptiX6 in 590. If you are using pre-RTX (< 2xxx) hardware, please try installing an older driver (e.g. 581.80).";
                }
                else
                {
                    msg += "\n\nIf you are using Nvidia driver >= 590, note that Nvidia removed support for OptiX6 in 590. If Bakery worked before or your GPU is in the supported range, try installing an older driver (e.g. 581.80).";
                }

                if (EditorUtility.DisplayDialog("Error", msg, "Open driver page", "OK"))
                {
                    Application.OpenURL("https://www.nvidia.com/en-eu/geforce/drivers/results/257569/");
                }
                yield break;
            }

            EditorUtility.DisplayDialog("Error", msg, "OK");

            yield break;
        }
        else
        {
            simpleProgressBarEnd();
        }

        string str = "Testing results:\n\n";
        str += "RTX ray-tracing (OptiX 9.0): " + (runsRTX9 ? "yes" : "no") + "\n";
        str += "RTX ray-tracing (OptiX 6.0): " + (runsRTX6 ? "yes" : "no") + "\n";
        str += "Non-RTX ray-tracing: " + (runsNonRTX ? "yes" : "no") + "\n";
        str += "OptiX 5.1 denoiser: " + (runsOptix5 ? "yes" : "no") + "\n";
        str += "OptiX 6.0 denoiser: " + (runsOptix6 ? "yes" : "no") + "\n";
        str += "OptiX 7.2 denoiser: " + (runsOptix7 ? "yes" : "no") + "\n";
        str += "OpenImageDenoise: " + (runsOIDN ? "yes" : "no") + "\n";
        str += "OpenImageDenoise2 (CUDA): " + (runsOIDN2 ? "yes" : "no") + "\n";

        str += "\n";
        str += "Recommended RTX mode: ";
        if (runsRTX6 && runsNonRTX)
        {
            str += "ON if you are using a GPU with RT acceleration (e.g. 2xxx or 3xxx GeForce series), OFF otherwise.\n";
        }
        else if (runsRTX6)
        {
            str += "ON (OptiX 6.0)\n";
        }
        else if (runsRTX9)
        {
            str += "ON (OptiX 9.0)\n";
        }
        else if (runsNonRTX)
        {
            str += "OFF\n";
        }

        str += "\n";
        str += "Recommended denoiser: ";
        if (runsOptix5)
        {
            // OptiX 5.1 has stable quality since release, but not supported on 30XX
            str += "OptiX 5.1\n";
        }
        else if (runsOIDN2)
        {
            // OIDN2+CUDA is the best option on modern HW
            str += "OpenImageDenoise2\n";
        }
        else if (runsOIDN)
        {
            // OIDN is stable and pretty good, but might be slower
            str += "OpenImageDenoise\n";
        }
        // OptiX 6 and 7.2 should run on 30XX, but quality is sometimes questionable IF driver is newer than 442.50
        // as the network is now part of the driver.
        // On older drivers they should work similar to 5.1.
        else if (runsOptix7)
        {
            str += "OptiX 7.2\n";
        }
        else if (runsOptix6)
        {
            str += "OptiX 6.0\n";
        }
        else
        {
            str += "all denoiser tests failed. Try updating GPU drivers.\n";
        }

        var bakeryRuntimePath = ftLightmaps.GetRuntimePath();
        var gstorage = AssetDatabase.LoadAssetAtPath(bakeryRuntimePath + "ftGlobalStorage.asset", typeof(ftGlobalStorage)) as ftGlobalStorage;
        if (gstorage == null) Debug.LogError("Can't find global storage");
        var storage = ftRenderLightmap.FindRenderSettingsStorage();

        if (gstorage != null)
        {
            gstorage.foundCompatibleSetup = true;
            gstorage.gpuName_2 = SystemInfo.graphicsDeviceName;
            gstorage.runsNonRTX = runsNonRTX;
            gstorage.runsRTX6 = runsRTX6;
            gstorage.runsRTX9 = runsRTX9;
            gstorage.alwaysEnableRTX = false;
            gstorage.runsOptix5 = runsOptix5;
            gstorage.runsOptix6 = runsOptix6;
            gstorage.runsOptix7 = runsOptix7;
            gstorage.runsOIDN = runsOIDN;
            gstorage.runsOIDN2 = runsOIDN2;
        }

        if (!EditorUtility.DisplayDialog("Results", str, "OK (Keep current)", "Set recommended as default"))
        {
            if (runsRTX6 && runsNonRTX)
            {
                gstorage.renderSettingsRTXMode = EditorUtility.DisplayDialog("Question", "Does your GPU have RT cores (set RTX mode as default)?", "Yes", "No");
            }
            else if (runsRTX6)
            {
                gstorage.renderSettingsRTXMode = true;
                gstorage.renderSettingsExportTerrainAsHeightmap = false;
            }
            else if (runsRTX9)
            {
                gstorage.renderSettingsRTXMode = true;
                gstorage.renderSettingsExportTerrainAsHeightmap = false;
            }
            else
            {
                gstorage.renderSettingsRTXMode = false;
            }

            if (runsOIDN2)
            {
                gstorage.renderSettingsDenoiserType = (int)ftGlobalStorage.DenoiserType.OpenImageDenoise2;
            }
            else if (runsOptix5)
            {
                gstorage.renderSettingsDenoiserType = (int)ftGlobalStorage.DenoiserType.Optix5;
            }
            else if (runsOIDN)
            {
                gstorage.renderSettingsDenoiserType = (int)ftGlobalStorage.DenoiserType.OpenImageDenoise;
            }
            else if (runsOptix7)
            {
                gstorage.renderSettingsDenoiserType = (int)ftGlobalStorage.DenoiserType.Optix7;
            }
            else if (runsOptix6)
            {
                gstorage.renderSettingsDenoiserType = (int)ftGlobalStorage.DenoiserType.Optix6;
            }

            EditorUtility.SetDirty(gstorage);
            Debug.Log("Default settings saved");

            if (storage != null)
            {
                storage.renderSettingsRTXMode = gstorage.renderSettingsRTXMode;
                if (storage.renderSettingsRTXMode) gstorage.renderSettingsExportTerrainAsHeightmap = false;
                storage.renderSettingsDenoiserType = gstorage.renderSettingsDenoiserType;
            }
        }

        var bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : new ftRenderLightmap();
        bakery.LoadRenderSettings();
    }

    static void DetectUpdate()
    {
        if (!progressFunc.MoveNext())
        {
            EditorApplication.update -= DetectUpdate;
        }
    }

    static IEnumerator ProcessCoroutine(string cmd)
    {
        var exeProcess = RunLocalProcess(cmd, true);
        if (exeProcess == (System.IntPtr)null)
        {
            lastReturnValue = -1;
            yield break;
        }
        while(!IsProcessFinished(exeProcess))
        {
            yield return null;
            userCanceled = simpleProgressBarCancelled();
            if (userCanceled)
            {
                simpleProgressBarEnd();
                yield break;
            }
        }
        lastReturnValue = GetProcessReturnValueAndClose(exeProcess);
    }
}

