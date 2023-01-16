using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreCluster
{
    // 3D position of the cluster centroid, must be within Chunk dimensions
    public Vector3Int center;

    // Kernel size of the cluster, needs to be odd
    public int kernelSize;

    // Sigma value of the distribution, needs to be 1/7th of the kernel size
    public float sigma;

    // Ore Type, stores the Ore ID from GameData
    public int oreID;

    //constructor which forces you to include these inputs when you generate an ore cluster
    public OreCluster(Vector3Int ctr, int size, float s, int ID)
    {
        center = ctr;
        kernelSize = size;
        sigma = s;
        oreID = ID;
    }
}
