Shader "Custom/texturedAlpha" {
	Properties {
		_AlphaBlend ("AlphaBlend", Range (0.0, 1.0)) = 0.5
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	Category{
		SubShader {
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			//Tags {"Queue"="Transparent" "RenderType"="Transparent"}
			LOD 200
			Blend SrcAlpha OneMinusSrcAlpha 
			Pass{	
	
			CGPROGRAM
			#pragma vertex vert
    		#pragma fragment frag
   		    #include "UnityCG.cginc"
   		    
   		    float _AlphaBlend;
   		    
			sampler2D _MainTex;
			float _MainTex_ST;
   		    struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
			    v2f o;
			    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			    o.uv = v.texcoord;//TRANSFORM_TEX (v.texcoord, _MainTex);
			    return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 texcol = tex2D(_MainTex, i.uv); 
				texcol.a = texcol.a * _AlphaBlend; 
			    return texcol;
			}
			ENDCG
			} 
		}
	} 
	FallBack "Diffuse"
}
