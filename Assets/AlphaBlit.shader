Shader "Hidden/AlphaBlit"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 frag (v2f i) : SV_Target
            {
                float depth = Linear01Depth(SampleSceneDepth(i.uv), _ZBufferParams);
                float4 col = tex2D(_MainTex, i.uv);

                // Hole-filling extravaganza!
                if(depth >= 1.f && col.a < 1.f)
                {
                    int n = 0;
                    float3 sum = (float3)0.f;
                    for(int y = -1; y <= 1; y++)
                    {
                        for(int x = -1; x <= 1; x++)
                        {
                            float ux = _MainTex_TexelSize.x * x;
                            float uy = _MainTex_TexelSize.y * y;
                            float4 tmp = tex2D(_MainTex, i.uv + float2(ux, uy));
                            if(tmp.a >= 1.f)
                            {
                                n++;
                                int i = abs(x) + abs(y);
                                sum += tmp.rgb;
                            }
                        }
                    }
                    col = float4(sum / n, 1.f);
                }
                return col;
            }
            ENDHLSL
        }
    }
}
