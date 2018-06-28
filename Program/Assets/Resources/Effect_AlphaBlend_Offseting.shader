// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Alpha Blended Particle shader. Differences from regular Alpha Blended Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Effect/AlphaBlend_Offseting" { 
Properties {
	//_Color ("Color", Color) = (1,1,1,1)
	[NoScaleOffset]_MainTex ("Particle Texture", 2D) = "white" {}
[NoScaleOffset] _AlphaTex("AlphaTex",2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Off Lighting Off ZWrite Off Fog { Mode off }
 
	SubShader {
		  Pass {
      
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x   
            uniform sampler2D _MainTex; uniform fixed4 _MainTex_ST;
            uniform sampler2D _AlphaTex; uniform fixed4 _AlphaTex_ST; 

             struct VertexInput {
                float4 vertex : POSITION;
                half2 texcoord0 : TEXCOORD0; 
                half4 vertexColor : COLOR;
                float4 texcoord1 : TEXCOORD1;
            };

            struct VertexOutput {
                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0; 
                float4 vertexColor : COLOR;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                // o.uv0 = TRANSFORM_TEX(v.texcoord0, _MainTex); 
                o.uv0 =  v.texcoord0 * v.texcoord1.xy + v.texcoord1.zw;
                o.vertexColor = v.vertexColor;
              
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }

            fixed4 frag(VertexOutput i) : SV_Target {
                fixed4 _BaseTex_var = tex2D(_MainTex,i.uv0)   ;
                _BaseTex_var.rgb = _BaseTex_var.rgb   *  i.vertexColor.rgb * 2;
                fixed4 _AlphaTexVar = tex2D(_AlphaTex,i.uv0);
                half alpha =lerp( _BaseTex_var.a,_AlphaTexVar.r, step(1,_BaseTex_var.a));

                alpha = alpha *  i.vertexColor.a * 2;
                return fixed4(_BaseTex_var.rgb,alpha);
            }
            ENDCG
        }
	}
    }
}
