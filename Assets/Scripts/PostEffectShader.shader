Shader "Custom/PostEffectShader"
{
    //FROM Unity Tutorial: A Practical Intro to Shaders - Part 1
    //quill18creates
    //https://youtu.be/C0uJ4sZelio

    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
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

            //runs once per vertex
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            //runs once per pixel
            //Unity stores UVs in 0-1 space. [0,0] represents the bottom-left corner of the texture, and [1,1] represents the top-right. 
            //Values are not clamped; you can use values below 0 and above 1 if needed.
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv + float2(sin(i.vertex.y/50 + _Time[1])/50, sin(i.vertex.x/50 + _Time[1])/50));
                // just invert the colors
                //col.rgb = 1 - col.rgb;

                //col.r = 1;

                return col;
            }
            ENDCG
        }
    }
}
