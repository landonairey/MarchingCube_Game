Shader "Custom/TerrainShader"
{
    //Following How To Make 7 Days to Die in Unity - 03
    //https://www.youtube.com/watch?v=Y2qS-c67NzU

    Properties //properties that show up in the inspector when you click on this shader
    {
        _TexArr("Textures", 2DArray) = "" {}

        _MainTex ("Texture", 2D) ="white" {}
        _WallTex("WallTexture", 2D) = "white" {}
        _TexScale ("Texture Scale", Float) = 1
    }

    SubShader 
    {
        Tags {"RenderType" = "Opaque"}
        LOD 200 //100 is lowest and 500 is highest, check the documentation

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows // Use Unity's standard lighting model
        #pragma target 3.5 //lower target means more features but lower compatibility, Texture2DArrays not supported below 3.5
        #pragma require 2darray

        sampler2D _MainTex;
        sampler2D _WallTex;
        UNITY_DECLARE_TEX2DARRAY(_TexArr);
        float _TexScale;

        struct Input //sets up data we want
        {
            float3 worldPos;
            float3 worldNormal;
            float4 color : COLOR;
            float2 uv_TexArr;
        };

        //this function is run for every pixel on the screen
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 scaledWorldPos = IN.worldPos / _TexScale;
            float3 pWeight = abs(IN.worldNormal);
            pWeight /= pWeight.x + pWeight.y + pWeight.z; //adjust the weight by each direction

            int texIndex = floor(IN.uv_TexArr.x + 0.1); // Current index of our texture in the array.
            float3 projected; // float3 storing the current 2D UV coords + the index stored in the Z value.

            // Get the texture projection on each axes and "weight" it by multiplying it by the pWeight.
            projected = float3(scaledWorldPos.y, scaledWorldPos.z, texIndex);
			float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

			projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
			float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

			projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
			float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

            //o.Albedo = tex2D(_MainTex, scaledWorldPos.xz); //basic setup

            // Return the sum of all of the projections.
            o.Albedo = xP + yP + zP;
        }

        ENDCG
    }

    Fallback "Diffuse"
}
