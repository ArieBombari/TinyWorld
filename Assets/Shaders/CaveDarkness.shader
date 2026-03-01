Shader "UI/CaveDarkness"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Darkness ("Darkness", Range(0,1)) = 0
        _LanternScreenPos ("Lantern Screen Pos", Vector) = (0.5, 0.5, 0, 0)
        _LanternRadius ("Lantern Radius", Range(0,1)) = 0.15
        _LanternSoftness ("Lantern Softness", Range(0.01,0.5)) = 0.1
        _LanternActive ("Lantern Active", Float) = 0
        _CircleMinAlpha ("Circle Min Alpha", Range(0,1)) = 0

        // Required for UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 screenPos : TEXCOORD0;
            };

            fixed4 _Color;
            float _Darkness;
            float4 _LanternScreenPos;
            float _LanternRadius;
            float _LanternSoftness;
            float _LanternActive;
            float _CircleMinAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Get screen position in 0-1 range
                float4 screenPos = ComputeScreenPos(o.vertex);
                o.screenPos = screenPos.xy / screenPos.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                col.a = _Darkness;

                // If lantern is active, cut a circular hole
                if (_LanternActive > 0.5)
                {
                    // Correct for aspect ratio
                    float2 screenUV = i.screenPos;
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    float2 lanternUV = _LanternScreenPos.xy;

                    float2 diff = screenUV - lanternUV;
                    diff.x *= aspect;

                    float dist = length(diff);

                    // Smooth circle: _CircleMinAlpha at center, fully dark at edge
                    float circle = smoothstep(_LanternRadius, _LanternRadius + _LanternSoftness, dist);

                    col.a = lerp(max(col.a * _CircleMinAlpha, _CircleMinAlpha), col.a, circle);
                }

                return col;
            }
            ENDCG
        }
    }
}
