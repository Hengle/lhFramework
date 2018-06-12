
Shader "Mobile/Battle/battle_swing" {
    Properties {
        _color ("color", Color) = (0.5,0.5,0.5,1)
        _texture ("texture", 2D) = "white" {}
        _wave ("wave", Float ) = 0.2
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
             #pragma multi_compile High Middle Low
            // #pragma target 3.0
            uniform sampler2D _texture; uniform fixed4 _texture_ST;
            uniform half _wave;
            uniform fixed4 _color;
            struct VertexInput {
                fixed4 vertex : POSITION;
                fixed2 texcoord0 : TEXCOORD0;
                fixed4 vertexColor : COLOR;
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                fixed2 uv0 : TEXCOORD0;
                half4 posWorld : TEXCOORD1;
                fixed4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
////// Lighting:

#ifdef High
                float4 node_143 = _Time;
                float2 node_3169 = float2((i.uv0.r+(cos((((i.posWorld.r+i.posWorld.b)*3.141592654*0.05)+node_143.g))*_wave*i.vertexColor.r)),i.uv0.g);
                fixed4 node_5913 = tex2D(_texture,TRANSFORM_TEX(node_3169, _texture));
#endif

#ifdef Middle
                float4 node_143 = _Time;
                float2 node_3169 = float2((i.uv0.r+(cos((((i.posWorld.r+i.posWorld.b)*3.141592654*0.05)+node_143.g))*_wave*i.vertexColor.r)),i.uv0.g);
                fixed4 node_5913 = tex2D(_texture,TRANSFORM_TEX(node_3169, _texture));
#endif
     
#ifdef Low    
                fixed4 node_5913 = tex2D(_texture,TRANSFORM_TEX(i.uv0, _texture));
#endif   
                fixed3 finalColor = (_color.rgb*node_5913.rgb);
                fixed4 finalRGBA = fixed4(finalColor,node_5913.a);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Particles/Alpha Blended"
}
