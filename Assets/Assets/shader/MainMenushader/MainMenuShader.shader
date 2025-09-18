// Converted GLSL Shader for Unity
// This version fixes the color inversion when the grid is on top and reinstates color customization.

Shader "Gemini/Converted/CustomizableBackground_Corrected"
{
    Properties
    {
        [Header(Camera Animation)]
        _RotationSpeed ("Rotation Speed", Range(-2, 2)) = 0.2
        _HeightAmplitude ("Height Amplitude", Range(-3, 3)) = 0.5
        _HeightSpeed ("Height Speed", Range(0, 2)) = 0.3

        [Header(Color Customization)]
        // These will be used in the palette function, respecting its original mathematical structure.
        _SkyGradientFactor1 ("Sky Gradient Factor 1", Color) = (0.5, 0.45, 0.55, 1) // Corresponds to 'a' in original palette
        _SkyGradientFactor2 ("Sky Gradient Factor 2", Color) = (0.5, 0.5, 0.5, 1)   // Corresponds to 'b' in original palette
        [HDR] _GridGlowColor ("Grid Glow Color (HDR)", Color) = (5, 4, 2, 1) // This will be multiplied by 0.075 as per original logic.
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; };

            // Properties from the Inspector
            float _RotationSpeed;
            float _HeightAmplitude;
            float _HeightSpeed;
            fixed4 _SkyGradientFactor1;
            fixed4 _SkyGradientFactor2;
            half4 _GridGlowColor; // Use half4 for HDR if supported, fixed4 otherwise

            // ------------------------------------------------------------------
            // Helper Functions
            // ------------------------------------------------------------------

            float2 squareFrame(float2 screenSize, float2 coord) {
                float2 position = 2.0 * (coord / screenSize) - 1.0;
                position.x *= screenSize.x / screenSize.y;
                return position;
            }

            float3x3 calcLookAtMatrix(float3 origin, float3 target, float roll) {
                float3 rr = float3(sin(roll), cos(roll), 0.0);
                float3 ww = normalize(target - origin);
                float3 uu = normalize(cross(ww, rr));
                float3 vv = normalize(cross(uu, ww));
                return float3x3(uu, vv, ww);
            }

            float3 getRay(float3x3 camMat, float2 screenPos, float lensLength) {
                return normalize(mul(camMat, float3(screenPos, lensLength)));
            }
            
            struct CameraVectors { float3 origin; float3 direction; };

            CameraVectors orbitCamera(float camAngle, float camHeight, float camDistance, float2 screenResolution, float2 coord) {
                float2 screenPos = squareFrame(screenResolution, coord);
                float3 rayTarget = float3(0.0, 0.0, 0.0);
                float3 rayOrigin = float3(camDistance * sin(camAngle), camHeight, camDistance * cos(camAngle));
                float3x3 camMat = calcLookAtMatrix(rayOrigin, rayTarget, 0.0);
                float3 rayDirection = getRay(camMat, screenPos, 2.0);
                
                CameraVectors cam;
                cam.origin = rayOrigin;
                cam.direction = rayDirection;
                return cam;
            }

            float random(float2 co) {
                float dt= dot(co.xy ,float2(12.9898, 78.233));
                float sn= fmod(dt, 3.14);
                return frac(sin(sn) * 43758.5453);
            }

            float fogFactorExp2(const float dist, const float density) {
                const float LOG2 = -1.442695;
                float d = density * dist;
                return 1.0 - clamp(exp2(d * d * LOG2), 0.0, 1.0);
            }

            float intersectPlane(float3 ro, float3 rd, float3 nor, float dist) {
                float denom = dot(rd, nor);
                float t = -(dot(ro, nor) + dist) / denom;
                return t;
            }

            float3 palette(in float t, in float3 a, in float3 b, in float3 c, in float3 d) {
                return a + b*cos(6.28318*(c*t+d));
            }

            float3 bg(float3 ro, float3 rd) {
                float iTime = _Time.y;
                
                // --- MODIFIED: Invert rd.y for palette when grid is on top ---
                // The original code uses rd.y to control the gradient. If the plane is flipped,
                // the sky gradient also needs to be effectively inverted based on the new "up" direction.
                float inverted_rd_y = -rd.y; 

                float3 col = 0.1 + (palette(
                    clamp((random(rd.xz + sin(iTime * 0.1)) * 0.5 + 0.5) * 0.035 - inverted_rd_y * 0.5 + 0.35, -1.0, 1.0), // Use inverted_rd_y here
                    _SkyGradientFactor1.rgb,  // Customizable
                    _SkyGradientFactor2.rgb,  // Customizable
                    float3(1.05, 1.0, 1.0),
                    float3(0.275, 0.2, 0.19)
                ));
                
                float t = intersectPlane(ro, rd, float3(0, -1, 0), 4.0); // Grid plane is still pointing down (visually on top)

                if (t > 0.0) {
                    float3 p = ro + rd * t;
                    float g = (1.0 - pow(abs(sin(p.x) * cos(p.z)), 0.25));
                    col += (1.0 - fogFactorExp2(t, 0.04)) * g * _GridGlowColor.rgb * 0.075; // Customizable grid glow
                }
                return col;
            }

            // ------------------------------------------------------------------
            // Main Vertex and Fragment Shaders
            // ------------------------------------------------------------------

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float iTime = _Time.y;
                float2 iResolution = _ScreenParams.xy;
                float2 fragCoord = i.vertex.xy;

                float3 ro, rd;
                float2 uv = squareFrame(iResolution, fragCoord);
                float dist = 4.5;
                
                float rotation = iTime * _RotationSpeed;
                float height = sin(iTime * _HeightSpeed) * _HeightAmplitude;
                
                CameraVectors cam = orbitCamera(rotation, height, dist, iResolution, fragCoord);
                ro = cam.origin;
                rd = cam.direction;
                
                float3 color = bg(ro, rd);

                float vignette = 1.0 - max(0.0, dot(uv * 0.155, uv));
                color.r = smoothstep(0.05, 0.995, color.r);
                color.b = smoothstep(-0.05, 0.95, color.b);
                color.g = smoothstep(-0.1, 0.95, color.g);
                color.b *= vignette;
                
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}