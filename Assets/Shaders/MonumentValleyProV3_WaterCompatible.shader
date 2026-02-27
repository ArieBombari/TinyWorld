Shader "Custom/MonumentValleyProV3_WaterCompatible"
{
    Properties
    {
        [Header(Y Axis Colors Top Bottom)]
        _TopColorStart ("Top Gradient Start", Color) = (0.93, 0.86, 0.80, 1)
        _TopColorEnd ("Top Gradient End", Color) = (0.85, 0.75, 0.68, 1)
        [Toggle] _UseTopGradient ("Use Top Gradient", Float) = 0
        _TopGradientHeight ("Top Gradient Height", Range(0.1, 10)) = 2.0
        
        [Space(10)]
        [Header(X Axis Colors Left Right)]
        _SideXColorStart ("Side X Gradient Start", Color) = (0.85, 0.69, 0.56, 1)
        _SideXColorEnd ("Side X Gradient End", Color) = (0.75, 0.60, 0.48, 1)
        [Toggle] _UseSideXGradient ("Use Side X Gradient", Float) = 0
        _SideXGradientHeight ("Side X Gradient Height", Range(0.1, 10)) = 2.0
        
        [Space(10)]
        [Header(Z Axis Colors Front Back)]
        _SideZColorStart ("Side Z Gradient Start", Color) = (0.73, 0.62, 0.58, 1)
        _SideZColorEnd ("Side Z Gradient End", Color) = (0.65, 0.54, 0.50, 1)
        [Toggle] _UseSideZGradient ("Use Side Z Gradient", Float) = 0
        _SideZGradientHeight ("Side Z Gradient Height", Range(0.1, 10)) = 2.0
        
        [Space(10)]
        [Header(Blending and Lighting)]
        _BlendSharpness ("Blend Sharpness", Range(1, 32)) = 12
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.45
        _DirectLightStrength ("Direct Light Strength", Range(0, 2)) = 0.35
        _ShadowStrength ("Shadow Darkness", Range(0, 1)) = 0.7
        
        [Space(10)]
        [Header(Textures and Materials)]
        _MainTex ("Albedo Texture (Optional)", 2D) = "white" {}
        _TextureScale ("Texture Scale", Float) = 1.0
        _TextureBlend ("Texture Blend", Range(0, 1)) = 0.0
        
        [Space(5)]
        _BumpMap ("Normal Map (Optional)", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0
        
        [Space(5)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _TopColorStart;
                half4 _TopColorEnd;
                half _UseTopGradient;
                half _TopGradientHeight;
                
                half4 _SideXColorStart;
                half4 _SideXColorEnd;
                half _UseSideXGradient;
                half _SideXGradientHeight;
                
                half4 _SideZColorStart;
                half4 _SideZColorEnd;
                half _UseSideZGradient;
                half _SideZGradientHeight;
                
                half _BlendSharpness;
                half _AmbientStrength;
                half _DirectLightStrength;
                half _ShadowStrength;
                
                float4 _MainTex_ST;
                float _TextureScale;
                half _TextureBlend;
                half _BumpScale;
                half _Smoothness;
                half _Metallic;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
                
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 worldNormal = normalize(input.normalWS);
                
                // Apply normal map if present
                if (_BumpScale > 0.01)
                {
                    float3 scaledPos = input.positionWS * _TextureScale;
                    half3 tangentNormalX = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, scaledPos.zy), _BumpScale);
                    half3 tangentNormalY = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, scaledPos.xz), _BumpScale);
                    half3 tangentNormalZ = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, scaledPos.xy), _BumpScale);
                    
                    half3 absNormal = abs(worldNormal);
                    half3 blendWeights = pow(absNormal, _BlendSharpness);
                    blendWeights /= max(blendWeights.x + blendWeights.y + blendWeights.z, 0.001);
                    
                    half3 triplanarNormal = tangentNormalX * blendWeights.x +
                                           tangentNormalY * blendWeights.y +
                                           tangentNormalZ * blendWeights.z;
                    
                    float3 bitangentWS = cross(worldNormal, input.tangentWS.xyz) * input.tangentWS.w;
                    worldNormal = normalize(triplanarNormal.x * input.tangentWS.xyz + 
                                          triplanarNormal.y * bitangentWS + 
                                          triplanarNormal.z * worldNormal);
                }
                
                // Calculate blend weights
                half3 absNormal = abs(worldNormal);
                absNormal = max(absNormal, half3(0.001, 0.001, 0.001));
                half3 blendWeights = pow(absNormal, _BlendSharpness);
                half sumWeights = blendWeights.x + blendWeights.y + blendWeights.z;
                blendWeights = blendWeights / max(sumWeights, 0.001);
                blendWeights = saturate(blendWeights);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z);
                
                // Calculate gradients
                half yPos = input.positionWS.y;
                
                half topGradient = saturate(yPos / _TopGradientHeight);
                half3 topColor = lerp(_TopColorStart.rgb, _TopColorEnd.rgb, _UseTopGradient * topGradient);
                
                half sideXGradient = saturate(yPos / _SideXGradientHeight);
                half3 sideXColor = lerp(_SideXColorStart.rgb, _SideXColorEnd.rgb, _UseSideXGradient * sideXGradient);
                
                half sideZGradient = saturate(yPos / _SideZGradientHeight);
                half3 sideZColor = lerp(_SideZColorStart.rgb, _SideZColorEnd.rgb, _UseSideZGradient * sideZGradient);
                
                // Blend colors
                half3 baseColor = topColor * blendWeights.y + 
                                 sideXColor * blendWeights.x + 
                                 sideZColor * blendWeights.z;
                baseColor = saturate(baseColor);
                
                // Apply texture
                if (_TextureBlend > 0.01)
                {
                    float3 scaledPos = input.positionWS * _TextureScale;
                    half4 xTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scaledPos.zy);
                    half4 yTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scaledPos.xz);
                    half4 zTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scaledPos.xy);
                    half3 triplanarTex = xTex.rgb * blendWeights.x + yTex.rgb * blendWeights.y + zTex.rgb * blendWeights.z;
                    baseColor = lerp(baseColor, baseColor * triplanarTex, _TextureBlend);
                    baseColor = saturate(baseColor);
                }
                
                // Lighting
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                half NdotL = saturate(dot(worldNormal, mainLight.direction));
                half shadowAttenuation = mainLight.shadowAttenuation;
                
                half ambient = _AmbientStrength;
                half direct = NdotL * _DirectLightStrength;
                direct *= lerp(1.0, shadowAttenuation, _ShadowStrength);
                
                half totalLight = saturate(ambient + direct);
                half3 finalColor = baseColor * totalLight;
                finalColor = saturate(finalColor);
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        // CRITICAL: Explicit DepthOnly pass for water shaders
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
        
        // DepthNormals pass for SSAO and other effects
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 DepthNormalsFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return half4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
            }
            ENDHLSL
        }
        
        // Shadow caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _LightDirection;
            float3 _LightPosition;

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }

            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
