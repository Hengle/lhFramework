﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Mobile/Battle/RadialBlurEffects" 
{
	//------------------------------------【属性值】------------------------------------
	Properties
	{
		_MainTex("MainTex (RGB)", 2D) = "white" {}
		_IterationNumber("Number", Int)=16 
	}

	//------------------------------------【唯一的子着色器】------------------------------------
	SubShader
	{	
		//--------------------------------唯一的通道-------------------------------
		Pass
		{
			//设置深度测试模式:渲染所有像素.等同于关闭透明度测试（AlphaTest Off）
			ZTest Always

			//===========开启CG着色器语言编写模块===========
			CGPROGRAM
 

			//编译指令:告知编译器顶点和片段着色函数的名称
			#pragma vertex vert
			#pragma fragment frag

			//包含辅助CG头文件
			#include "UnityCG.cginc"

			//外部变量的声明
			uniform sampler2D _MainTex;
			uniform half _Value;
			uniform half _Value2;
			uniform half _Value3;
			uniform int _IterationNumber;

			//顶点输入结构
			struct vertexInput
			{
				half4 vertex : POSITION;//顶点位置
				half4 color : COLOR;//颜色值
				half2 texcoord : TEXCOORD0;//一级纹理坐标
			};

			//顶点输出结构
			struct vertexOutput
			{
				half2 texcoord : TEXCOORD0;//一级纹理坐标
				half4 vertex : SV_POSITION;//像素位置
				fixed4 color : COLOR;//颜色值
			};


			//--------------------------------【顶点着色函数】-----------------------------
			// 输入：顶点输入结构体
			// 输出：顶点输出结构体
			//---------------------------------------------------------------------------------
			vertexOutput vert(vertexInput Input)
			{
				//【1】声明一个输出结构对象
				vertexOutput Output;

				//【2】填充此输出结构
				//输出的顶点位置为模型视图投影矩阵乘以顶点位置，也就是将三维空间中的坐标投影到了二维窗口
				Output.vertex = UnityObjectToClipPos(Input.vertex);
				//输出的纹理坐标也就是输入的纹理坐标
				Output.texcoord = Input.texcoord;
				//输出的颜色值也就是输入的颜色值
				Output.color = Input.color;

				//【3】返回此输出结构对象
				return Output;
			}

			//--------------------------------【片段着色函数】-----------------------------
			// 输入：顶点输出结构体
			// 输出：half4型的颜色值
			//---------------------------------------------------------------------------------
			fixed4 frag(vertexOutput i) : COLOR
			{
				//【1】设置中心坐标
				half2 center = half2(_Value2, _Value3);
				//【2】获取纹理坐标的x，y坐标值
				half2 uv = i.texcoord.xy;
				//【3】纹理坐标按照中心位置进行一个偏移
				uv -= center;
				//【4】初始化一个颜色值
				half4 color = half4(0.0, 0.0, 0.0, 0.0);
				//【5】将Value乘以一个系数
				_Value *= 0.085;
				//【6】设置坐标缩放比例的值
				half scale = 1;

				//【7】进行纹理颜色的迭代
				for (int j = 1; j < _IterationNumber; ++j)
				{
					//将主纹理在不同坐标采样下的颜色值进行迭代累加
					color += tex2D(_MainTex, uv * scale + center);
					//坐标缩放比例依据循环参数的改变而变化
					scale = 1 + (half(j * _Value));
				}

				//【8】将最终的颜色值除以迭代次数，取平均值
				color /= (half)_IterationNumber;

				//【9】返回最终的颜色值
				return  color;
			}

			//===========结束CG着色器语言编写模块===========
			ENDCG
		}
	}
}
