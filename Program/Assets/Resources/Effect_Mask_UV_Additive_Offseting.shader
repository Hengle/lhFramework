 
Shader "Effect/Mask_UV_Additive_Offseting" {
    Properties {
        //_color ("color", Color) = (0.5,0.5,0.5,0.5) 
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
	[NoScaleOffset]_AlphaTex("AlphaTex",2D) = "white" {}
	[NoScaleOffset]_MaskTex ("MaskTex", 2D) = "white" {}
        //_SpeedU ("SpeedU", Float ) = 0.1
        //_SpeedV ("SpeedV", Float ) = 0.1 
        //_Power ("Power",Float) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
       
            Blend SrcAlpha One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
            #include "UnityCG.cginc"
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal  
            uniform sampler2D _MainTex; uniform fixed4 _MainTex_ST;
            uniform sampler2D _AlphaTex; uniform fixed4 _AlphaTex_ST;
            uniform sampler2D _MaskTex; uniform fixed4 _MaskTex_ST;
           
            
            struct VertexInput {
                fixed4 vertex : POSITION;
                fixed2 texcoord0 : TEXCOORD0;
                fixed4 vertexColor : COLOR;

                
				half4 texcoord1 : TEXCOORD1;
				half4 texcoord2 : TEXCOORD2;
				half4 texcoord3 : TEXCOORD3;
              
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                fixed2 uv0 : TEXCOORD0;
                fixed4 vertexColor : COLOR;
                float2 movingUV:TEXCOORD1;
                  half power:TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                  VertexOutput o  ;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                half4 time = _Time;
                o.movingUV = float2(((v.texcoord2.x*time.g)+o.uv0.r),(o.uv0.g+(time.g*v.texcoord2.y)));
                o.movingUV = o.movingUV * v.texcoord3.xy + v.texcoord3.zw;
                o.uv0 =o.uv0*v.texcoord1.xy + v.texcoord1.zw;
                o.power = v.texcoord2.z;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            fixed4 frag(VertexOutput i, half facing : VFACE) : COLOR {
               half isFrontFace = ( facing >= 0 ? 1 : 0 );
                half faceSign = ( facing >= 0 ? 1 : -1 );
                fixed4 _MainTexVar = tex2D(_MainTex,i.movingUV);
                 fixed4 _AlphaTex_var = tex2D(_AlphaTex,i.movingUV);
                fixed4 _MaskTex_var = tex2D(_MaskTex,i.uv0);
                fixed3 emissive = (_MainTexVar.rgb*i.vertexColor.rgb *_MaskTex_var.rgb);
                fixed3 finalColor = i.power*emissive; 
                half alpha =lerp(_MainTexVar.a, _AlphaTex_var.r,step(1,_MainTexVar.a));
                return fixed4(finalColor,(alpha*i.vertexColor.a*_MaskTex_var.r));
            }
            ENDCG
        }
        
    }
}
