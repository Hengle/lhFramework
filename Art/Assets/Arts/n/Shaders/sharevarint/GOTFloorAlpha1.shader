// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GOT/FloorAlpha1" {
	Properties{
	}
		SubShader{
		pass {
		Tags{ "LightMode" = "ForwardBase" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"  
		struct a2v {
			half4 vertex : POSITION;
			half3 normal : NORMAL;
			half4 texcoord : TEXCOORD0;
		};
		struct vertOut {
			half4 pos : SV_POSITION;
#ifdef USING_DIRECTIONAL_LIGHT
			LIGHTING_COORDS(1,1)
#endif
		};
		vertOut vert(a2v v)
		{
			vertOut o;
			o.pos = UnityObjectToClipPos(v.vertex);
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}
		fixed4 frag(vertOut i) :COLOR
		{
		fixed4 c = fixed4(0.0h, 0.0h, 0.0h, 0.0h);
		c.a = 1 - LIGHT_ATTENUATION(i);
		c.rgb *= c.a;
		return c;
		}
			ENDCG
	}//endpass

	Pass{
		Name "Caster"
		Tags{ "LightMode" = "ShadowCaster" "Queue" = "Transparent" "IgnoreProjector" = "Flase" "RenderType" = "Transparent" }
		Offset 1, 1

		Fog{ Mode Off }
		ZWrite On ZTest Less Cull Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		//#pragma multi_compile_shadowcaster
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

	struct v2f {
		V2F_SHADOW_CASTER;
		half2  uv : TEXCOORD1;
	};


	v2f vert(appdata_base v)
	{
		v2f o;
		o.uv = v.texcoord.xy;
		TRANSFER_SHADOW_CASTER(o)
			return o;
	}

	float4 frag(v2f i) : COLOR
	{
		SHADOW_CASTER_FRAGMENT(i)
	}
		ENDCG
	}
	}
}

