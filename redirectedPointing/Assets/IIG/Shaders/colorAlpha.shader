Shader "Custom/colorAlpha" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	Category{
		SubShader {
			Tags {"Queue"="Transparent" "RenderType"="Transparent"}
			LOD 200
			Blend SrcAlpha OneMinusSrcAlpha 
			Pass{	
	
			CGPROGRAM
			#pragma vertex vert
    		#pragma fragment frag
   		    #include "UnityCG.cginc"
   		    
   			float4 _Color;

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
				half4 texcol = _Color; 
			
			    return texcol;
			}
			ENDCG
			} 
		}
	} 
	FallBack "Diffuse"
}
