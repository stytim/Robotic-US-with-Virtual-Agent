// Shader to make the curved surface of a cylinder pulse with color,
// while the top and bottom caps remain transparent.
// Allows seeing the back faces (Cull Off) and is XR friendly.
// Designed for Unity's Built-in Render Pipeline.

Shader "Custom/CylinderPulseSurface"
{
    Properties
    {
        _PulseColor ("Pulse Color", Color) = (1,0,0,1) // RGBA for the pulsing effect
        _PulseSpeed ("Pulse Speed", Float) = 5.0      // How fast the color pulses
        _PulseMinIntensity ("Pulse Min Intensity", Range(0,1)) = 0.2 // Minimum brightness of the pulse
        _PulseMaxIntensity ("Pulse Max Intensity", Range(0,1)) = 1.0 // Maximum brightness of the pulse
        _CapThreshold ("Cap Normal Y Threshold", Range(0.001, 0.5)) = 0.05 // How much of the normal's Y component identifies it as a cap. Adjust if edges are not clean.
        _MainTex ("Albedo (not used for pulse)", 2D) = "white" {} // Standard property, not directly used for pulsing logic but good practice
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
            ZWrite Off                     // Don't write to depth buffer for transparent objects (usually)
            Cull Off                       // Render both front and back faces. Changed from Cull Back.

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            // Enabling instancing for better performance, especially in XR
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            // Struct to pass data from vertex to fragment shader
            struct appdata
            {
                float4 vertex : POSITION;    // Vertex position in object space
                float3 normal : NORMAL;      // Vertex normal in object space
                float2 uv : TEXCOORD0;       // UV coordinates
                UNITY_VERTEX_INPUT_INSTANCE_ID // For GPU instancing
            };

            struct v2f
            {
                float4 vertex : SV_POSITION; // Vertex position in clip space
                float2 uv : TEXCOORD0;       // UV coordinates
                float3 normalOS : TEXCOORD1; // Vertex normal in object space (OS)
                UNITY_VERTEX_OUTPUT_STEREO // For stereo rendering in XR
            };

            // Properties exposed in the Material Inspector
            fixed4 _PulseColor;
            float _PulseSpeed;
            float _PulseMinIntensity;
            float _PulseMaxIntensity;
            float _CapThreshold;

            // Vertex Shader
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); // Setup instancing
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // Initialize stereo output

                // Transform vertex position from object space to clip space
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Pass UV coordinates
                o.uv = v.uv;
                // Pass object space normal (important for distinguishing caps from sides)
                o.normalOS = v.normal;
                return o;
            }

            // Fragment Shader
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // Setup stereo eye index for fragment shader if needed (often not for simple effects)
                fixed4 finalColor = fixed4(0,0,0,0); // Initialize to fully transparent black

                // Determine if this fragment is on a cap or the curved surface
                // For a standard Unity cylinder:
                // - Normals on the caps point mostly along the Y-axis (e.g., (0, 1, 0) or (0, -1, 0))
                // - Normals on the curved surface have a Y-component close to 0.
                if (abs(i.normalOS.y) > (1.0 - _CapThreshold))
                {
                    // This fragment is part of a cap, so make it transparent.
                    finalColor.a = 0.0;
                }
                else
                {
                    // This fragment is part of the curved surface.
                    // Calculate pulsing effect.
                    // sin() gives a value from -1 to 1. We map it to _PulseMinIntensity to _PulseMaxIntensity.
                    float pulseFactor = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5; // Ranges from 0 to 1
                    pulseFactor = lerp(_PulseMinIntensity, _PulseMaxIntensity, pulseFactor); // Map to min/max intensity

                    // Apply pulsing color
                    finalColor.rgb = _PulseColor.rgb * pulseFactor;
                    // Set alpha from the _PulseColor property (allows controlling opacity of the pulse)
                    finalColor.a = _PulseColor.a * pulseFactor; // Modulate alpha by pulse as well for a more "glowy" pulse
                                                                // If you want constant opacity for the sides, use: finalColor.a = _PulseColor.a;
                }

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit" // Fallback for older hardware or if shader fails
}
