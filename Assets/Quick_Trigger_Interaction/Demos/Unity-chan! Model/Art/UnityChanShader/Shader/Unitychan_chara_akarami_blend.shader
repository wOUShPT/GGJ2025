Shader "UnityChan/Blush - Transparent"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)

		_MainTex ("Diffuse", 2D) = "white" {}
		_FalloffSampler ("Falloff Control", 2D) = "white" {}
		_RimLightSampler ("RimLight Control", 2D) = "white" {}
	}

	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha, One One 
		ZWrite Off
		Tags
		{
			"Queue"="Geometry+3"
			"IgnoreProjector"="True"
			"RenderType"="Overlay"
		}
		
		Pass
		{
			Cull Back
			ZTest LEqual
			
		HLSLPROGRAM
		
		#pragma target 2.0
		#pragma vertex vert
		#pragma fragment frag
		#include "CharaSkin.hlsl"
		
		ENDHLSL
		}
	}

	FallBack "Transparent/Cutout/Diffuse"
}
