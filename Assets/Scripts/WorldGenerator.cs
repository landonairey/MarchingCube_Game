using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// World Coordinates:
//         +Z is NORTH
//      
// -X is WEST     +X is EAST
//
//         -Z is SOUTH

public class WorldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks = 10;
    public GameObject loadingScreen;
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    List<Vector3Int> EditCellsPositions = new List<Vector3Int>(); //list of cells affected in the edit methods
    List<Chunk> EditChunks = new List<Chunk>(); //list of Chunks affected in the edit methods

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

        //Debug.Log(string.Format("Lookup Chunk Position: {0}, {1}, {2} ", x, y, z));
        return chunks[new Vector3Int(x, y, z)];
    }

    public float HandleModifyTerrain(Vector3 pos, float editSphereDiameter, int editVal)
    {
        //initialize variables
        float deltaVol = 0;
        float startVolume = 0;
        float endVolume = 0;
        EditChunks = new List<Chunk>();
        EditCellsPositions = new List<Vector3Int>();

        // Find list of vertex locations that are within the Edit Sphere
        List<Vector3Int> EditVerticesPositions = FindEditVertices(pos, editSphereDiameter);

        //Create list of cells affected by the EditSphere
        FindEditCells(EditVerticesPositions);

        // Loop through all EditCellsPositions list
        for (int i = 0; i < EditCellsPositions.Count; i++)
        {
            //Get appropriate chunk
            float chunkX = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].x / GameData.chunkWidth); //round down to the nearest starting chunk coord
            float chunkZ = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].z / GameData.chunkWidth);
            Debug.Log(string.Format("Current Edit Chunk Position: {0}, {1}, {2} ", chunkX, 0, chunkZ));

            Chunk currentChunk = GetChunkFromVector3(new Vector3(chunkX, 0, chunkZ));
            //AddUniqueEditChunk(currentChunk);

            //Get the starting volume
            //Debug.Log(string.Format("Current Edit Cell Position: {0}, {1}, {2} ", EditCellsPositions[i].x, EditCellsPositions[i].y, EditCellsPositions[i].z));
            startVolume = currentChunk.TetraCellVolume(EditCellsPositions[i]);
        }

        // Loop through all EditVerticesPositions list
        for (int i = 0; i < EditVerticesPositions.Count; i++)
        {
            int remainderX = EditVerticesPositions[i].x % GameData.chunkWidth; //Modulo check of W/E direction
            int remainderZ = EditVerticesPositions[i].z % GameData.chunkWidth; //Modulo check of N/S direction

            //Vertex on Shared Chunk Corner
            if (remainderX == 0 & remainderZ == 0)
            {
                ModifyVertexOnBorder(EditVerticesPositions[i], -1,  0, editVal); //Look North West
                ModifyVertexOnBorder(EditVerticesPositions[i],  0,  0, editVal); //Look North East
                ModifyVertexOnBorder(EditVerticesPositions[i],  0, -1, editVal); //Look South East
                ModifyVertexOnBorder(EditVerticesPositions[i], -1, -1, editVal); //Look South West
            }

            //Vertex on Shared N/S Chunk Border
            else if (remainderZ == 0)
            {
                ModifyVertexOnBorder(EditVerticesPositions[i], 0,  0, editVal); //Look North
                ModifyVertexOnBorder(EditVerticesPositions[i], 0, -1, editVal); //Look South
            }

            //Vertex on Shared W/E Chunk Border
            else if (remainderX == 0)
            {
                ModifyVertexOnBorder(EditVerticesPositions[i], -1, 0, editVal); //Look West
                ModifyVertexOnBorder(EditVerticesPositions[i],  0, 0, editVal); //Look East
            }

            //Vertex within single Chunk
            else
            {
                //Get appropriate chunk
                float chunkX = GameData.chunkWidth * Mathf.Floor(EditVerticesPositions[i].x / GameData.chunkWidth); //round down to the nearest starting chunk coord
                float chunkZ = GameData.chunkWidth * Mathf.Floor(EditVerticesPositions[i].z / GameData.chunkWidth);
                Chunk currentChunk = GetChunkFromVector3(new Vector3(chunkX, 0, chunkZ));

                //Add to list
                AddUniqueEditChunk(currentChunk);

                //modify TerrainMap
                if (editVal == 0)
                {
                    currentChunk.PlaceTerrain(EditVerticesPositions[i]);
                }
                else if (editVal == 1)
                {
                    currentChunk.RemoveTerrain(EditVerticesPositions[i]);
                }
                else
                {
                    Debug.Log("ERROR: Expected editVal to be 0 or 1");
                }
            }

        }

        // Loop through all EditCellsPositions list
        for (int i = 0; i < EditCellsPositions.Count; i++)
        {
            //Get appropriate chunk
            float chunkX = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].x / GameData.chunkWidth); //round down to the nearest starting chunk coord
            float chunkZ = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].z / GameData.chunkWidth);
            Chunk currentChunk = GetChunkFromVector3(new Vector3(chunkX, 0, chunkZ));

            //Get the new volume
            endVolume = currentChunk.TetraCellVolume(EditCellsPositions[i]);

            //change in volume
            deltaVol = deltaVol + endVolume - startVolume;
        }

        // Loop through all affected Chunks and generate update their mesh
        for (int i = 0; i < EditChunks.Count; i++)
        {
            //Update terrain mesh
            EditChunks[i].Update();
        }

        /*
        // Loop through all EditCellsPositions list
        for (int i = 0; i < EditCellsPositions.Count; i++)
        {
            //Get appropriate chunk
            float chunkX = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].x / GameData.chunkWidth); //round down to the nearest starting chunk coord
            float chunkZ = GameData.chunkWidth * Mathf.Floor(EditCellsPositions[i].z / GameData.chunkWidth);
            Debug.Log(string.Format("Current Edit Chunk Position: {0}, {1}, {2} ", chunkX, 0, chunkZ));

            Chunk currentChunk = GetChunkFromVector3(new Vector3(chunkX, 0, chunkZ));
            AddUniqueEditChunk(currentChunk);

            //Get the starting volume
            //Debug.Log(string.Format("Current Edit Cell Position: {0}, {1}, {2} ", EditCellsPositions[i].x, EditCellsPositions[i].y, EditCellsPositions[i].z));
            float startVolume = currentChunk.TetraCellVolume(EditCellsPositions[i]);

            //modify TerrainMap to write 0's for placing terrain
            currentChunk.PlaceTerrain(EditCellsPositions[i]);

            //MarchCube to update mesh
            //currentChunk.MarchCube(EditCellsPositions[i]);

            //Get the new volume
            float endVolume = currentChunk.TetraCellVolume(EditCellsPositions[i]);

            //change in volume
            deltaVol = deltaVol + endVolume - startVolume;
        }

        // Loop through all affected Chunks and generate update their mesh
        for (int i = 0; i < EditChunks.Count; i++)
        {
            EditChunks[i].Update();
        }
        */

        return deltaVol;

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

    // MOVING FROM CHUNK SCRIPT TO HERE
    // Find list of vertex locations that are within the Edit Sphere
    //Takes in a list of Vertices that are within the EditSphere volume from the FindEditVertices method
     //Returns a list of Vertices that define the origin position for each cell affected by the EditSphere
    public void FindEditCells(List<Vector3Int> EditVerticesPositions)
    {
        //Preload Cell Positions will all of the Vertex Positions
        //EditCellsPositions = EditVerticesPositions;

        int vertIndex = 0;

        //Loop through all of the Vertex locations within the EditSphere
        foreach (Vector3Int vertLocation in EditVerticesPositions)
        {
            //Debug.Log("vertIndex: " + vertIndex);
            vertIndex = vertIndex + 1;
            //Find 8 cells that share the same vertex and add them to the EditCellsPositions list if they aren't there already
            Vector3Int Neighbor_0 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y - 1, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z - 1, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_1 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y - 1, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z - 1, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_2 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y - 1, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_3 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y - 1, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_4 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z - 1, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_5 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z - 1, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_6 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z, 0, WorldSizeInChunks * GameData.chunkWidth));
            Vector3Int Neighbor_7 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, WorldSizeInChunks * GameData.chunkWidth), Mathf.Clamp(vertLocation.y, 0, GameData.chunkHeight - 1), Mathf.Clamp(vertLocation.z, 0, WorldSizeInChunks * GameData.chunkWidth));
            //Debug.Log(string.Format("Neighbor_0: {0:0.}, {1:0.}, {2:0.}", Neighbor_0.x, Neighbor_0.y, Neighbor_0.z));

            AddUniqueEditCellPosition(Neighbor_0);
            AddUniqueEditCellPosition(Neighbor_1);
            AddUniqueEditCellPosition(Neighbor_2);
            AddUniqueEditCellPosition(Neighbor_3);
            AddUniqueEditCellPosition(Neighbor_4);
            AddUniqueEditCellPosition(Neighbor_5);
            AddUniqueEditCellPosition(Neighbor_6);
            AddUniqueEditCellPosition(Neighbor_7);
        }
    }

    // MOVING FROM CHUNK SCRIPT TO HERE
    private void AddUniqueEditCellPosition(Vector3Int neighborCell)
    {
        // Loop through all vertices currently in the EditCellsPositions list
        for (int i = 0; i < EditCellsPositions.Count; i++)
        {
            // If we find a cell position that matches our, then don't add it to the list
            if (EditCellsPositions[i] == neighborCell)
            {
                //do nothing
                //Debug.Log("Vert Matches, Do Nothing");
                return;
            }
        }
        // If we did NOT find a cell position that matches our, then don't add it to the list
        //add to list
        EditCellsPositions.Add(neighborCell);
        return;
    }

    private void AddUniqueEditChunk(Chunk currentChunk)
    {
        // Loop through all Chunks currently in the EditChunks list
        for (int i = 0; i < EditChunks.Count; i++)
        {
            // If we find a Chunk that matches our currentChunk, then don't add it to the list
            if (EditChunks[i] == currentChunk)
            {
                //do nothing
                //Debug.Log("Vert Matches, Do Nothing");
                return;
            }
        }
        // If we did NOT find a cell position that matches our, then don't add it to the list
        //add to list
        EditChunks.Add(currentChunk);
        return;
    }

    //Helper function to take in the Vector3Int position of a terrain vertex that's on a Chunk border
    //Also input the number of chunks to shift the vertex, i.e. -1 xChunkShift to "look" West
    private void ModifyVertexOnBorder(Vector3Int pos, int xChunkShift, int zChunkShift, int editVal)
    {
        float chunkX = GameData.chunkWidth * Mathf.Floor((pos.x + xChunkShift * GameData.chunkWidth) / GameData.chunkWidth); //round down to the nearest starting chunk coord
        float chunkZ = GameData.chunkWidth * Mathf.Floor((pos.z + zChunkShift * GameData.chunkWidth) / GameData.chunkWidth);

        if (chunkX >= 0 & chunkX <= WorldSizeInChunks * GameData.chunkWidth & chunkZ >= 0 & chunkZ <= WorldSizeInChunks * GameData.chunkWidth) //Check if within world
        {
            Chunk chunk = GetChunkFromVector3(new Vector3(chunkX, 0, chunkZ));
            AddUniqueEditChunk(chunk);

            //modify TerrainMap
            if (editVal == 0)
            {
                chunk.PlaceTerrain(pos);
            }
            else if (editVal == 1)
            {
                chunk.RemoveTerrain(pos);
            }
            else
            {
                Debug.Log("ERROR: Expected editVal to be 0 or 1");
            }
        }
    }
}
