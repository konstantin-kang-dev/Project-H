#if UNITY_EDITOR
#if UNITY_2022_3_OR_NEWER
#if ENTITIES_EXIST
#if ENTITIES_GFX_EXIST

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Entities.Conversion;
using Unity.Transforms;
using UnityEngine;
using UnityEditor;
using Unity.Rendering;

[UpdateInGroup(typeof(PreBakingSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
partial class ftECSCompat : SystemBase
{
    static int MaxSize(Texture2D lm, int maxSize)
    {
        if (lm == null) return maxSize;
        if (lm.width > maxSize) maxSize = lm.width;
        if (lm.height > maxSize) maxSize = lm.height;
        return maxSize;
    }

    static Texture2D UpscaleIfNeeded(Texture2D lm, int maxSize, int maxMipCount)
    {
        if (lm == null) return null;

        bool rebuild = false;
        if (lm.width != maxSize || lm.height != maxSize)
        {
            Debug.LogWarning("Lightmap "+lm.name+" is smaller than the largest one ("+lm.width+", "+lm.height+" vs "+maxSize+"). In ECS all lightmaps must have the same size. Upsampling...\nTo prevent further upsampling, set Min Resolution = Max Resolution before lightmap baking.");
            rebuild = true;
        }
        else if (lm.mipmapCount != maxMipCount)
        {
            Debug.LogWarning("Lightmap "+lm.name+" needs "+maxMipCount+" mipmaps for ECS (has "+lm.mipmapCount+"). Computing...");
            rebuild = true;
        }

        if (rebuild)
        {
            var rt = new RenderTexture(maxSize, maxSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            rt.Create();
            Graphics.Blit(lm, rt);
            var tex = new Texture2D(maxSize, maxSize, TextureFormat.RGBAFloat, true, false);
            Graphics.SetRenderTarget(rt);
            tex.ReadPixels(new Rect(0,0,maxSize,maxSize), 0, 0, true);
            tex.Apply();
            Graphics.SetRenderTarget(null);
            rt.Release();
            EditorUtility.CompressTexture(tex, lm.format, UnityEditor.TextureCompressionQuality.Best);
            lm = tex;
        }

        return lm;
    }

    protected override void OnUpdate()
    {
        ftLightmaps.RefreshFull();

        var lms = LightmapSettings.lightmaps;

        int maxSize = 0;
        for(int i=0; i<lms.Length; i++)
        {
            maxSize = MaxSize(lms[i].lightmapColor, maxSize);
            maxSize = MaxSize(lms[i].lightmapDir, maxSize);
            maxSize = MaxSize(lms[i].shadowMask, maxSize);
        }

        int curRes = maxSize;
        int maxMipCount = 0;
        while(curRes > 0)
        {
            curRes /= 2;
            maxMipCount++;
        }

        for(int i=0; i<lms.Length; i++)
        {
            lms[i].lightmapColor = UpscaleIfNeeded(lms[i].lightmapColor, maxSize, maxMipCount);
            lms[i].lightmapDir = UpscaleIfNeeded(lms[i].lightmapDir, maxSize, maxMipCount);
            lms[i].shadowMask = UpscaleIfNeeded(lms[i].shadowMask, maxSize, maxMipCount);
        }

        LightmapSettings.lightmaps = lms;
    }
}

#endif
#endif
#endif
#endif