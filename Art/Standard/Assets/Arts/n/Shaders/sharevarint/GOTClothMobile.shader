// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
Shader "GOT/ClothMobile" {
	Properties{
		_Diffuse("Diffuse", Color) = (1,1,1,1)
		_MainTex("Base 2D", 2D) = "white"{}
	_BumpMap("Bump Map", 2D) = "bump"{}
	_MaskTex("Mask (R)Alpha (G)Metallic (B)Smoothness", 2D) = "white" {}
	_Metallic("Metallic", Range(0.0, 1.0)) = 0.222
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
		_EnvMap("Env Map", CUBE) = "black" {}
	_EnvPower("Env Power", Range(0.0, 1.0)) = 0.0
		_Cutoff("Cut off", Range(0, 1)) = 0.5
		_SmoothnessSplit("Smoothness Split", Range(0.0, 1.0)) = 1.0
		_LightLimit("Light Limit", Range(0.0, 1.0)) = .97
	}
		CGINCLUDE
#include "Lighting.cginc"  
#include "AutoLight.cginc"
		fixed4 _Diffuse;
	sampler2D _MainTex;
	fixed4 _MainTex_ST;
	sampler2D _BumpMap;
	sampler2D _MaskTex;
	half _Metallic;
	half _Smoothness;
	half _SmoothnessSplit;
	half _LightLimit;
	samplerCUBE _EnvMap;
	half _EnvPower;
	half _Cutoff;
	struct v2f
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half3 normalDir : TEXCOORD1;
		half3 tangentDir : TEXCOORD2;
		half3 bitangentDir : TEXCOORD3;
		fixed3 viewDir : TEXCOORD4;
		half4 posWorld : TEXCOORD5;
		LIGHTING_COORDS(6, 2)
	};

	fixed3 fragLight(fixed3 diffColor, half3 N, half3 V, half3 L, half roughness, fixed3 ambient, fixed3 light, fixed3 specColor)
	{
		fixed3 c;

		half3 H = Unity_SafeNormalize(L + V);
		half nl = saturate(dot(N, L));
		half nh = saturate(dot(N, H));
		//half lh = saturate(dot(L, H));
		//half nv = saturate(dot(N, V));

		half a = roughness;
		half a2 = a*a;
		half d = nh * nh * (a2 - 1.0h) + 1.00001h;

#ifdef UNITY_COLORSPACE_GAMMA
		half specularTerm = a / (max(0.32h, 1.0h) * (1.5h + roughness) * d);
#else
		half specularTerm = a2 / (max(0.1h, 1.0h /*lh*lh*/) * (roughness + 0.5h) * (d * d) * 4.0h);
#endif
		c = ambient * diffColor + diffColor * light * nl + specularTerm * specColor * light * nl;
		return c;
	}

	v2f vert(appdata_tan v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

		o.normalDir = UnityObjectToWorldNormal(v.normal);

		//half3 tangent = normalize(cross(v.normal, half3(0, 0, 1)));
		//if(length(tangent) == 0) tangent = normalize(cross(v.normal, half3(0, 1, 0)));

		o.tangentDir = normalize(mul(unity_ObjectToWorld, half4(v.tangent.xyz, 0.0h)).xyz);
		o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);

		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		o.viewDir = WorldSpaceViewDir(v.vertex);
		TRANSFER_VERTEX_TO_FRAGMENT(o);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed3 mask = tex2D(_MaskTex, i.uv);
	    clip(mask.r - _Cutoff);
		half3 V = normalize(i.viewDir);
		fixed3 albedo = tex2D(_MainTex, i.uv);

		half3 N = normalize(mul(UnpackNormal(tex2D(_BumpMap, i.uv)), half3x3(i.tangentDir, i.bitangentDir, i.normalDir)));
		/* 
		fixed4 normal = tex2D(_BumpMap, i.uv);
		half spec = normal.z;
		spec = (1.0h - spec) * .5h + _Adjust;
		normal.xy = normal.xy * 2.0h - 1.0h;
		normal.z = sqrt(1.0h - saturate(dot(normal.xy, normal.xy)));
		half3x3 normalM = half3x3(i.tangentDir, i.bitangentDir, i.normalDir);
		half3 N = normalize(mul(normal.xyz, normalM));
		*/

		half smoothness = _Smoothness * mask.b;
		half perceptualRoughness = 1.0h - smoothness;
		half roughness = perceptualRoughness * perceptualRoughness;

		half oneMinusReflectivity;
		half3 specColor;
		fixed3 reflectColor = texCUBE(_EnvMap, normalize(reflect(-V, N))).rgb;
		albedo = lerp(albedo, reflectColor, _EnvPower * _Smoothness * _Metallic) * _Diffuse.xyz;
		half3 diffColor = DiffuseAndSpecularFromMetallic(albedo, _Metallic * mask.g, /*out*/ specColor, /*out*/ oneMinusReflectivity);
		//diffColor = PreMultiplyAlpha (diffColor, mask.r, oneMinusReflectivity, /*out*/ mask.r);

		half4 aLight = half4(normalize(_WorldSpaceLightPos0.xyz), 1.0h);
		fixed4 c = fixed4(0.0h, 0.0h, 0.0h, 1.0h);
		c.rgb += fragLight(diffColor, N, V, aLight.xyz, roughness, UNITY_LIGHTMODEL_AMBIENT.xyz, _LightColor0.xyz * aLight.w * LIGHT_ATTENUATION(i), specColor);

		aLight.xyz = half3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x) - i.posWorld;
		aLight.w = 1.0h / (1.0h + max(aLight.x * aLight.x + aLight.y * aLight.y + aLight.z * aLight.z, 0.000001h) * unity_4LightAtten0.x);
		aLight.xyz = normalize(aLight.xyz);
		aLight.w *= unity_LightColor[0].a;
		c.rgb += fragLight(diffColor, N, V, aLight.xyz, roughness, fixed3(0.0h, 0.0h, 0.0h), unity_LightColor[0].rgb * aLight.w, specColor);

		aLight.xyz = half3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y) - i.posWorld;
		aLight.w = 1.0h / (1.0h + max(aLight.x * aLight.x + aLight.y * aLight.y + aLight.z * aLight.z, 0.000001h) * unity_4LightAtten0.y);
		aLight.xyz = normalize(aLight.xyz);
		aLight.w *= unity_LightColor[1].a;
		c.rgb += fragLight(diffColor, N, V, aLight.xyz, roughness, fixed3(0.0h, 0.0h, 0.0h), unity_LightColor[1].rgb * aLight.w, specColor);

		c.rgb = mask.b > _SmoothnessSplit ? c.rgb : min(c.rgb, _LightLimit);
		//c.a = mask.r;
		return c;
	}

	struct v2fShadow {
		V2F_SHADOW_CASTER;
		float2  uv : TEXCOORD1;
	};
	uniform float4 _MaskTex_ST;
	v2fShadow vertShadow(appdata_base v) {
		v2fShadow o;
		TRANSFER_SHADOW_CASTER(o)
			o.uv = TRANSFORM_TEX(v.texcoord, _MaskTex);
		return o;
	}
	float4 fragShadow(v2fShadow i) : COLOR{
		fixed alpha = tex2D(_MaskTex, i.uv).r;
	clip(alpha - _Cutoff);
	SHADOW_CASTER_FRAGMENT(i)
	}
		ENDCG
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
			Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
#pragma multi_compile_fwdbase
#pragma vertex vert  
#pragma fragment frag     
			ENDCG
		}

			Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }
			Offset 1, 1
			Fog{ Mode Off }
			ZWrite On ZTest LEqual Cull Off
			CGPROGRAM
			//#pragma multi_compile_shadowcaster
#pragma vertex vertShadow
#pragma fragment fragShadow
#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
	}
}