Shader "Hidden/EditorPreview"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Zoom ("Zoom", Float) = 1.0
        _Slice ("Slice", Float) = 0.0
        _Channels ("Channels", Vector) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler3D _MainTex;
            float _Zoom;
            float _Slice;
            float4 _Channels;

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv = i.uv * _Zoom - (_Zoom - 1.0) / 2.0;
                //float2 bord = frac(i.uv);
                //if(bord.x < 0.005 || bord.y < 0.005) return fixed4(0.5,0.8,0.5,1);

                float3 uv = float3(_Slice, i.uv.xy);
                fixed4 col = tex3D(_MainTex, uv);
                col *= _Channels;
                if(_Channels.a > 0.0)
                {
                    col = (fixed4)col.a;
                }
                return col;
            }
            ENDCG
        }
    }
}
