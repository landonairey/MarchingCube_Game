Shader "Unlit/texturesplattingRGB_p3" //https://catlikecoding.com/unity/tutorials/rendering/part-3/
{
    Properties
    {
        _MainTex ("Splat Map", 2D) = "white" {}
        [NoScaleOffset] _Texture1 ("Texture 1", 2D) = "white" {} //No Scale Offset removes tiling and offset parameters
        [NoScaleOffset] _Texture2 ("Texture 2", 2D) = "white" {}
        [NoScaleOffset] _Texture3 ("Texture 3", 2D) = "white" {}
        [NoScaleOffset] _Texture4 ("Texture 4", 2D) = "white" {}
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
                float2 uvSplat : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; //clip space position
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Texture1, _Texture2, _Texture3, _Texture4;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvSplat = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the splatmap before sampling the other textures
                fixed4 splat = tex2D(_MainTex, i.uvSplat);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return
                    tex2D(_Texture1, i.uv) * splat.r +      //value of  1 in the r channel turns on Texture 1
                    tex2D(_Texture2, i.uv) * splat.g +      //value of  1 in the g channel turns on Texture 2
                    tex2D(_Texture3, i.uv) * splat.b +      //value of  1 in the b channel turns on Texture 3
                    tex2D(_Texture4, i.uv) * (1 - splat.r - splat.g - splat.b); //remainder turns on Texture 4
            }
            ENDCG
        }
    }
}
 