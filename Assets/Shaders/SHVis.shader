Shader "Custom/SHVis"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off
        ZWrite On

        Pass
        {
            Name "ForwardBase"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 仅当您要访问片元着色器中的实例化属性时才需要。
            };

            float4 _Color;
           
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
           
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                return _Color;
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