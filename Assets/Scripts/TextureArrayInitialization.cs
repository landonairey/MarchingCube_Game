using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureArrayInitialization : MonoBehaviour
{
    //public static Object[] objects;
    [SerializeField] Texture2D[] textures;
    [SerializeField] bool createTexArr;

    // Start is called before the first frame update
    void Start()
    {


        //modified from https://forum.unity.com/threads/creating-texture2darray-as-asset.425461/
        /*
        //Object[] selection = Selection.objects;
        //Texture2D[] textures = new Texture2D[selection.Length];
        //for (int i = 0; i < textures.Length; i++)
        //{
        //    textures[i] = (Texture2D)selection[i];
        //}

        Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, false);
        for (int i = 0; i < textures.Length; i++)
        {
            Debug.Log(textures[i].GetPixels().GetType()); //textures[i].GetPixels() returns type Color
            array.SetPixels(textures[i].GetPixels(), 0);
        }
        array.Apply();
        AssetDatabase.CreateAsset(array, "Assets/TextureArray.tarr");
        */

        if (createTexArr)
            CreateTextureArray();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method from https://medium.com/@calebfaith/how-to-use-texture-arrays-in-unity-a830ae04c98b
    private void CreateTextureArray()
    {
        // Create Texture2DArray
        Texture2DArray texture2DArray = new
            Texture2DArray(textures[0].width,
            textures[0].height, textures.Length,
            TextureFormat.RGBA32, true, false);
        // Apply settings
        texture2DArray.filterMode = FilterMode.Bilinear;
        texture2DArray.wrapMode = TextureWrapMode.Repeat;
        // Loop through ordinary textures and copy pixels to the
        // Texture2DArray
        for (int i = 0; i < textures.Length; i++)
        {
            texture2DArray.SetPixels(textures[i].GetPixels(0),
                i, 0);
        }
        // Apply our changes
        texture2DArray.Apply();
        // Set the texture to a material
        //objectToAddTextureTo.GetComponent<Renderer>()
        //    .sharedMaterial.SetTexture("_MainTex", texture2DArray);

        //Save asset
#if UNITY_EDITOR //only use this code IF we are in the editor, othewise it will break when trying to build this project
        AssetDatabase.CreateAsset(texture2DArray, "Assets/Resources/Textures/TextureArray.asset");
#endif
    }
}
