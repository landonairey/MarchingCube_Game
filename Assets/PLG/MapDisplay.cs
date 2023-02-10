using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[z * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        //Allows it to be updated in real time and not just run time
        textureRender.sharedMaterial.mainTexture = texture;

        //set size of plane to the same size of the map
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
