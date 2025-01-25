// Character shader
// Includes falloff shadow and highlight, specular, reflection, and normal mapping

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

// Material parameters
float4 _Color;
float4 _ShadowColor;
float4 _LightColor0;
float _SpecularPower;
float4 _MainTex_ST;

// Textures
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_FalloffSampler);
SAMPLER(sampler_FalloffSampler);
TEXTURE2D(_RimLightSampler);
SAMPLER(sampler_RimLightSampler);
TEXTURE2D(_SpecularReflectionSampler);
SAMPLER(sampler_SpecularReflectionSampler);
TEXTURE2D(_EnvMapSampler);
SAMPLER(sampler_EnvMapSampler);
TEXTURE2D(_NormalMapSampler);
SAMPLER(sampler_NormalMapSampler);

// Constants
#define FALLOFF_POWER 0.3

struct Attributes
{
	float3 positionOS			: POSITION; // Position in object space
	float3 normalOS				: NORMAL; // Normal in object space
	float4 tangentOS			: TANGENT;
	float2 uv					: TEXCOORD0;
	float2 lightMapUV			: TEXCOORD1;
	float4 color				: COLOR;
};

// Structure from vertex shader to fragment shader
struct Varyings
{
	float4 positionCS			: SV_POSITION;
	float2 uv					: TEXCOORD0;
	float3 positionWS			: TEXCOORD2;
	float3 viewDirectionWS		: TEXCOORD3;
	float3 normalWS				: TEXCOORD5;
	float3 tangentWS			: TEXCOORD6;
	float3 binormalWS			: TEXCOORD7;
	float4 color				: COLOR;
};

// Float types

// Overlay blend
inline half3 GetOverlayColor(half3 inUpper, half3 inLower )
{
	half3 oneMinusLower = half3( 1.0, 1.0, 1.0 ) - inLower;
	half3 valUnit = 2.0 * oneMinusLower;
	half3 minValue = 2.0 * inLower - half3( 1.0, 1.0, 1.0 );
	half3 greaterResult = inUpper * valUnit + minValue;

	half3 lowerResult = 2.0 * inLower * inUpper;

	half3 lerpVals = round(inLower);
	return lerp(lowerResult, greaterResult, lerpVals);
}

// Compute normal from normal map
inline half3 GetNormalFromMap(Varyings input )
{
	half3 normalVec = normalize( SAMPLE_TEXTURE2D( _NormalMapSampler, sampler_NormalMapSampler, input.uv ).xyz * 2.0 - 1.0 );
	half3x3 localToWorldTranspose = half3x3(
		input.tangentWS,
		input.binormalWS,
		input.normalWS
	);
	
	normalVec = normalize( mul( normalVec, localToWorldTranspose ) );
	return normalVec;
}

Varyings vert(Attributes input)
{
	Varyings output = (Varyings)0;
	
	VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
	VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS.xyz);
	
	output.positionCS = positionInputs.positionCS;
	output.positionWS = positionInputs.positionWS;
	output.normalWS = normalInputs.normalWS;
	output.tangentWS = normalInputs.tangentWS;
	output.viewDirectionWS = GetWorldSpaceViewDir(positionInputs.positionWS);
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	output.color = input.color;
	
	return output;
}

// Fragment shader
half4 frag(Varyings input) : SV_TARGET
{
	half4 diffSamplerColor = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, input.uv.xy);
	half3 normalVec = GetNormalFromMap(input);

	// Falloff. Convert the angle between the normal and the camera direction into a lookup for the gradient
	half normalDotEye = dot( normalVec, input.viewDirectionWS.xyz );
	half falloffU = clamp( 1.0 - abs( normalDotEye ), 0.02, 0.98 );
	half4 falloffSamplerColor = FALLOFF_POWER * SAMPLE_TEXTURE2D(_FalloffSampler, sampler_FalloffSampler,float2( falloffU, 0.25f));
	half3 shadowColor = diffSamplerColor.rgb * diffSamplerColor.rgb;
	half3 combinedColor = lerp( diffSamplerColor.rgb, shadowColor, falloffSamplerColor.r );
	combinedColor *= ( 1.0 + falloffSamplerColor.rgb * falloffSamplerColor.a );
	
	// Use the eye vector as the light vector
	half4 reflectionMaskColor = SAMPLE_TEXTURE2D( _SpecularReflectionSampler, sampler_SpecularReflectionSampler, input.uv.xy );
	half specularDot = dot( normalVec, input.viewDirectionWS.xyz );
	half4 lighting = lit( normalDotEye, specularDot, _SpecularPower );
	half3 specularColor = saturate( lighting.z ) * reflectionMaskColor.rgb * diffSamplerColor.rgb;
	combinedColor += specularColor;
	
	// Reflection
	half3 reflectVector = reflect( -input.viewDirectionWS.xyz, normalVec ).xzy;
	half2 sphereMapCoords = 0.5 * ( half2( 1.0, 1.0 ) + reflectVector.xy );
	half3 reflectColor = SAMPLE_TEXTURE2D( _EnvMapSampler, sampler_EnvMapSampler, sphereMapCoords ).rgb;
	reflectColor = GetOverlayColor( reflectColor, combinedColor );
	
	combinedColor = lerp( combinedColor, reflectColor, reflectionMaskColor.a );
	combinedColor *= _Color.rgb * _LightColor0.rgb;
	float opacity = diffSamplerColor.a * _Color.a * _LightColor0.a;
	
	shadowColor = _ShadowColor.rgb * combinedColor;
	half attenuation = saturate( 2.0 * _MainLightPosition - 1.0 );
	combinedColor = lerp( shadowColor, combinedColor, attenuation );

	// Rimlight
	half rimlightDot = saturate( 0.5 * ( dot( normalVec, _MainLightPosition ) + 1.0 ) );
	falloffU = saturate( rimlightDot * falloffU );
	falloffU = SAMPLE_TEXTURE2D( _RimLightSampler, sampler_RimLightSampler, float2( falloffU, 0.25f ) ).r;
	half3 lightColor = diffSamplerColor.rgb; // * 2.0;
	combinedColor += falloffU * lightColor;

	return half4(combinedColor, opacity);
}
