// Converted to Unity HLSL
// Original by Stephane Cuillerdier - Aiekick/2021 (github:aiekick)
// Final version with all fixes and scene blending.

Shader "Unlit/HexaTunnelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        _ReflectionTex ("Reflection Cubemap", CUBE) = "_Default" {}
        _FOV ("Field of View", Range(0.1, 2.0)) = 0.4
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
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            samplerCUBE _ReflectionTex;
            float _FOV;

            // ---- GLSL to HLSL Ported Functions ----

            float3x3 rotz(float a) {
                float c = cos(a); float s = sin(a);
                return float3x3(c,-s,0,  s,c,0,  0,0,1);
            }

            float sdHexPrism( float3 p, float2 h ) {
                const float3 k = float3(-0.8660254, 0.5, 0.57735);
                p = abs(p);
                p.xy -= 2.0*min(dot(k.xy, p.xy), 0.0)*k.xy;
                float2 d = float2(
                    length(p.xy - float2(clamp(p.x, -k.z*h.x, k.z*h.x), h.x))*sign(p.y - h.x),
                    p.z-h.y );
                return min(max(d.x,d.y),0.0) + length(max(d,0.0));
            }

            #define ox 1.3
            #define oz 1.5

            float var_z = 0.0;

            void common_map(float3 p, out float df0, out float df1) {
                p = mul(rotz(p.z * 0.05), p);
                
                p.y = 5.0 + 5.0 * var_z - abs(p.y);
                float wave = sin(length(p.xz) * 0.25 - _Time.y * 1.5);
                df0 = abs(p.y + wave) - 1.0;
                float2 hex_size = float2(0.25 + p.y * 0.25, 10.0);
                float3 q0 = p;
                q0.x = fmod(q0.x - ox, ox + ox) - ox;
                q0.z = fmod(q0.z - oz * 0.5, oz) - oz * 0.5;
                float hex0 = sdHexPrism(q0.xzy, hex_size) - 0.2; 
                float3 q1 = p;
                q1.x = fmod(q1.x, ox + ox) - ox;
                q1.z = fmod(q1.z, oz) - oz * 0.5;
                float hex1 = sdHexPrism(q1.xzy, hex_size) - 0.2; 
                df1 = min(hex0, hex1);
            }

            float smin( float a, float b, float k ) {
                float h = clamp( 0.5 + 0.5*(b-a)/k, 0.0, 1.0 );
                return lerp( b, a, h ) - k*h*(1.0-h);
            }

            float smax(float a, float b, float k) { return smin(a, b, -k); }

            float map(float3 p) {
                float df0, df1;
                common_map(p, df0, df1);
                return smax(df0, df1, 0.1);
            }

            float mat(float3 p) {
                float df0, df1;
                common_map(p, df0, df1);
                return (df0 > df1) ? 1.0 : 0.0;
            }

            float3 getNormal(float3 p) {
                const float3 e = float3(0.1, 0, 0);
                return normalize(float3(
                    map(p+e)-map(p-e),
                    map(p+e.yxz)-map(p-e.yxz),
                    map(p+e.zyx)-map(p-e.zyx)));
            }
            
            float getShadow(float3 ro, float3 rd, float minD, float maxD, float k) {
                float res = 1.0;
                float d = minD;
                for(int i = 0; i < 20; ++i) {
                    float s = map(ro + rd * d);
                    if( abs(s)<d*d*1e-5 ) return 0.0;
                    res = min( res, k * s / d );
                    d += s;
                    if(d >= maxD) break;
                }
                return res;
            }

            float3 cam(float2 uv, float3 ro, float3 cv, float fov) {
                float3 z = normalize(cv-ro);
                float3 x = normalize(cross(float3(0,1,0), z));
                float3 y = cross(z,x);
                return normalize(z + fov*uv.x*x + fov*uv.y*y);
            }
            
            // ---- Unity Specific Functions ----
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 si = _ScreenParams.xy;
                float2 uvc = (2.0 * i.uv - 1.0) * float2(si.x / si.y, 1.0);
                
                float3 ro = float3(0.0, 0.0, _Time.y * 20.0 + 5.0);
                float3 cv = ro + float3(0.0, 0.0, 4.0);
                float3 rd = cam(uvc, ro, cv, _FOV);

                float3 col = float3(0,0,0);
                float3 p = ro;
                float s = 1.0, d = 0.0;
                const float md = 70.0;
                
                for (int j = 0; j < 200; j++) {
                    if (d*d/s > 1e6 || d > md) break;
                    var_z = sin(p.z * 0.1) * 0.5 + 0.5;
                    s = map(p);
                    d += s * 0.5;
                    p = ro + rd * d;
                }
                
                if (d < md) {
                    float3 n = getNormal(p);
                    float3 lp = float3(0,5,0);
                    float3 ld = normalize(lp - p);
                    float diff = pow(dot(n, ld) * .5 + .5, 2.0);
                    float sha = clamp(getShadow(p, ld, 0.01, 150.0, 5.0), 0. ,0.9);
                    
                    if (mat(p) > 0.5) { // Hexagon face
                        col = lerp(float3(1.5, 1.0, 0.0), float3(2.0, 2.0, 2.0), var_z);
                    } else { // Hexagon sides
                        col = float3(1.0, 0.85, 0.0) * 0.75;    
                    }
                    
                    col *= texCUBE(_ReflectionTex, reflect(rd, n)).rgb;
                    col += diff * sha * 0.5;
                }
                
                // Get the original scene color (with your cube) from the camera's view
                fixed4 sceneColor = tex2D(_MainTex, i.uv);

                // Calculate an "alpha" value for our tunnel based on the distance/fog
                // When the tunnel is far away (d is large), alpha is near 0.
                // When the tunnel is close (d is small), alpha is near 1.
                float alpha = exp(1.0 - d * d * 0.001);

                // Blend the original scene color with our calculated tunnel color using the alpha
                // 'lerp' is a linear interpolation function: lerp(a, b, t)
                col = lerp(sceneColor.rgb, col, alpha);

                // Clamp the final mixed color to avoid overly bright spots
                col = clamp(col, 0., 1.);
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}