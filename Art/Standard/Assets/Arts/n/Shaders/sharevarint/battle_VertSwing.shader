 
Shader "Mobile/Battle/VertSwingDiffuse" {
    Properties {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Amplitude ("Amplitude", Float ) = 1
       
        _MainTex ("BaseTex", 2D) = "white" {}
        _Speed ("Speed", Float ) = 1
        _dir ("dir", Vector) = (1,0,1,0)
        _Cutout ("Cutout", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        
    }
    SubShader {
        Tags {
  	"RenderType"="Opaque" 
        }
        LOD 200
        Pass {
            Name "FORWARD"
             Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
			#pragma multi_compile High Middle Low
            // #pragma target 3.0
            uniform half _Amplitude;
            
            uniform sampler2D _MainTex; uniform half4 _MainTex_ST;
            uniform half _Speed;
            uniform half4 _dir;
            uniform fixed _Cutout;
            uniform fixed4 _Color;
            struct VertexInput {
                fixed4 vertex : POSITION;
                fixed3 normal : NORMAL;
                fixed4 tangent : TANGENT;
                fixed2 texcoord0 : TEXCOORD0;
                fixed2 texcoord1 : TEXCOORD1;
                fixed2 texcoord2 : TEXCOORD2;
                fixed4 vertexColor : COLOR;
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                fixed2 uv0 : TEXCOORD0;
                fixed2 uv1 : TEXCOORD1;
                fixed2 uv2 : TEXCOORD2;
                half4 posWorld : TEXCOORD3;
                fixed3 normalDir : TEXCOORD4;
                fixed3 tangentDir : TEXCOORD5;
                fixed3 bitangentDir : TEXCOORD6;
                fixed4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    half4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, half4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                half3 WorldPos = (mul(unity_ObjectToWorld, v.vertex).rgb/1.0);
                
             

#ifdef High
                float time = _Time;
                float3 worldPosMsk = frac(WorldPos);
                float scale =  unity_ObjectToWorld[0].x;
                float offset = cos(_Speed*(worldPosMsk.r+(worldPosMsk.b*3.0)+time)) * _Amplitude * _dir.rgb* o.vertexColor.g / scale;
                v.vertex.xyz +=  offset;
#endif

#ifdef Middle
                float time = _Time;
                float3 worldPosMsk = frac(WorldPos);
                float scale =  unity_ObjectToWorld[0].x;
                float offset = cos(_Speed*(worldPosMsk.r+(worldPosMsk.b*3.0h)+time)) * _Amplitude * _dir.rgb* o.vertexColor.g / scale;
                v.vertex.xyz +=  offset;
#endif

#ifdef Low

#endif

                // v.vertex.xyz += (cos((_Speed*(WorldPos.r+(WorldPos.b*3.0)+time)))*_Amplitude*_dir.rgb*0.01) * o.vertexColor.g*5.0 ;
            
                
                // v.vertex.xyz  += v.vertex.xyz * _dir.rgb;
                // v.vertex.xyz += (cos((_Speed*(worldPosMsk.r+(worldPosMsk.b*3.0)+time+(o.vertexColor.g*3.141592654*5.0))))*_Amplitude*_dir.rgb*0.001);

                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                fixed3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                half3x3 tangentTransform = half3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                half3 normalDirection = i.normalDir;
                half3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                fixed4 _BaseTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_BaseTex_var.a-_Cutout) - 0.5);
                half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                half3 lightColor = _LightColor0.rgb;
////// Lighting:
                half attenuation = LIGHT_ATTENUATION(i);
                half3 attenColor = attenuation * _LightColor0.xyz;
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.h, 0.h, 0.h);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.h, 0.h, 0.h);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0h - 0.h;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
/////// Diffuse:
                fixed NdotL = max(0.0h,dot( normalDirection, lightDirection ));
                fixed3 directDiffuse = max( 0.0h, NdotL) * attenColor;
                fixed3 indirectDiffuse = fixed3(0.0h,0.0h,0.0h);
                indirectDiffuse += gi.indirect.diffuse;
                fixed3 diffuseColor = (_Color.rgb*_BaseTex_var.rgb);
                fixed3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                fixed3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1.0h);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            // #pragma target 3.0
            uniform half _Amplitude;
            uniform sampler2D _MainTex; uniform half4 _MainTex_ST;
            uniform half _Speed;
            uniform half4 _dir;
            uniform fixed _Cutout;
            struct VertexInput {
                fixed4 vertex : POSITION;
                fixed2 texcoord0 : TEXCOORD0;
                fixed2 texcoord1 : TEXCOORD1;
                fixed2 texcoord2 : TEXCOORD2;
                fixed4 vertexColor : COLOR;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                fixed2 uv0 : TEXCOORD1;
                fixed2 uv1 : TEXCOORD2;
                fixed2 uv2 : TEXCOORD3;
                half4 posWorld : TEXCOORD4;
                fixed4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                half3 WorldPos = (mul(unity_ObjectToWorld, v.vertex).rgb/1.0h);
                half3 worldPosMsk = WorldPos.rgb;
                half4 node_995 = _Time;
               // v.vertex.xyz += (cos((_Speed*(worldPosMsk.r+(worldPosMsk.b*3.0)+node_995.g+(o.vertexColor.g*3.141592654*5.0))))*_Amplitude*_dir.rgb*0.01);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                fixed4 _BaseTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_BaseTex_var.a-_Cutout) - 0.6h);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
       
    }
    FallBack "Legacy Shaders/Transparent/VertexLit"
}
