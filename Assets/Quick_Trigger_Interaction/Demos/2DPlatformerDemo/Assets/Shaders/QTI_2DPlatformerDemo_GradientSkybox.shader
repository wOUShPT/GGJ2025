// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AstralShift/QTI/2DPlatformerDemo/GradientSkybox"
{
	Properties
	{
		_Color1("Color 1", Color) = (0,0,0,0)
		_Color12Height("Color 1/2 Height", Range( 0 , 1)) = 0
		_Color2("Color 2", Color) = (0.7830189,0.3103631,0,1)
		_Color34Height("Color 3/4 Height", Range( 0 , 1)) = 0
		_Color3("Color 3", Color) = (0,0,0,0)
		_Color23Height1("Color 2/3 Height", Range( 0 , 1)) = 0
		_Color4("Color 4", Color) = (0.7830189,0.3103631,0,1)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float4 _Color1;
			uniform float _Color12Height;
			uniform float4 _Color2;
			uniform float _Color23Height1;
			uniform float4 _Color3;
			uniform float _Color34Height;
			uniform float4 _Color4;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 appendResult10 = (float2(0.0 , (-1.5 + (_Color12Height - 0.0) * (2.0 - -1.5) / (1.0 - 0.0))));
				float2 texCoord8 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult10;
				float4 lerpResult4 = lerp( ( _Color1 * texCoord8.y ) , ( _Color2 * ( 1.0 - texCoord8.y ) ) , 0.5);
				float2 appendResult38 = (float2(0.0 , (-1.5 + (_Color23Height1 - 0.0) * (2.0 - -1.5) / (1.0 - 0.0))));
				float2 texCoord37 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult38;
				float4 lerpResult45 = lerp( ( lerpResult4 * texCoord37.y ) , ( ( 1.0 - texCoord37.y ) * _Color3 ) , 0.5);
				float2 appendResult25 = (float2(0.0 , (-1.5 + (_Color34Height - 0.0) * (2.0 - -1.5) / (1.0 - 0.0))));
				float2 texCoord24 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult25;
				float4 lerpResult34 = lerp( ( lerpResult45 * texCoord24.y ) , ( ( 1.0 - texCoord24.y ) * _Color4 ) , 0.5);
				
				
				finalColor = lerpResult34;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-565.0903,-247.9272;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;4;-300.7417,-338.2674;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-1118.227,-698.7651;Inherit;False;Property;_Color1;Color 1;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.3301418,0.6509434,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-28.11293,-100.807;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-25.09848,202.4997;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-554.3815,-116.6322;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;45;225.7971,58.91357;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;36;-239.6996,100.9122;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;27;-546.0375,229.5248;Inherit;False;Property;_Color3;Color 3;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.8414739,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-1117.254,-252.4964;Inherit;False;Property;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;0.7830189,0.3103631,0,1;1,0.321268,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;22;-762.0217,-320.144;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-571.5696,-475.075;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-16.24142,348.5888;Inherit;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;451.1341,929.2918;Inherit;False;Constant;_Float1;Float 0;2;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;464.8854,360.7415;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;28;267.7748,574.7814;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;467.4774,649.4228;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;34;733.5356,478.1204;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;26;-43.06223,675.786;Inherit;False;Property;_Color4;Color 4;6;0;Create;True;0;0;0;False;0;False;0.7830189,0.3103631,0,1;0.2071455,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;32;-439.9864,555.6523;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1.5;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-773.0254,550.2395;Inherit;False;Property;_Color34Height;Color 3/4 Height;3;0;Create;True;0;0;0;False;0;False;0;0.528;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-55.43992,484.8449;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;25;-227.5206,532.8011;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;974.6359,474.6106;Float;False;True;-1;2;ASEMaterialInspector;100;5;GradientSkybox;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1424.74,-467.5669;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;10;-1596.822,-419.611;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;23;-1806.246,-395.8452;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1.5;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2133.263,-397.5336;Inherit;False;Property;_Color12Height;Color 1/2 Height;1;0;Create;True;0;0;0;False;0;False;0;0.849;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;37;-952.5059,23.32676;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;38;-1139.01,71.82685;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;39;-1351.475,94.67796;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1.5;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1666.671,94.08836;Inherit;False;Property;_Color23Height1;Color 2/3 Height;5;0;Create;True;0;0;0;False;0;False;0;0.226;0;1;0;1;FLOAT;0
WireConnection;21;0;14;0
WireConnection;21;1;22;0
WireConnection;4;0;20;0
WireConnection;4;1;21;0
WireConnection;4;2;13;0
WireConnection;42;0;4;0
WireConnection;42;1;37;2
WireConnection;43;0;36;0
WireConnection;43;1;27;0
WireConnection;45;0;42;0
WireConnection;45;1;43;0
WireConnection;45;2;46;0
WireConnection;36;0;37;2
WireConnection;22;0;8;2
WireConnection;20;0;1;0
WireConnection;20;1;8;2
WireConnection;30;0;45;0
WireConnection;30;1;24;2
WireConnection;28;0;24;2
WireConnection;29;0;28;0
WireConnection;29;1;26;0
WireConnection;34;0;30;0
WireConnection;34;1;29;0
WireConnection;34;2;31;0
WireConnection;32;0;33;0
WireConnection;24;1;25;0
WireConnection;25;1;32;0
WireConnection;0;0;34;0
WireConnection;8;1;10;0
WireConnection;10;1;23;0
WireConnection;23;0;9;0
WireConnection;37;1;38;0
WireConnection;38;1;39;0
WireConnection;39;0;40;0
ASEEND*/
//CHKSM=FD8701F5691FAD2367F8820DA19CC171DFAED088