// Alien Planet Landscape Shader
// Original by XT95, License CC0
// Ported to Unity HLSL and modified for scene blending and color control.

Shader "Unlit/AlienPlanetShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        _BlendHeight ("Blend Height", Range(0.0, 1.0)) = 0.4
        
        // --- Color Controls ---
        _SkyColor1 ("Sky Color 1 (Horizon)", Color) = (0.35, 0.45, 0.6, 1.0)
        _SkyColor2 ("Sky Color 2 (Zenith)", Color) = (0.4, 0.7, 1.0, 1.0)
        _SunColor1 ("Sun Color (Glow)", Color) = (1.0, 0.6, 0.4, 1.0)
        _SunColor2 ("Sun Color (Core)", Color) = (1.0, 0.9, 0.7, 1.0)
        _MountainColor ("Mountain Color", Color) = (0.97, 0.80, 0.67, 1.0)
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

            #define PI 3.141592654
            #define TAU (2.0*PI)
            #define TOLERANCE 0.00001
            #define MAX_ITER 65
            #define MIN_DISTANCE 0.01
            #define MAX_DISTANCE 9.0

            // ---- Ported GLSL Functions and Constants ----
            static const float4 planet = float4(80.0, -20.0, 100.0, 50.0) * 1000.0;

            sampler2D _MainTex;
            float _BlendHeight;
            fixed4 _SkyColor1;
            fixed4 _SkyColor2;
            fixed4 _SunColor1;
            fixed4 _SunColor2;
            fixed4 _MountainColor;


            void rot(inout float2 p, float a) {
                float c = cos(a);
                float s = sin(a);
                p = float2(p.x*c + p.y*s, -p.x*s + p.y*c);
            }

            float2 mod2(inout float2 p, float2 size) {
                float2 c = floor((p + size*0.5)/size);
                p = fmod(p + size*0.5,size) - size*0.5;
                return c;
            }

            float2 hash(float2 p) {
                p = float2(dot (p, float2 (127.1, 311.7)), dot (p, float2 (269.5, 183.3)));
                return -1. + 2.*frac (sin (p)*43758.5453123);
            }
            
            float noise1(float2 p) {
                float2 n = mod2(p, float2(1.0, 1.0));
                float2 hh = hash(sqrt(2.0)*(n+1000.0));
                hh.x *= hh.y;
                float r = 0.225;
                float d = length(p) - 2.0*r;
                float h = hh.x*smoothstep(0.0, r, -d);
                return h*0.25;
            }

            float egg(float2 p, float ra, float rb) {
                const float k = sqrt(3.0);
                p.x = abs(p.x);
                float r = ra - rb;
                return ((p.y<0.0) ? length(float2(p.x, p.y )) - r :
                       (k*(p.x+r)<p.y) ? length(float2(p.x, p.y-k*r)) :
                       length(float2(p.x+r,p.y )) - 2.0*r) - rb;
            }

            float noise2(float2 p) {
                float2 n = mod2(p, float2(1.0, 1.0));
                float2 hh = hash(sqrt(2.0)*(n+1000.0));
                hh.x *= hh.y;
                rot(p, TAU*hh.y);
                float r = 0.45;
                float d = egg(p, 0.75*r, 0.5*r*abs(hh.y));
                float h = (hh.x)*smoothstep(0.0, r, -2.0*d);
                return h*0.275;
            }
            
            float height(float2 p, float dd, int mx) {
                const float aa = 0.45;
                const float ff = 2.03;
                const float tt = 1.2;
                const float oo = 3.93;
                
                float a = 1.0;
                float o = 0.2;
                float s = 0.0;
                float d = 0.0;
                
                for (int i = 0; i < 4; ++i) {
                    s += a*noise2(p);
                    d += abs(a);
                    p += o; a *= aa; p *= ff; o *= oo; rot(p, tt);
                }
                
                float lod = s/d;
                float rdd = dd/MAX_DISTANCE;
                mx = (int)lerp(float(4), float(mx), step(rdd, 0.65));

                for (int i = 4; i < mx; ++i) {
                    s += a*noise1(p);
                    d += abs(a);
                    p += o; a *= aa; p *= ff; o *= oo; rot(p, tt);
                }
                
                float hid = (s/d);
                return lerp(hid, lod, smoothstep(0.25, 0.65, rdd));
            }

            float hiheight(float2 p, float d) { return height(p, d, 8); }

            float3 normal(float2 p, float d) {
                float2 eps = float2(0.00125, 0.0);
                float3 n;
                n.x = (hiheight(p - eps.xy, d) - hiheight(p + eps.xy, d));
                n.y = 2.0*eps.x;
                n.z = (hiheight(p - eps.yx, d) - hiheight(p + eps.yx, d));
                return normalize(n);
            }

            float march(float3 ro, float3 rd, out int max_iter) {
                float d = MIN_DISTANCE;
                for (int i = 0; i < MAX_ITER; ++i) {
                    max_iter = i;
                    float3 p = ro + d*rd;
                    float h = height(p.xz, d, 6);
                    if (p.y - h < TOLERANCE || d > MAX_DISTANCE) break;
                    d += max(p.y - h, TOLERANCE) * 0.5;
                }
                return d;
            }

            float3 sunDirection() { return normalize(float3(-0.5, 0.085, 1.0)); }

            float2 raySphere(float3 ro, float3 rd, float4 sphere) {
                float3 m = ro - sphere.xyz;
                float b = dot(m, rd);
                float c = dot(m, m) - sphere.w*sphere.w;
                if(c > 0.0 && b > 0.0) return float2(-1.0, -1.0);
                float discr = b*b - c;
                if(discr < 0.0) return float2(-1.0, -1.0);
                float s = sqrt(discr);
                return float2(-b - s, -b + s);
            }
            
            float3 skyColor(float3 ro, float3 rd) {
                float3 sunDir = sunDirection();
                float sunDot = max(dot(rd, sunDir), 0.0);
                float angle = atan2(rd.y, length(rd.xz))*2.0/PI;
                
                float3 skyCol3 = pow(_SkyColor1.rgb, float3(0.25, 0.25, 0.25));

                float3 skyCol = lerp(lerp(_SkyColor1.rgb, _SkyColor2.rgb, max(0.0, angle)), skyCol3, clamp(-angle*2.0, 0.0, 1.0));
                float3 sunCol = 0.5*_SunColor1.rgb*pow(sunDot, 20.0) + 8.0*_SunColor2.rgb*pow(sunDot, 2000.0);
                float3 dust = pow(_SunColor2.rgb*_MountainColor.rgb, float3(1.75,1.75,1.75))*smoothstep(0.05, -0.1, rd.y)*0.5;
                
                float2 si = raySphere(ro, rd, planet);
                float3 finalCol = skyCol + sunCol + dust;
                if(si.x > 0.0) {
                    float3 p = ro + si.x*rd;
                    float3 n = normalize(p - planet.xyz);
                    float diff = max(dot(n, sunDir), 0.0);
                    float border = max(dot(n, -rd), 0.0);
                    float lat = (p.x+p.y)*0.0005;
                    float psin_lat = 0.5 + 0.5*sin(lat);
                    float3 pcol = lerp(1.3*float3(0.9, 0.8, 0.7), 0.3*float3(0.9, 0.8, 0.7), pow(psin_lat, 0.5));
                    finalCol += pow(diff, 0.75)*pcol*smoothstep(-0.075, 0.0, rd.y)*smoothstep(0.0, 0.1, border);
                }
                return finalCol;
            }

            float3 getColor(float3 ro, float3 rd) {
                int max_iter = 0;
                float3 skyCol = skyColor(ro, rd);
                float d = march(ro, rd, max_iter);

                if (d < MAX_DISTANCE) {
                    float3 p = ro + d*rd;
                    float3 n = normal(p.xz, d);
                    float3 sunDir = sunDirection();
                    float diff = max(0.0, dot(sunDir, n));
                    float3 skyCol3 = pow(_SkyColor1.rgb, float3(0.25, 0.25, 0.25));

                    float3 shade = _SunColor2.rgb*lerp(0.2, 1.0, pow(diff, 0.75));
                    float3 ref = reflect(rd, n);
                    float3 rcol = skyColor(p, ref);
                    float fre = pow(1.0 - max(dot(n, -rd), 0.0), 5.0);
                    
                    float3 col = _MountainColor.rgb*0.2*skyCol3;
                    col += shade*_MountainColor.rgb;
                    col += rcol*fre*0.5;
                    col += (1.0*p.y);
                    col = tanh(col);
                    return lerp(col, skyCol, smoothstep(0.5*MAX_DISTANCE, 1.0*MAX_DISTANCE, d));
                }
                return skyCol;
            }

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 p = -1.0 + 2.0 * i.uv;
                p.x *= _ScreenParams.x / _ScreenParams.y;
                
                float off = 0.5 * _Time.y;
                float3 ro = float3(0.5, 0.75, -2.0 + off);
                float3 la = ro + float3(0.0, -0.30, 2.0);
                
                float3 ww = normalize(la - ro);
                float3 uu = normalize(cross(float3(0.0,1.0,0.0), ww));
                float3 vv = normalize(cross(ww, uu));
                float3 rd = normalize(p.x*uu + p.y*vv + 2.0*ww);

                // Calculate the color of the alien planet landscape
                float3 landscapeCol = getColor(ro, rd);
                
                // Get the original scene color (with the aircraft) from the camera's texture
                float3 sceneColor = tex2D(_MainTex, i.uv).rgb;

                // Create a blend factor based on the screen's vertical position (i.uv.y)
                // This creates a soft gradient from the landscape at the bottom to the scene at the top
                float blendFactor = smoothstep(_BlendHeight, _BlendHeight + 0.25, i.uv.y);

                // Mix the landscape color with the original scene color based on the blend factor
                float3 finalCol = lerp(landscapeCol, sceneColor, blendFactor);
                
                return fixed4(finalCol, 1.0);
            }
            ENDCG
        }
    }
}
