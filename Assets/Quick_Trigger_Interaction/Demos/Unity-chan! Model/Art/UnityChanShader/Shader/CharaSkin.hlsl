// Character skin shader
// Includes falloff shadow

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

// Material parameters
float4 _Color;
float4 _ShadowColor;
float4 _LightColor0;
float4 _MainTex_ST;

// Textures
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_FalloffSampler);
SAMPLER(sampler_FalloffSampler);
TEXTURE2D(_RimLightSampler);
SAMPLER(sampler_RimLightSampler);

// Constants
#define FALLOFF_POWER 1.0

// Structure from vertex shader to fragment shader
struct Attributes
{
	float3 positionOS	: POSITION; // Position in object space
	float3 normalOS		: NORMAL; // Normal in object space
	float4 tangentOS    : TANGENT;
	float2 uv			: TEXCOORD0;
	float2 lightmapUV	: TEXCOORD1;
	float4 color		: COLOR;
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

// Vertex shader
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
half4 frag(Varyings input) : SV_Target
{
	half4 diffSamplerColor = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, input.uv );

	// Falloff. Convert the angle between the normal and the camera direction into a lookup for the gradient
	half normalDotEye = dot( input.normalWS, input.viewDirectionWS.xyz);
	half falloffU = clamp( 1 - abs( normalDotEye ), 0.02, 0.98 );
	half4 falloffSamplerColor = FALLOFF_POWER * SAMPLE_TEXTURE2D( _FalloffSampler, sampler_FalloffSampler, float2( falloffU, 0.25f ) );
	half3 combinedColor = lerp( diffSamplerColor.rgb, falloffSamplerColor.rgb * diffSamplerColor.rgb, falloffSamplerColor.a );

	// Rimlight
	half rimlightDot = saturate( 0.5 * ( dot( input.normalWS, _MainLightPosition) + 1.0 ) );
	falloffU = saturate( rimlightDot * falloffU );
	falloffU = SAMPLE_TEXTURE2D( _RimLightSampler, sampler_RimLightSampler, float2( falloffU, 0.25f ) ).r;
	half3 lightColor = diffSamplerColor.rgb * 0.5; // * 2.0;
	combinedColor += falloffU * lightColor;

	half3 shadowColor = _ShadowColor.rgb * combinedColor;
	half attenuation = saturate( 2.0 * _MainLightPosition - 1.0 );
	combinedColor = lerp( shadowColor, combinedColor, attenuation );

	return half4( combinedColor, diffSamplerColor.a ) * _Color * _LightColor0;
}
