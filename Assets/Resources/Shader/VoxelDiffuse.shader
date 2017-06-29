Shader "Voxel/Diffuse"
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
		_TileSize ("Tile Size", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct voxelData
			{
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float2 textCoord : TEXCOORD0;
				float2 tileCoord : TEXCOORD1;
				float3 color: COLOR;
			};

			struct fragData
			{
				float4 pos: SV_POSITION;
				float2 textCoord: TEXCOORD1;
				float3 color : COLOR;
			};

			sampler2D _MainTex;
			float _TileSize;
			
			fragData vert (voxelData v)
			{
				fragData o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.textCoord = (_TileSize * v.tileCoord) + (v.textCoord * _TileSize);
				o.color = v.color; //ShadeVertexLights(v.vertex, v.normal) * v.color;

				return o;
			}
			
			float4 frag (fragData f) : SV_Target
			{
				float4 col = tex2D(_MainTex, f.textCoord) * float4(f.color, 1);
				return col;
			}
			ENDCG
		}
	}
}
