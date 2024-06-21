Shader "Custom/SHVis"
{
    Properties
    {
        _Floor("Floor", float) = 0
        _Ceil("Ceil", float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Back
        ZWrite On

        Pass
        {
            Name "ForwardBase"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            #define PI 3.14159265358979323846

            uniform float _Floor;
            uniform float _Ceil;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4x4, _SHCoefficients)
            UNITY_INSTANCING_BUFFER_END(Props)
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 仅当您要访问片元着色器中的实例化属性时才需要。
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.vertex);
                return o;
            }
            
            void PrecomputeLegendre(float3 N, out float p_lm_values[9])
            {
                float x = N.x;
                float y = N.y;
                float z = N.z;
                
                // l = 0, m = 0
                p_lm_values[0] = 1.f / (2 * sqrt(PI));
                // l = 1, m = 0
                p_lm_values[1] = 1.f / 2 * sqrt(3 / PI) * z;
                // l = 1, m = 1
                p_lm_values[2] = 1.f / 2 * sqrt(3 / PI) * x;
                // l = 1, m = -1
                p_lm_values[3] = 1.f / 2 * sqrt(3 / PI) * y;
                // l = 2, m = 0
                p_lm_values[4] = 1.f / 4 * sqrt(5 / PI) * (2 * z * z - x * x - y * y);
                // l = 2, m = 1
                p_lm_values[5] = 1.f / 2 * sqrt(15 / PI) * (z * x);
                // l = 2, m = -1
                p_lm_values[6] = 1.f / 2 * sqrt(15 / PI) * (y * z);
                // l = 2, m = 2
                p_lm_values[7] = 1.f / 4 * sqrt(15 / PI) * (x * x - y * y);
                // l = 2, m = -2
                p_lm_values[8] = 1.f / 2 * sqrt(15 / PI) * (x * y);
            }
            
            float SampleSH9(float4x4 coef, float3 N)
            {
                float res = 0;
                float p_lm_values[9];
                PrecomputeLegendre(N, p_lm_values);

                for (int i = 0; i < 9; i++)
                {
                    res += p_lm_values[i] * coef[i / 4][i % 4];
                }

                return res;
            }

            float Remap(float x, float a, float b, float c, float d)
            {
                return (x - a) / (b - a) * (d - c) + c;
            }
           
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4x4 shCoef = UNITY_ACCESS_INSTANCED_PROP(Props, _SHCoefficients);
                
                float rawUncer = SampleSH9(shCoef, i.normalWS);
                float remapUncer = Remap(rawUncer, _Floor, _Ceil, 0, 1);
                
                return fixed4(remapUncer, remapUncer, remapUncer, 1);
                
            }
            ENDCG
        }

        Pass
        {
            Name "DepthForwardOnly" 
            Tags{ "LightMode" = "DepthForwardOnly" }

            ZWrite On
            Cull Front

            HLSLPROGRAM

            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            //enable GPU instancing support
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // Note: Only shader graph support Shadow Matte, so we do'nt need normal buffer here
            #pragma multi_compile_fragment _ WRITE_MSAA_DEPTH
            // Note we don't need to define WRITE_NORMAL_BUFFER
            // Note we don't need to define WRITE_DECAL_BUFFER

            #define SHADERPASS SHADERPASS_DEPTH_ONLY
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/UnlitProperties.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/ShaderPass/UnlitDepthPass.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/UnlitData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }
}