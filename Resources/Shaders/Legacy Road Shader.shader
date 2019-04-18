Shader "Custom/Legacy Road Shader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Tiling("Tiling", Vector) = (1,1,0,0) 
		_Offset("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM 
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
		float4 _Tiling;
		float4 _Offset;

        struct Input
        {
            float2 uv_MainTex;
			float2 getUV2;
			float2 customUV;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.getUV2 = v.texcoord1.xy;
			o.customUV = v.texcoord.xy * _Tiling.xy + _Offset.xy;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 uv2 = IN.getUV2;
			float x = IN.customUV.x / uv2.x;
			float2 uv = float2(x, IN.customUV.y);
            fixed4 c = tex2D (_MainTex, uv);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
		}
        ENDCG
    }
    FallBack "Diffuse"
}
