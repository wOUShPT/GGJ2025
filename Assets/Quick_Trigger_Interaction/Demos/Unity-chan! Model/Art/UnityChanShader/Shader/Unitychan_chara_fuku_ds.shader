Shader "UnityChan/Clothing - Double-sided"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
		_SpecularPower ("Specular Power", Float) = 20
		_EdgeThickness ("Outline Thickness", Float) = 1
				
		_MainTex ("Diffuse", 2D) = "white" {}
		_FalloffSampler ("Falloff Control", 2D) = "white" {}
		_RimLightSampler ("RimLight Control", 2D) = "white" {}
		_SpecularReflectionSampler ("Specular / Reflection Mask", 2D) = "white" {}
		_EnvMapSampler ("Environment Map", 2D) = "" {} 
		_NormalMapSampler ("Normal Map", 2D) = "" {} 
	}

	SubShader
	{
		Tags 
		{ 
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
			"IgnoreProjector"="True"
		}	

		Pass
		{
			Name "ForwardLit"
			
			Cull Off
			ZTest LEqual
			
			Tags 
            { 
                "Lightmode" = "UniversalForward"
            }
			
			HLSLPROGRAM
			
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "CharaMain.hlsl"
			
			ENDHLSL
		}

		UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
		UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
		UsePass "Universal Render Pipeline/Lit/META"
		UsePass "Universal Render Pipeline/Lit/GBUFFER"
		UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
	}
}
