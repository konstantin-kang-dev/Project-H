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

        float4 c = LoadCustomColor(posInput.positionSS);

        if (c.a > 0.5)
            return float4(0, 0, 0, 0);

        float  currentCamD      = LoadCameraDepth(posInput.positionSS);
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

                float4 s      = SampleCustomColor(sampleUV);
                float  sCustD = LoadCustomDepth(sampleSS);
                float  sCamD  = LoadCameraDepth(sampleSS);

                // Сосед — видимый пиксель объекта
                // И текущий пиксель находится перед или на том же уровне что объект
                if (s.a > 0.5 && sCamD >= sCustD && currentCamD <= sCustD)
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