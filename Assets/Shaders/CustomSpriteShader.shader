Shader "Thinker/SpriteColorizer"
{
	Properties
	{
		[Header(Texture)]
		_MainTex ("Main Texture", 2D) = "white" {}

		[Header(Color)]
		_Color("Tint", Color) = (1,1,1,1)
		_Colorize("Colorize",Color) = (1,1,1,0)
	}
	SubShader
	{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off
		
		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenPos:TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			fixed4 _Color;
			fixed4 _Colorize;
	
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = UnityPixelSnap(o.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;

				// Colorize
				col = fixed4(_Colorize.r* _Colorize.a + col.r*(1-_Colorize.a),
							 _Colorize.g * _Colorize.a + col.g*(1 - _Colorize.a),
							 _Colorize.b * _Colorize.a + col.b*(1 - _Colorize.a), col.a);

				return col;
			}

			ENDCG
		}
	}
}