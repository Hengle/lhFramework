
Shader "Mobile/Battle/Bump/T4M 4 Textures Bumped" {
    Properties {
        _Color("Main Color", Color) = (1,1,1,1)
        _Layer1 ("Layer1", 2D) = "white" {}
        _Layer2 ("Layer2", 2D) = "white" {}
        _Layer3 ("Layer3", 2D) = "white" {}
        _Layer4 ("Layer4", 2D) = "white" {}
       
        _Normal1 ("Normal1", 2D) = "bump" {}
        _Normal2 ("Normal2", 2D) = "bump" {}
        _Normal3 ("Normal3", 2D) = "bump" {}
        _Normal4 ("Normal4", 2D) = "bump" {}
 
         _ControlRGBA ("Control(RGBA)", 2D) = "white" {}
        
        _spec("spec", 2D) = "white" {}
     	_LightDirecton("LightDirection",Vector) = (0,0,-1,1)//object to light
        _LightColor("LightColor", Color) = (0.5,0.5,0.5,1)
		_specControl("specControl", Range(0.02, 10)) = 0.3841303
		_Gloss("Gloss", Range(0.02, 5)) = 0.4210565
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
         
            #include "UnityCG.cginc"  
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile High Middle Low
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            // #pragma target 3.0

            fixed4 _Color;
            uniform sampler2D _Normal1; uniform half4 _Normal1_ST;
            uniform sampler2D _ControlRGBA; uniform half4 _ControlRGBA_ST;
            uniform sampler2D _Layer1; uniform half4 _Layer1_ST;
            uniform sampler2D _Layer2; uniform half4 _Layer2_ST;
            uniform sampler2D _Layer3; uniform half4 _Layer3_ST;
            uniform sampler2D _Layer4; uniform half4 _Layer4_ST;
            uniform sampler2D _Normal2; uniform half4 _Normal2_ST;
            uniform sampler2D _Normal3; uniform half4 _Normal3_ST;
            uniform sampler2D _Normal4; uniform half4 _Normal4_ST;
 
            #ifdef High 
                uniform sampler2D _spec; uniform half4 _spec_ST;
                uniform half _specControl;
                uniform half _Gloss;
            #endif
 
             #ifdef Middle 
                uniform sampler2D _spec; uniform half4 _spec_ST;
                uniform half _specControl;
                uniform half _Gloss;
            #endif

            #ifdef LIGHTMAP_OFF
                uniform fixed4 _LightColor0;
            #else
                half4 _LightDirecton;
                fixed4 	_LightColor;
            #endif
 
            struct VertexInput {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half2 texcoord0 : TEXCOORD0;
                half2 texcoord1 : TEXCOORD1;
                half2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0;
                half2 uv1 : TEXCOORD1;
                half2 uv2 : TEXCOORD2;
                half4 posWorld : TEXCOORD3;
                half3 normalDir : TEXCOORD4;
                half3 tangentDir : TEXCOORD5;
                half3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                 half2 uv3 : TEXCOORD10;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
              
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, half4( v.tangent.xyz, 0.0h ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                #ifdef LIGHTMAP_OFF
                        fixed3 lightColor = _LightColor0.rgb;
                #else
                        fixed3 lightColor = _LightColor.rgb;
                #endif
 
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                o.uv3 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);

            fixed4 _ControlRGBA_var = tex2D(_ControlRGBA,TRANSFORM_TEX(i.uv0, _ControlRGBA));
            fixed4 ctlAlpha = (((1.0-_ControlRGBA_var.r)-_ControlRGBA_var.g)-_ControlRGBA_var.b);
   #ifdef High 
                half3x3 tangentTransform = half3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                 
                fixed3 _Normal1_var = UnpackNormal(tex2D(_Normal1,TRANSFORM_TEX(i.uv0, _Normal1)));
                fixed3 _Normal2_var = UnpackNormal(tex2D(_Normal2,TRANSFORM_TEX(i.uv0, _Normal2)));
                fixed3 _Normal3_var = UnpackNormal(tex2D(_Normal3,TRANSFORM_TEX(i.uv0, _Normal3)));
                fixed3 _Normal4_var = UnpackNormal(tex2D(_Normal4,TRANSFORM_TEX(i.uv0, _Normal4)));
                
                fixed3 Normals = ((_Normal1_var.rgb*_ControlRGBA_var.r)+(_Normal2_var.rgb*_ControlRGBA_var.g)+(_Normal3_var.rgb*_ControlRGBA_var.b)+(_Normal4_var.rgb*ctlAlpha));
                fixed3 normalLocal = Normals;
                fixed3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals

                 #ifdef LIGHTMAP_OFF
                    fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                #else
                        fixed3 lightDirection = normalize(_LightDirecton.xyz);
                        fixed3 lightColor = _LightColor.rgb;
                #endif
                 fixed3 halfDirection = normalize(viewDirection + lightDirection);
    #endif      

   #ifdef Middle 
                half3x3 tangentTransform = half3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
              
                fixed3 _Normal1_var = UnpackNormal(tex2D(_Normal1,TRANSFORM_TEX(i.uv0, _Normal1)));
                fixed3 _Normal2_var = UnpackNormal(tex2D(_Normal2,TRANSFORM_TEX(i.uv0, _Normal2)));
                fixed3 _Normal3_var = UnpackNormal(tex2D(_Normal3,TRANSFORM_TEX(i.uv0, _Normal3)));
                fixed3 _Normal4_var = UnpackNormal(tex2D(_Normal4,TRANSFORM_TEX(i.uv0, _Normal4)));
               
                fixed3 Normals = ((_Normal1_var.rgb*_ControlRGBA_var.r)+(_Normal2_var.rgb*_ControlRGBA_var.g)+(_Normal3_var.rgb*_ControlRGBA_var.b)+(_Normal4_var.rgb*ctlAlpha));
                fixed3 normalLocal = Normals;
                fixed3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals

                 #ifdef LIGHTMAP_OFF
                    fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                #else
                        fixed3 lightDirection = normalize(_LightDirecton.xyz);
                        fixed3 lightColor = _LightColor.rgb;
                #endif
                 fixed3 halfDirection = normalize(viewDirection + lightDirection);
    #endif   
               
    #ifdef Low
             fixed3   normalDirection = i.normalDir;
              fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
    #endif
                
               
                 half NdotL = max(0, dot(normalDirection, lightDirection));
    ///////// Gloss:
                #ifdef High 
                    half gloss = _Gloss;
                    half specPow = exp2(gloss * 10.0 + 1.0);
                     fixed4 _spec_var = tex2D(_spec, TRANSFORM_TEX(i.uv0, _spec));
                #endif

                 #ifdef Middle 
                    half gloss = _Gloss;
                    half specPow = exp2(gloss * 10.0 + 1.0);
                     fixed4 _spec_var = tex2D(_spec, TRANSFORM_TEX(i.uv0, _spec));
                #endif
    ////// Specular:
                
                #ifdef High 
                    half3 specularColor = (_specControl*_spec_var.rgb);
                    half3 directSpecular = pow(max(0, dot(halfDirection, normalDirection)), specPow)*specularColor;
                    half3 specular = directSpecular;
                #endif

                #ifdef Middle 
                    half3 specularColor = (_specControl*_spec_var.rgb);
                    half3 directSpecular = pow(max(0, dot(halfDirection, normalDirection)), specPow)*specularColor;
                    half3 specular = directSpecular;
                #endif
 
    /////// Diffuse: 
                fixed4 _Layer1_var = tex2D(_Layer1,TRANSFORM_TEX(i.uv0, _Layer1));
                fixed4 _Layer2_var = tex2D(_Layer2,TRANSFORM_TEX(i.uv0, _Layer2));
                fixed4 _Layer3_var = tex2D(_Layer3,TRANSFORM_TEX(i.uv0, _Layer3));
                fixed4 _Layer4_var = tex2D(_Layer4,TRANSFORM_TEX(i.uv0, _Layer4));
                fixed3 diffuseColor = ((_ControlRGBA_var.r*_Layer1_var.rgb)+(_ControlRGBA_var.g*_Layer2_var.rgb)+(_ControlRGBA_var.b*_Layer3_var.rgb)+(ctlAlpha*_Layer4_var.rgb));
                diffuseColor *= _Color;
    /// Final Color: 
        #ifdef LIGHTMAP_OFF
                half attenuation = LIGHT_ATTENUATION(i);
                half3 attenColor = attenuation * _LightColor0.xyz;
                half3 directDiffuse = max( 0.0, NdotL) * attenColor;
                half3 diffuse = directDiffuse * diffuseColor; 
                NdotL = max(0.0, dot(normalDirection, lightDirection));
            
              
                #ifdef High
                    half3 pointlight = half3(0, 0, 0);
                    pointlight = Shade4PointLights(
                    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, i.posWorld, i.normalDir);
                    half3 finalColor = diffuse *(UNITY_LIGHTMODEL_AMBIENT.rgb + directDiffuse + pointlight) + specular;
                #endif

                #ifdef Middle
                    half3 pointlight = half3(0, 0, 0);
                    pointlight = Shade4PointLights(
                    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, i.posWorld, i.normalDir);
                    half3 finalColor = diffuse *(UNITY_LIGHTMODEL_AMBIENT.rgb + directDiffuse + pointlight) + specular;
                #endif

                #ifdef Low
                    half3 finalColor = diffuse *(UNITY_LIGHTMODEL_AMBIENT.rgb + directDiffuse);
                #endif
               
                fixed4 finalRGBA = fixed4(finalColor, 1);
        #else
                
                #ifdef High 
                  half3  finalColor = diffuseColor + specular *_LightColor;
                #endif
                 #ifdef Middle 
                  half3  finalColor = diffuseColor + specular *_LightColor;
                #endif
                #ifdef Low 
                  half3  finalColor = diffuseColor ;
                #endif

                fixed4 finalRGBA = fixed4(finalColor, 1);
                finalRGBA.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv3));
        #endif
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        } 
    }
    FallBack "Diffuse"
}
