Shader "Mobile/Battle/BlackScreen"
{
	Properties
	{
		_Color("Main Color",Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Base (RGB)",2D) = "while" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			fixed4 _Color;
			sampler2D _MainTex;

			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv)*_Color;
				return col;
			}
			ENDCG
		}
	}
}
