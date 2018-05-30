 
Shader "Mobile/Battle/SimpleWater" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _FlowTex ("FlowTex", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        _FlowIntensity ("FlowIntensity", Range(-1, 1)) = 0
        _USpeed ("USpeed", Range(0, 1)) = 0
        _VSpeed ("VSpeed", Range(0, 1)) = 0
        _Alpha ("Alpha", Range(0, 1)) = 0
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
   
            #include "UnityCG.cginc"
  
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal n3ds wiiu 
      
            uniform sampler2D _MainTex; uniform half4 _MainTex_ST;
            uniform sampler2D _FlowTex; uniform half4 _FlowTex_ST;
            uniform half _FlowIntensity;
            uniform half _USpeed;
            uniform half _VSpeed;
            uniform half _Alpha;
            uniform fixed4 _Color;
            struct VertexInput {
                half4 vertex : POSITION;
                fixed2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
                float4 time = _Time;
                half2 moveUV = (i.uv0+float2((_USpeed*time.g),(time.g*_VSpeed)));
                fixed4 _FlowTex_var = tex2D(_FlowTex,TRANSFORM_TEX(moveUV, _FlowTex));
                fixed2 node_3211 = (i.uv0+(float2(_FlowTex_var.r,_FlowTex_var.g)*_FlowIntensity*0.1));
                fixed4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3211, _MainTex));
                float3 finalColor = _MainTex_var.rgb * _Color.rgb;
                fixed4 finalRGBA = fixed4(finalColor,(_MainTex_var.a*_Alpha*_Color.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
