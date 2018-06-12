// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Battle/ShadowMap" { 
	Properties {
		_ShadowTex ("_ShadowTex", 2D) = "gray" {}
		//_Bias("_Bias", Range(0, 0.01)) = 0
		_BlurSize("_BlurSize", float) = 1600
		_Strength("_Strength", Range(0, 0.2)) = 0.0379
		[Toggle] _Blur ("blur?",float)=1

	}
	Subshader {
		Tags {"Queue"="Transparent"} 
		Pass {
			ZWrite Off
			Fog { Color (1, 1, 1) }
			AlphaTest Greater 0
			//ColorMask RGB
			Blend zero Oneminussrcalpha 
			//Blend DstColor Zero
			Offset -1, -1
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f {
				half4 uvShadow : TEXCOORD0;
				half4 uvFalloff : TEXCOORD1;
				half4 pos : SV_POSITION;
				half2 uv: TEXCOORD2;
				half2 uvs[5] :TEXCOORD3;

			};
			fixed _BlurSize =1600;
			fixed _Blur ;
			uniform half4x4 ShadowMatrix;
			half4x4 unity_Projector;
			half4x4 unity_ProjectorClip;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos (v.vertex);
				fixed4x4 matWVP = mul (ShadowMatrix, unity_ObjectToWorld);
				fixed4 uvShadow = mul(matWVP, v.vertex);
				o.uv =  uvShadow.xy / uvShadow.w * 0.5 + 0.5;
				#if UNITY_UV_STARTS_AT_TOP
                	o.uv.y = 1 - o.uv.y;
                #endif

                o.uvs[0] =o.uv;
				      
				return o;
			}
			
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			half _Bias;
			half _Strength;
			fixed4 frag (v2f i) : SV_Target
			{

					half2 uvMask = min(i.uv, 1.0 - i.uv);
					half mask = min(uvMask.x, uvMask.y);
					
 				fixed4 res = fixed4(0, 0, 0, 0);
 				//half shadowz = i.uvShadow.z / i.uvShadow.w;
 				half pad = _BlurSize;
 				//fixed4 texS = tex2D(_ShadowTex, i.uv);

 			 //texS = tex2D(_ShadowTex, i.uvs[0]);
			   half4   texS = tex2D(_ShadowTex,i.uvs[0] + half2(-0.94201624/pad, -0.39906216/pad)*_Blur);
			   res.a += _Strength*ceil(texS.a);
 
				   
			texS = tex2D(_ShadowTex,i.uvs[0] + half2(0.94558609/pad, -0.76890725/pad)*_Blur);
			res.a += _Strength*ceil(texS.a);
			texS = tex2D(_ShadowTex,i.uvs[0] + half2(-0.094184101/pad, -0.92938870/pad)*_Blur);
			res.a += _Strength*ceil(texS.a);
			texS = tex2D(_ShadowTex, i.uvs[0] + half2(0.34495938/pad, 0.29387760/pad)*_Blur);
			res.a += _Strength*ceil(texS.a);
 
 

 					//res.a += _Strength*ceil(texS.a);
 				//half3 kDecodeDot = half3(1.0, 1/255.0, 1/65025.0);
				//half z = dot(texS.gba, kDecodeDot);
				//half flag = 1;
 				//if(texS.r == 1)
 				//{
 				//	flag = -1;
 				//}
 				//if(shadowz - _Bias> z * flag)
 				//{
 					//res.r += _Strength;
 				//}

 				//texS = tex2D(_ShadowTex, i.uv + half2(-0.94201624/pad, -0.39906216/pad));



 				//fixed4 col = fixed4(0, 0, 0, res.a );
				fixed4 col =mask < 0.0 ?fixed4(0, 0, 0, 0) : fixed4(0, 0, 0, res.a );
				return col;
			}
			ENDCG
		}
	}
}
