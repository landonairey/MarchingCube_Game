Shader "Custom/TerrainShader"
{
    //Following How To Make 7 Days to Die in Unity - 03
    //https://www.youtube.com/watch?v=Y2qS-c67NzU

    Properties //properties that show up in the inspector when you click on this shader
    {
        _MainTex ("Texture", 2D) ="white" {}
        _WallTex("WallTexture", 2D) = "white" {}
        _TexScale ("Texture Scale", Float) = 1
    }

    SubShader 
    {
        Tags {"RenderType" = "Opaque"}
        LOD 200 //100 is lowest and 500 is highest, check the documentation

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0 //lower target means more features but lower compatibility

        sampler2D _MainTex;
        sampler2D _WallTex;
        float _TexScale;

        struct Input //sets up data we want
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 scaledWorldPos = IN.worldPos / _TexScale;
            float3 pWeight = abs(IN.worldNormal);
            pWeight /= pWeight.x + pWeight.y + pWeight.z; //adjust the weight by each direction

            float3 xP = tex2D (_WallTex, scaledWorldPos.yz) * pWeight.x; //xP means X Projection, blend it by the x value
            float3 yP = tex2D (_MainTex, scaledWorldPos.xz) * pWeight.y;
            float3 zP = tex2D (_WallTex, scaledWorldPos.xy) * pWeight.z;

            //o.Albedo = tex2D(_MainTex, scaledWorldPos.xz); //basic setup

            o.Albedo = xP + yP + zP;
        }

        ENDCG
    }

    Fallback "Diffuse"
}
