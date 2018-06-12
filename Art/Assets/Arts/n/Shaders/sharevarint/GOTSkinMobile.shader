
Shader "GOT/GOTSkinMobile" {
	Properties{
		_Color("Diffuse Tint", color) = (1, 0.859, 0.859, 0)
		_MainTex("Base (RGB) Boob Mask (A)", 2D) = "white" {}
	_BumpMap("Bump Map", 2D) = "bump"{}
	_MaskTex("Mask (R)Alpha (G)Spec pow(B)Blur", 2D) = "white" {}
	_BRDFTex("Brdf Map", 2D) = "gray" {}
	_Fresnel("Spec Pow", Range(0, 1)) = 0.028
		_Smoothness("Smoothness", Range(0.1, 1)) = 0.28
		_SkinColor("Skin Color", color) = (1, 1, 1, 0)
		_DeepColor("Deep Color", color) = (0.463, 0.243, 0.224, 0)
		_Blend("Color Blend", Range(0, 1)) = 0.1
		_Blur("Blur", Range(0, 1)) = 0.1
		_Ambient("Ambient", Range(0, 1)) = 0.15
		_Curvature("Curvature", Range(0, 1)) = 0.4
		_Balance("Balance", Range(0, 1)) = 0.5
		_Shadow("Shadow", Range(0, 1)) = 1
		_Cutoff("Cut off", Range(0, 1)) = 0.5
		_LightLimit("Light Limit", Range(0.0, 1.0)) = 0.97
	}
		CGINCLUDE
#include "Lighting.cginc"
#include "AutoLight.cginc"

		fixed4 _Color;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _MaskTex;
	sampler2D _BRDFTex;
	fixed4 _MainTex_ST;
	half _Ambient;
	half _Fresnel;
	half _Smoothness;
	fixed4 _SkinColor;
	fixed4 _DeepColor;
	half _Blur;
	half _Blend;
	half _Curvature;
	half _Balance;
	half _Shadow;
	half _Cutoff;
	half _LightLimit;

	struct v2f {
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half3 normalDir : TEXCOORD1;
		half3 tangentDir : TEXCOORD2;
		half3 bitangentDir : TEXCOORD3;
		fixed3 viewDir : TEXCOORD4;
		half4 posWorld : TEXCOORD5;
		LIGHTING_COORDS(6, 1)
	};

	fixed3 fragLight(fixed3 diffColor, half3 Nblur, half3 N, half3 V, half3 L, fixed3 ambient, fixed3 light, fixed3 para)
	{
		fixed3 c;
		half nl = dot(Nblur, L);
		/*
		half deltaWorldNormal = length(fwidth(N));
		half deltaWorldPosition = length(fwidth(i.worldpos));
		half Curvature = (deltaWorldNormal / deltaWorldPosition) * _Curvature / 10.0h;
		*/
		half2 brdfUV = half2((nl * (1.0h - _Balance) + _Balance) * para.x, _Curvature * dot(light, fixed3(0.22h, 0.707h, 0.071h)));
		half3 brdf = tex2D(_BRDFTex, brdfUV).rgb * light * para.z;

		half3 H = Unity_SafeNormalize(L + V);
		half roughness = _Smoothness;

		half specBase = saturate(dot(N, H));
		half fresnel = pow(1.0h - dot(V, H), 5.0h);
		fresnel += para.y * (1.0h - fresnel);

		half spec = pow(specBase, roughness * 128.0h) * para.y * fresnel;
		nl = saturate(dot(N, L));
		fixed3 specularColor = spec * fixed3(1.0h, 1.0h, 1.0h) * nl  * para.x * para.z;

		c.rgb = diffColor * (brdf + ambient) + specularColor;

		return c;
	}

	v2f vert(appdata_tan v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.normalDir = UnityObjectToWorldNormal(v.normal);
		o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0h)).xyz);
		o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
		o.viewDir = WorldSpaceViewDir(v.vertex);
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		TRANSFER_VERTEX_TO_FRAGMENT(o);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target{
		fixed3 mask = tex2D(_MaskTex, i.uv);
	clip(mask.r - _Cutoff);

	fixed spec = mask.g * _Fresnel;
	fixed3 V = normalize(i.viewDir);
	half3x3 tangentTransform = half3x3(i.tangentDir, i.bitangentDir, i.normalDir);

	fixed blur = _Blur * mask.b;
	half3 N = UnpackNormal(tex2D(_BumpMap, i.uv));

	half3 blurredNormal = UnpackNormal(tex2Dlod(_BumpMap, half4(i.uv, 0.0h, 5.0h)));
	blurredNormal = lerp(N, blurredNormal, blur);
	blurredNormal = normalize(mul(blurredNormal, tangentTransform));
	N = normalize(mul(N, tangentTransform));

	fixed4 albedo = tex2D(_MainTex, i.uv);
	albedo.rgb *= _Color.xyz;
	half twiceLuminance = dot(albedo, fixed4(0.2126h, 0.7152h, 0.0722h, 0.0h)) * 2.0h;
	fixed4 output = lerp(_SkinColor, _DeepColor, abs(twiceLuminance - 1.0h));
	albedo.rgb = lerp(albedo.rgb, output, _Blend);

	half4 aLight = half4(normalize(_WorldSpaceLightPos0.xyz), 1.0h);
	fixed4 c = fixed4(0.0h, 0.0h, 0.0h, 1.0h);
	fixed3 para = fixed3(lerp(1.0h, LIGHT_ATTENUATION(i), _Shadow), spec, _LightColor0.a);
	c.rgb += fragLight(albedo, blurredNormal, N, V, aLight.xyz, UNITY_LIGHTMODEL_AMBIENT.xyz, _LightColor0.xyz * aLight.w, para);

	aLight.xyz = half3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x) - i.posWorld;
	aLight.w = 1.0h / (1.0h + max(aLight.x * aLight.x + aLight.y * aLight.y + aLight.z * aLight.z, 0.000001h) * unity_4LightAtten0.x);
	aLight.xyz = normalize(aLight.xyz);
	para = fixed3(1.0h, spec, unity_LightColor[0].a);
	c.rgb += fragLight(albedo, blurredNormal, N, V, aLight.xyz, fixed3(0.0h, 0.0h, 0.0h), unity_LightColor[0].rgb * aLight.w, para);

	aLight.xyz = half3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y) - i.posWorld;
	aLight.w = 1.0h / (1.0h + max(aLight.x * aLight.x + aLight.y * aLight.y + aLight.z * aLight.z, 0.000001h) * unity_4LightAtten0.y);
	aLight.xyz = normalize(aLight.xyz);
	para = fixed3(1.0h, spec, unity_LightColor[1].a);
	c.rgb += fragLight(albedo, blurredNormal, N, V, aLight.xyz, fixed3(0.0h, 0.0h, 0.0h), unity_LightColor[1].rgb * aLight.w, para);

	half rim = 1.0h - saturate(dot(N, V));
	c.rgb += _LightColor0.xyz * pow(rim, 4.0h) * _Smoothness * _Ambient;
	c.rgb = min(_LightLimit, c.rgb);
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
		SubShader {
		Tags{ "RenderType" = "Opaque" }
			Pass{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
#pragma multi_compile_fwdbase
#pragma vertex vert  
#pragma fragment frag     
			ENDCG
		}
			Pass{
			Tags{ "LightMode" = "ShadowCaster" }
			Offset 1, 1
			Fog{ Mode Off }
			ZWrite On ZTest LEqual Cull Off
			CGPROGRAM
#pragma vertex vertShadow
#pragma fragment fragShadow
#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
	}
}