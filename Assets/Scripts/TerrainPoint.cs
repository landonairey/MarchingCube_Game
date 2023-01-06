﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPoint
{
    // The distance from the surface, we're setting to 1 by default because that is outside
    // the surface. In other words, air.
    public float dstToSurface = 1f;

    // The ID of the texture this point will be located at.
    public int textureID = 0;

    //constructor which forces you to include these inputs when you generate a terrain point
    public TerrainPoint(float dst, int tex)
    {
        dstToSurface = dst;
        textureID = tex;
    }
}
