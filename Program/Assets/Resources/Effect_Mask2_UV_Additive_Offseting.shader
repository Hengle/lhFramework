Shader "Effect/Mask2_UV_Additive_Offseting" {
    Properties {
	[NoScaleOffset]_MainTex ("MainTex", 2D) = "white" {}
	[NoScaleOffset]_mask01 ("mask01", 2D) = "white" {}
	[NoScaleOffset]_mask02 ("mask02", 2D) = "white" {}
        
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
         
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
            #include "UnityCG.cginc"
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
   
            uniform sampler2D _MainTex;
            uniform sampler2D _mask01; 
            uniform sampler2D _mask02; 
            struct VertexInput {
                fixed4 vertex : POSITION;
                fixed2 texcoord0 : TEXCOORD0;
                fixed4 vertexColor : COLOR;

                //half4 tangent : TANGENT; //mainTex scale offset
                half4 texcoord1 : TEXCOORD1;//x0 y0 x1 y1
                half4 texcoord2 : TEXCOORD2;  //mask1 scale offset
                half4 texcoord3 : TEXCOORD3;  //mask2 scale offset
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                fixed2 uv0 : TEXCOORD0;
                half2 mainuv : TEXCOORD1;
                half2 mask1uv: TEXCOORD2;
                half2 mask2uv: TEXCOORD3;
                half4 color : TEXCOORD4;
                fixed4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                half4 time = _Time;
                o.mainuv = o.uv0+v.texcoord1.xy*time.g;

				half4 tangentFor = floor(v.texcoord3 / 100);
				half4 tangentBac = floor(v.texcoord2 / 100);
				half4 texcoord3 = v.texcoord3 - tangentFor*100;
				half4 texcoord2 = v.texcoord2 - tangentBac*100;
				half4 tangent = tangentFor + tangentBac/100;
				//half4 mask2uv = (v.texcoord3, 99);
                o.mainuv = o.mainuv*tangent.xy+ tangent.zw;

                o.mask1uv = time.g*v.texcoord1.zw+o.uv0;
                o.mask1uv =  o.mask1uv * texcoord2.xy + texcoord2.zw;
                o.mask2uv =  o.uv0 * texcoord3.xy + texcoord3.zw;
                o.color = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            fixed4 frag(VertexOutput i) : SV_Target { 
                fixed4 _MainTexVar = tex2D(_MainTex,i.mainuv); 
                fixed4 _mask01_var = tex2D(_mask01,i.mask1uv);
                fixed4 _mask02_var = tex2D(_mask02, i.mask2uv);
                fixed3 emissive = _MainTexVar.rgb*_mask01_var.rgb*_mask02_var.rgb* i.color.rgb;
                fixed3 finalColor = emissive;
                return fixed4(finalColor,1.0h);
            }
            ENDCG
        }
    }
}