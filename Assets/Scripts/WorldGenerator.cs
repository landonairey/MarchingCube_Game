using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks = 10;
    public GameObject loadingScreen;
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    void Start()
    {
        //Chunk chunk = new Chunk(Vector3Int.zero); //test making one chunk
        Generate();
    }

    void Generate ()
    {
        loadingScreen.SetActive(true);
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * GameData.chunkWidth, 0, z * GameData.chunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos));
                chunks[chunkPos].chunkObject.transform.SetParent(transform); //put chunks under transform of the World Generator object
            }
        }

        loadingScreen.SetActive(false);
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * GameData.chunkWidth)));
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];
    }

    // MOVING FROM CHUNK SCRIPT TO HERE
    // scan through all xyz points and flag ones that are within the EditSphere volume
    // takes in the EditSphere position and diameter
    // returns a list of vertices
    public List<Vector3Int> FindEditVertices(Vector3 pos, float editSphereDiameter)
    {
        //Edit Sphere radius
        float editSphereRadius = editSphereDiameter / 2;

        //Initialize empty list
        List<Vector3Int> EditVerticesGroup = new List<Vector3Int>();

        //for (int x = 0; x < width; x++) // search all voxel points
        //clamp to xyz range
        for (int x = Mathf.Clamp(Mathf.CeilToInt(pos.x - editSphereRadius), 0, WorldSizeInChunks * GameData.chunkWidth + 1); x < Mathf.Clamp(Mathf.FloorToInt(pos.x + editSphereRadius) + 1, 0, WorldSizeInChunks * GameData.chunkWidth + 1); x++) //bound grid search by the sphere position and diameter
        {
            for (int y = Mathf.Clamp(Mathf.CeilToInt(pos.y - editSphereRadius), 0, GameData.chunkHeight + 1); y < Mathf.Clamp(Mathf.FloorToInt(pos.y + editSphereRadius) + 1, 0, GameData.chunkHeight + 1); y++)
            {
                for (int z = Mathf.Clamp(Mathf.CeilToInt(pos.z - editSphereRadius), 0, WorldSizeInChunks * GameData.chunkWidth + 1); z < Mathf.Clamp(Mathf.FloorToInt(pos.z + editSphereRadius) + 1, 0, WorldSizeInChunks * GameData.chunkWidth + 1); z++)
                {
                    if (Vector3.Distance(new Vector3Int(x, y, z), pos) < editSphereDiameter / 2)
                    {
                        EditVerticesGroup.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        return EditVerticesGroup;
    }
}
