using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostEffectScript : MonoBehaviour
{
    public Material mat;

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        //src is the fully rendered scene that you would normally to the monitor
        //We are intercepting this so we can do some work before passing it on

        // PRETENDING TO DO IMAGE EFFECT IN CPU
        /*
        Color[] pixels = new Color[1920 * 1080]; //texture.GetPixels();

        for (int x = 0; x < 1920; x++)
        {
            for (int y = 0; y < 1080; y++)
            {
                pixels[x + y * 1080].r = Mathf.Pow(2.18f, 3.17f);
            }
        }
        */

        //probably apply some kind of texture.SetPixels(pixels);

        Graphics.Blit(src, dst, mat);
    }
}
