////////////////////////////////////////////////////////////////////////////////////
//  CAMERA FILTER PACK - by VETASOFT 2014 //////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

Shader "CameraFilterPack/Oculus_NightVision2" {
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TimeX ("Time", Range(0.0, 1.0)) = 1.0
		_Distortion ("_Distortion", Range(0.0, 1.0)) = 0.3
		_ScreenResolution ("_ScreenResolution", Vector) = (0.,0.,0.,0.)
		_BinocularSize ("_BinocularSize", Range(0.0, 1.0)) = 0.5
		_BinocularDistance ("_BinocularDistance", Range(0.0, 1.0)) = 0.5
		_Greenness ("_Greenness", Range(0.0, 1.0)) = 0.4
	}
	SubShader 
	{
		Pass
		{
			ZTest Always
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma glsl
			#include "UnityCG.cginc"
			
			
			uniform sampler2D _MainTex;
			uniform float _TimeX;
			uniform float _Distortion;
			uniform float4 _ScreenResolution;
			uniform float _BinocularSize;
			uniform float _BinocularDistance;
			uniform float _Greenness;
			
		       struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f
            {
                  half2 texcoord  : TEXCOORD0;
                  float4 vertex   : SV_POSITION;
                  fixed4 color    : COLOR;
           };   
             
  			v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                
                return OUT;
            }
            
            struct Circle
			{
				float2 center;
				float radius;
			};

			float remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
			{
			    return (value - inputMin) * ((outputMax - outputMin) / (inputMax - inputMin)) + outputMin;
			}

			float rand(float2 n, float time)
			{
			  return 0.5 + 0.5 * frac(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.5453 + time);
			}

			float4 circle_mask_color(Circle circle, float2 position)
			{
				float d = distance(circle.center, position);
				if(d > circle.radius)
				{
					return fixed4(0.0, 0.0, 0.0, 1.0);
				}
				
				float distanceFromCircle = circle.radius - d;
				float intencity = smoothstep(
											    0.0, 1.0, 
											    clamp(
												    remap(distanceFromCircle, 0.0, 0.1, 0.0, 1.0),
												    0.0,
												    1.0
											    )
											);
				return fixed4(intencity, intencity, intencity, 1.0);
			}

			float4 mask_blend(float4 a, float4 b)
			{
				fixed4 one = fixed4(1.0, 1.0, 1.0, 1.0);
				return one - (one - a) * (one - b);
			}

			float f1(float x)
			{
				return -4.0 * pow(x - 0.5, 2.0) + 1.0;
			}

			float4 frag (v2f i) : COLOR
			{
				
				float2 uv = i.texcoord.xy;
				
				float wide = _ScreenResolution.x / _ScreenResolution.y;
				float high = 1.0;
				
				float2 position = float2(uv.x * wide, uv.y);
				
				Circle circle_a = Circle(float2(_BinocularDistance, 0.5), _BinocularSize);
				Circle circle_b = Circle(float2(wide - _BinocularDistance, 0.5), _BinocularSize);
		
				float4 mask_a = circle_mask_color(circle_a, position);
				float4 mask_b = circle_mask_color(circle_b, position);
				float4 mask = mask_blend(mask_a, mask_b);
				
				float4 coloring = float4(1.0 - _Greenness, 1.0, 1.0 - _Greenness, 1.0);
				
				float noise = rand(uv * float2(0.1, 1.0), _TimeX * 1.0);
				float noiseColor = 1.0 - (1.0 - noise) * 0.3;
				float4 noising = float4(noiseColor, noiseColor, noiseColor, 1.0);
				
				float warpLine = frac(-_TimeX * 0.5);
				
				float warpArg = remap(clamp((position.y - warpLine) - 0.05, 0.0, 0.1), 0.0, 0.1, 0.0, 1.0);
				float offset = sin(warpArg * 10.0)  * f1(warpArg);
				
				float4 lineNoise = float4(1.0, 1.0, 1.0, 1.0);
				if(abs(uv.y - frac(-_TimeX * 19.0)) < 0.0005) lineNoise = float4(0.5, 0.5, 0.5, 1.0);
				
				float4 base = tex2D(_MainTex, uv + float2(offset * 0.02, 0.0));
				
				return (base * mask * coloring * noising * lineNoise);
				
			}
			
			ENDCG
		}
		
	}
}