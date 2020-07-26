Shader "Custom/myshader"
{  Properties
{
	_MainTex("Texture", 2D) = "white" {}

// 滚动速度
_Speed("Speed",float) = 1
}
SubShader
{
	Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
	LOD 100

	Pass
	{
		CGPROGRAM
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

			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float _Speed;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);

			// 添加 x 上的滚动动画效果 （根据需要可以进行 y 上的效果添加）
			o.uv = o.uv + float2(_Speed,0) * _Time.y;

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			// sample the texture
			fixed4 col = tex2D(_MainTex, i.uv);

			return col;
		}
		ENDCG
	}
}

}
