Shader "Hidden/ftUnlitTerrainOpaque"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" { }
        _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
    }
	SubShader
	{
        Pass
        {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off
            CGPROGRAM

            #include"UnityStandardMeta.cginc"

            float4 frag_meta2 (v2f_meta i): SV_Target
            {
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);
                o.Albedo = tex2D(_MainTex, i.uv);
                return UnityMetaFragment(o);
            }

            #pragma vertex vert_meta
            #pragma fragment frag_meta2
            ENDCG
        }

        Tags {"Queue" = "Transparent+1" }
        ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex vs
			#pragma fragment ps
			#include "UnityCG.cginc"

            sampler2D _MainTex;

			struct pi
			{
				float4 Position : SV_POSITION;
				float2 TexCoords : TEXCOORD0;
			};

			void vs(in appdata_full IN, out pi OUT)
			{
                OUT.Position = UnityObjectToClipPos(IN.vertex);
				OUT.TexCoords = IN.texcoord.xy;
			}

			float4 ps( in pi IN ) : COLOR
			{
				float4 tex = tex2D(_MainTex, IN.TexCoords);
                tex.a = 1;
                return tex;
			}
			ENDCG
		}
	}
    Dependency "BaseMapShader"    = "Hidden/ftUnlitTerrain"
    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Diffuse-BaseGen"
}
