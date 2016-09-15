// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/CellShading"
{
	Properties
	{
		_Color ("Diffuse Color", Color) = (1,1,1,1)
		_UnlitColor ("Unlit Diffuse Color", Color) = (0.5,0.5,0.5,1)
		_ShadingAngle ("Diffuse Threshold", Range(0,1)) = 0.2

		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
      	_LitOutlineThickness ("Lit Outline Thickness", Range(0,1)) = 0.1
      	_UnlitOutlineThickness ("Unlit Outline Thickness", Range(0,1)) = 0.4
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normal : TEXCOORD1;				 
			};

			float4 _LightColor0;

			float _ShadingAngle;
			float4 _Color;
			float4 _UnlitColor;

			float4 _OutlineColor;
			float _LitOutlineThickness;
			float _UnlitOutlineThickness;



			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(float4(v.normal, 0.0), unity_WorldToObject).xyz;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normalDir = normalize(i.normal);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);

				float4 diffuse = _UnlitColor;

				if (max(0.0, dot(normalDir, lightDir)) >= _ShadingAngle) {
					diffuse = _Color;
				}

				if (dot(viewDir, normalDir) < lerp(_UnlitOutlineThickness, _LitOutlineThickness, max(0.0, dot(normalDir, lightDir)))) {
					diffuse = _OutlineColor;
				}

				//if (dot(normalDir, lightDir) > 0.0 	&& pow(max(0.0, dot(reflect(-lightDir, normalDir), viewDir)), 10) > 0.5) {
				//	diffuse = _LightColor0 * float4(1.0, 1.0, 1.0, 1.0) * diffuse;
				//}

				return _LightColor0 *  diffuse;
			}
			ENDCG
		}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
