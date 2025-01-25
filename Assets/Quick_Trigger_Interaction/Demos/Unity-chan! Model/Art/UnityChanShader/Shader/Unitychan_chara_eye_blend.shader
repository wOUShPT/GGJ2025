Shader "UnityChan/Eye - Transparent"
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
		
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent+1" // Transparent+1"
			"IgnoreProjector"="True"
			"RenderType"="Overlay"
		}

		Pass
		{
			Name "ForwardLit"
			
			Cull Back
			ZTest LEqual
			
			Tags 
            { 
                "Lightmode" = "UniversalForward"
            }
			
			HLSLPROGRAM
			
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "CharaSkin.hlsl"
			
			ENDHLSL
		}

		UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
		UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
		UsePass "Universal Render Pipeline/Lit/META"
		UsePass "Universal Render Pipeline/Lit/GBUFFER"
		UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
	}
}
