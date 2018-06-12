Shader "GOT/Transparent/Diffuse" {
Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
		_UseSecondAlpha("Is Use Second Alpha", int) = 0
		}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _MainTex_Alpha;
int _UseSecondAlpha;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

			fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v) 
			 {
			 	fixed4 col = tex2D(sa,v);
			 	fixed alp = tex2D(sb,v).r;
			 	col.a = min(col.a,alp) ;
				return col;
			 }

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D_ETC1(_MainTex,_MainTex_Alpha, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a ;
}
ENDCG
}

Fallback "GDE/Transparent/VertexLit"
}

