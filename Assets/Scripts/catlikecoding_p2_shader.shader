Shader "Unlit/catlikecoding_p2_shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; //object-space position of the vertex
                float2 uv : TEXCOORD0; //2D image coordinates, horizontal coordinate as U and the vertical coordinate as V
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; //clip space position
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Tint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                //o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; //xy tiling vector used to scale the texture and zw offset vector, same thing as UnityCG.cginc macro "o.uv = TRANSFORM_TEX(v.uv, _MainTex);"

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * _Tint;
                //return float4(i.uv, 1, 1)+0.5 * _Tint;
            }
            ENDCG
        }
    }
}
 