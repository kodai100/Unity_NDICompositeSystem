﻿Shader "Custom/AlphaComposite"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Lighting Off Cull Off ZTest Always ZWrite Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Blending.cginc"

			sampler2D _MainTex;
			sampler2D _Layer1Tex;
			sampler2D _Layer2Tex;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 texture1 = tex2D(_Layer1Tex, i.uv);
				fixed4 texture2 = tex2D(_Layer2Tex, i.uv);

				fixed4 col = Alpha(texture1, texture2);	// Composition

				return col;
			}

			ENDCG
		}
	}
}