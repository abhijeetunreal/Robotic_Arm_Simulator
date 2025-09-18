Shader "UI/AirMouseRaymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AirMouseRotation("AirMouse Rotation", Vector) = (0, 0, 0, 0)
        
        // NEW: Color properties that will appear in the Inspector.
        [HDR]_GlowColor ("Glow & Rim Color", Color) = (0.001, 0.002, 0.009, 1)
        [HDR]_FloorColor ("Floor Color", Color) = (0, 1, 8, 1)
        _SkyMultiplier ("Sky Brightness", Float) = 500
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            // Input from C# script and Material Inspector
            float4 _AirMouseRotation;
            fixed4 _GlowColor;
            fixed4 _FloorColor;
            float _SkyMultiplier;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2x2 rotate2D(float angle) {
                float s = sin(angle);
                float c = cos(angle);
                return float2x2(c, -s, s, c);
            }

            float D(float3 p, inout float d, inout float G, float2x2 Rxy, float2x2 Rxz) {
                p.xy = mul(Rxy, p.xy);
                p.xz = mul(Rxz, p.xz);
                float3 S = sin(123.0 * p);
                float3 p8 = p * p * p * p; p8 *= p8;
                d = pow(dot(p8, float3(1,1,1)), 0.125) - 0.5;
                G = min(G, max(abs(length(p) - 0.6), d - pow(1.0 + S.x * S.y * S.z, 8.0) / 1e5));
                return d;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = 1.0, z = 0.0, G = 9.0, M = 1e-3;

                float3 r_res = float3(_ScreenParams.xy, 0);
                float3 O = float3(0,0,0);
                float3 I = normalize(float3(i.uv * r_res.xy - 0.5 * r_res.xy, r_res.y));
                
                // MODIFIED: Base color is now taken from the Inspector property.
                float3 B = _GlowColor.rgb;
                
                float2x2 Rxy = rotate2D(_AirMouseRotation.y);
                float2x2 Rxz = rotate2D(_AirMouseRotation.x);

                float3 p = float3(0,0,0);
                for (int j = 0; j < 100; j++) {
                    if (z > 9.0 || d < M) break;
                    p = z * I;
                    p.z -= 2.0;
                    z += D(p, d, G, Rxy, Rxz);
                }

                if (z < 9.0) {
                    float3 r_eps = float3(M, 0, 0);
                    float d_dummy = 0, g_dummy = 0;
                    
                    O.x = D(p + r_eps.xyy, d_dummy, g_dummy, Rxy, Rxz) - D(p - r_eps.xyy, d_dummy, g_dummy, Rxy, Rxz);
                    O.y = D(p + r_eps.yxy, d_dummy, g_dummy, Rxy, Rxz) - D(p - r_eps.yxy, d_dummy, g_dummy, Rxy, Rxz);
                    O.z = D(p + r_eps.yyx, d_dummy, g_dummy, Rxy, Rxz) - D(p - r_eps.yyx, d_dummy, g_dummy, Rxy, Rxz);
                    O = normalize(O);

                    float fresnel = 1.0 + dot(O, I);
                    float3 r_reflect = reflect(I, O);
                    
                    float2 C = (p + r_reflect * (5.0 - p.y) / abs(r_reflect.y)).xz;
                    float sky_d = sqrt(dot(C, C)) + 1.0;
                    
                    // MODIFIED: Sky and Floor colors now use the Inspector properties.
                    float3 sky = _SkyMultiplier * smoothstep(5.0, 4.0, sky_d) * sky_d * B;
                    float3 floor = exp(-2.0 * length(C)) * _FloorColor.rgb;

                    float3 finalColor = fresnel * fresnel * (r_reflect.y > 0.0 ? sky : floor);
                    finalColor += pow(1.0 + O.y, 5.0) * B;
                    O = finalColor;
                }
                
                float4 outputColor = sqrt(float4(O + B / G, 1.0));
                return outputColor;
            }
            ENDCG
        }
    }
}