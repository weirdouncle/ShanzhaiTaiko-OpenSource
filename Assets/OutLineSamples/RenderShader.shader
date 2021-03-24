Shader "Hidden/UnityFx/OutlineTransparent"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	HLSLINCLUDE

	#include "UnityCG.cginc"
	sampler2D _MainTex;
	half4 frag(v2f_img i) : SV_Target
	{
		half4 col = tex2D(_MainTex, i.uv);
		clip(col.a - 0.9);
		return 1;
	}

	ENDHLSL

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			ENDHLSL
		}
	}
}
