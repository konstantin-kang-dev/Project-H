Shader "02_Selection/Fullscreen"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Float) = 2
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1)
    }

    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float  _OutlineWidth;
    float4 _OutlineColor;

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

        // Current pixel — is it part of the object?
        float4 c        = LoadCustomColor(posInput.positionSS);
        float  customD  = LoadCustomDepth(posInput.positionSS);
        float  cameraD  = LoadCameraDepth(posInput.positionSS);

        // This pixel is on the visible surface of the object — skip, don't touch it
        bool onObject = c.a > 0.5 && cameraD >= customD;
        if (onObject)
            return float4(0, 0, 0, 0);

        // Sample neighbours to detect edge
        float2 uvOffsetPerPixel = 1.0 / _ScreenSize.xy;

        float2 dirs[4] = {
            float2( 1,  0),
            float2(-1,  0),
            float2( 0,  1),
            float2( 0, -1)
        };

        bool nearVisibleObject = false;

        for (int i = 0; i < 4; ++i)
        {
            for (float r = 1.0; r <= _OutlineWidth; r += 1.0)
            {
                float2 sampleUV = posInput.positionNDC + uvOffsetPerPixel * r * dirs[i];
                float2 sampleSS = sampleUV * _ScreenSize.xy;

                float4 s       = SampleCustomColor(sampleUV);
                float  sCustD  = LoadCustomDepth(sampleSS);
                float  sCamD   = LoadCameraDepth(sampleSS);

                // Neighbour is a visible part of the object
                if (s.a > 0.5 && sCamD >= sCustD)
                {
                    nearVisibleObject = true;
                    break;
                }
            }
            if (nearVisibleObject) break;
        }

        if (!nearVisibleObject)
            return float4(0, 0, 0, 0);

        return _OutlineColor;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
