using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//How to Make 7 Days to Die in Unity - 01 - Marching Cubes by b3agz
//https://github.com/b3agz/how-to-make-7-days-to-die-in-unity

//LEFT OFF:
//https://youtu.be/PgZDp5Oih38?t=868

public class Chunk
{
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();

	public GameObject chunkObject;
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	MeshRenderer meshRenderer;
	Vector3Int chunkPosition;

	int width { get { return GameData.chunkWidth; } }
	int height { get { return GameData.chunkHeight; } }
	float terrainSurface { get { return GameData.terrainSurface; } }
	TerrainPoint[,,] terrainMap; //3D storage for terrain values that we will sample when generating the mesh of type TerrainPoint


	//Gaussian Distribution Kernel
	float[,,] data3d_Au = GaussTable_3D(GameData.kernelSize_Au, GameData.kernelSigma_Au);

	//Number of different elements/minerals including dirt/soil
	int Au_ID = 0;
	int Dirt_ID = 1;

	float[,,,] oreMap; //Ore Distribution Map containing the values of generation lieklihood of any given ore at any given voxel location
	int seed = 2; //seed for random number generator in Ore distribution

	int _configIndex = 0; //debug stepping through marching cube configurations see Update()

	public bool smoothTerrain = false; //true for using linearly interpolated mesh positions or false for not for the marching cubes
	public bool flatShaded = true; //true for using duplicate vertices or false for shared vertices which blends shading

	List<Vector3Int> EditCellsPositions = new List<Vector3Int>(); //list of cells affected in the edit methods

	bool DEBUG_MARCHING_EN = false; //enable debug marching mode which removes the auto regeneration within the update loop and let's you cycle through cases

	public Chunk (Vector3Int _position)
    {
		chunkObject = new GameObject();
		chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
		chunkPosition = _position;
		chunkObject.transform.position = chunkPosition;
		chunkObject.layer = LayerMask.NameToLayer("Ground");

		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshCollider = chunkObject.AddComponent<MeshCollider>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
		meshRenderer.material.SetTexture("_TexArr", World.Instance.terrainTexArray);

		chunkObject.transform.tag = "Terrain";
		terrainMap = new TerrainPoint[width + 1, height + 1, width + 1]; //needs to be plus one or you'll get an index out of range error
		oreMap = new float[width + 1, height + 1, width + 1, GameData.numElements];

		if (!DEBUG_MARCHING_EN)
		{
			PopulateTerrainMap(); //COMMENT WHEN USING DEBUG_MARCHING
			OreDistribution();
		}
		CreateMeshData();
		BuildMesh();
	}

    public void Update()
    {
		// Uncomment these if you want to change the width, height, terrainSurface at run time 
		//COMMENT WHEN USING DEBUG_MARCHING
		if (!DEBUG_MARCHING_EN)
		{
			ClearMeshData();
			CreateMeshData();
			BuildMesh();
		}

		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = meshFilter.mesh;


		if (DEBUG_MARCHING_EN)
		{
			//UNCOMMENT FOR USING DEBUG_MARCHING
			if (Input.GetKeyDown(KeyCode.Period))
			{
				ClearMeshData();
				//MarchCube_debug(Vector3.zero, _configIndex); //used before
				PopulateTerrainMap_debug(_configIndex); //use method
				MarchCube(Vector3Int.zero);
				BuildMesh();
				Debug.Log(_configIndex);
				
				float cellVolume = TetraCellVolume(Vector3Int.zero);
				Debug.Log("Cell Volume :" + cellVolume);

				_configIndex++;

			}


			//UNCOMMENT FOR USING MARCHING_VOLUME
			if (Input.GetKeyDown(KeyCode.Comma))
			{
				//New Approach:
				ClearMeshData();
				terrainMap[1, 0, 0].dstToSurface = 1; //define cube corner values manually
				MarchCube(Vector3Int.zero);
				BuildMesh();

				float cellVolume = TetraCellVolume(Vector3Int.zero);
				Debug.Log("Cell Volume :" + cellVolume);
			}
		}

	}

	//Return volume of terrain cell given the case, vertex locations of the mesh, and above/below terrain flag
	public float CalculateMarchCellVolume(int cellCase, Vector3[] CoordsOfVertices, bool aboveTerrain)
    {
		float cellVolume = 0f;

		switch (cellCase)
        {
			case 0:
				Debug.Log("Case 0");
				if (aboveTerrain)
                {
					cellVolume = 0; //empty vertice list above terrain is just air, no terrain volume to account for
				}
				else
                {
					cellVolume = 1; //empty vertice list below terrain is burried, return full cell volume 
				}
				break;

			case 1:
				Debug.Log("Case 1");

				Vector3 a = new Vector3(0f, 0f, 0f);
				Vector3 b = CoordsOfVertices[0];
				Vector3 c = CoordsOfVertices[2];
				Vector3 d = CoordsOfVertices[1];
				cellVolume = CalculateTetrahedronVolume(a, b, c, d);

				if (aboveTerrain)
				{
					//do nothing //total volume is just the corner tetrahedron
				}
				else
				{
					cellVolume = 1 - cellVolume; //the cube cell is all terrain except the corner tetrahedron is air
				}

				break;
		}

		return cellVolume;
    }

	//Return volume of tetrahedron as defined from 4 unique points
	public float CalculateTetrahedronVolume(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
		//https://en.wikipedia.org/wiki/Tetrahedron#Volume
		//setup the inputs for the determinant
		var a = p0.x - p3.x;
		var b = p1.x - p3.x;
		var c = p2.x - p3.x;
		var d = p0.y - p3.y;
		var e = p1.y - p3.y;
		var f = p2.y - p3.y;
		var g = p0.z - p3.z;
		var h = p1.z - p3.z;
		var i = p2.z - p3.z;

		//calculate the determinant
		var DET = a*e*i + b*f*g + c*d*h - a*f*h - b*d*i - c*e*g;

		//volume of tetrahedron is 1/6th the volume of a parallelepiped
		var vt = Mathf.Abs(DET) / 6;

		return vt;
    }

	void PopulateTerrainMap()
    {
		float thisHeight;
		for (int x = 0; x < width + 1; x++)
        {
			for (int y = 0; y < height + 1; y++)
			{
				for (int z = 0; z < width + 1; z++)
				{


					//Using clamp to bound PerlinNoise as it intends to return a value 0.0f-1.0f but may sometimes be slightly out of that range
					//Multipying by height will return a value in the range of 0-height					
					//thisHeight = (float)height * Mathf.Clamp(Mathf.PerlinNoise((float)x / 16f * 1.5f, (float)z / 16f * 1.5f), 0.0f, 1.0f); //the 16f and 1.5f are made up coefficients
					thisHeight = GameData.GetTerrianHeight(x + chunkPosition.x, z + chunkPosition.z);

					/*
					if (x > 5 && x < 10 && z >5 && z < 10)
                    {
						terrainMap[x, y, z] = new TerrainPoint((float)y - thisHeight, 0);
					}
					else
                    {
						terrainMap[x, y, z] = new TerrainPoint((float)y - thisHeight, Random.Range(1, World.Instance.terrainTextures.Length));
					}
					*/
					
					//y points below thisHeight will be negative (below terrain) and y points above this Height will be positve and will render 
					terrainMap[x, y, z] = new TerrainPoint((float)y - thisHeight, Random.Range(0,World.Instance.terrainTextures.Length));
					


					//Debug
					//Debug.Log(thisHeight);
				}
			}
		}
    }

	void OreDistribution()
    {
		// Instantiate random number generator
		System.Random random = new System.Random(seed);

		//Center of cluster
		Vector3Int ore_center = new Vector3Int(16 - (GameData.kernelSize_Au - 1) / 2, 60 - (GameData.kernelSize_Au - 1), 16 - (GameData.kernelSize_Au - 1) / 2);

		//Load distribution values for each element:
		//Au distribution:
		for (int ix = 0; ix < data3d_Au.GetLength(0); ix++)
		{
			for (int iy = 0; iy < data3d_Au.GetLength(1); iy++)
			{
				for (int iz = 0; iz < data3d_Au.GetLength(2); iz++)
				{
					if (ix + ore_center.x < width && ore_center.y < height && iz + ore_center.z < width)
					{
						//write to OreMap if the location is within the bounds of the array
						oreMap[ix + ore_center.x, iy + ore_center.y, iz + ore_center.z, Au_ID] = data3d_Au[ix, iy, iz];
					}
				}
			}
		}
		float maxAu = data3d_Au[(GameData.kernelSize_Au - 1) / 2, (GameData.kernelSize_Au - 1) / 2, (GameData.kernelSize_Au - 1) / 2];

		//Dirt distribution:
		float dirtVal = 0.1f * Mathf.Max(maxAu); //include the other maxes here as well i.e. Mathf.Max(maxAu, maxAg, maxCu);
		for (int ix = 0; ix < oreMap.GetLength(0); ix++)
		{
			for (int iy = 0; iy < oreMap.GetLength(1); iy++)
			{
				for (int iz = 0; iz < oreMap.GetLength(2); iz++)
				{
					oreMap[ix, iy, iz, Dirt_ID] = dirtVal;
				}
			}
		}

		//Loop through the same for loop as the PopulateTerrainMap and write element ID texture values to the TerrainPoints within terrainMap
		for (int x = 0; x < width + 1; x++)
		{
			for (int y = 0; y < height + 1; y++)
			{
				for (int z = 0; z < width + 1; z++)
				{
					//Pick Element from Weighted Random Number Density Function

					float AuPick = oreMap[x, y, z, Au_ID];
					float DirtPick = oreMap[x, y, z, Dirt_ID];
					float normFactor = 1 / (AuPick + DirtPick);

					//weighted random number density function, these need to add up to 1
					double RATIO_CHANCE_Au = normFactor * AuPick;
					double RATIO_CHANCE_Dirt = normFactor * DirtPick;

					double r = random.NextDouble(); //uniform(0,1] random doubles

					if ((r -= RATIO_CHANCE_Au) < 0) // Test for A, a -= b is equivalent to a = a - b
					{
						//Debug.Log("pick Au");
						terrainMap[x, y, z].textureID = Au_ID;
					}
					else // No need for final if statement
					{
						//Debug.Log("pick Dirt");
						terrainMap[x, y, z].textureID = Dirt_ID;
					}
				}
			}
		}
	}

	static float[,,] GaussTable_3D(int size, float sigma)
	{
		float[,,] result = new float[size, size, size];
		float u = (size - 1) / 2f; // center point coordinates
		float sum = 0;

		for (int z = 0; z < size; z++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float temp1 = (u - x) * (u - x) + (u - y) * (u - y) + (u - z) * (u - z);
					float temp2 = 2 * sigma * sigma;
					float temp3 = Mathf.Sqrt(2 * Mathf.PI * sigma * sigma * sigma);
					float temp4 = Mathf.Exp(-temp1 / temp2) / temp3;

					result[x, y, z] = temp4;
					sum += temp4;
				}
			}
		}

		for (int z = 0; z < size; z++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					result[x, y, z] /= sum;
				}
			}
		}

		return result;
	}



	//debug function to populate a specified cell configuration at position [0,0,0] in the terrainMap
	void PopulateTerrainMap_debug(int config)
	{
		var bits = new BitArray(new int[] { config }); //convert into bit array

		terrainMap[0, 0, 0].dstToSurface = System.Convert.ToInt32(bits.Get(0));
		terrainMap[1, 0, 0].dstToSurface = System.Convert.ToInt32(bits.Get(1));
		terrainMap[1, 1, 0].dstToSurface = System.Convert.ToInt32(bits.Get(2));
		terrainMap[0, 1, 0].dstToSurface = System.Convert.ToInt32(bits.Get(3));
		terrainMap[0, 0, 1].dstToSurface = System.Convert.ToInt32(bits.Get(4));
		terrainMap[1, 0, 1].dstToSurface = System.Convert.ToInt32(bits.Get(5));
		terrainMap[1, 1, 1].dstToSurface = System.Convert.ToInt32(bits.Get(6));
		terrainMap[0, 1, 1].dstToSurface = System.Convert.ToInt32(bits.Get(7));
	}

	void CreateMeshData()
	{
		ClearMeshData();

		//looking at the cubes, not the points, so you only need to loop the width number and not the width + 1 numbers
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					MarchCube(new Vector3Int(x, y, z));
				}
			}
		}

		BuildMesh();
	}

	//helper function to create a configIndex from cube values from the terrainMap data
	int GetCubeConfiguration(float[] cube)
    {
		int configurationIndex = 0; //8 bit integer which describes one of the 256 TriangleTable entries
		for (int i = 0; i < 8; i++)
        {
			if (cube[i] > terrainSurface)
				configurationIndex |= 1 << i; //the 8bit integer is built bit by bit by masking ones or zeros to each bit depth slot i
        }
		return configurationIndex;

	}


	//position is the position of the given cube, cube is an array
	//Compute marching cube algorithm on given cube cell, add vertices and triangles to list
	public void MarchCube(Vector3Int position)
	{
		//Sample terrain values at each corner of the cube
		float[] cube = new float[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + GameData.CornerTable[i]);
		}

		int configIndex = GetCubeConfiguration(cube);

		if (configIndex == 0 || configIndex == 255) //ignore entries in the TriangleTable that have all -1 value entries
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++) //never more than 5 triangles in each cube
		{
			for (int p = 0; p < 3; p++) //loop through 3 points of the triangle
			{
				int indice = GameData.TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return;

				// Get the vertices for the start and end of this edge
				Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				if (smoothTerrain)
				{
					// Get the terrain values at either end of our current edge from the cube array created above
					float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
					float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

					// Calculate the difference between the terrain values
					float difference = vert2Sample - vert1Sample;

					// If the difference is 0, then the terrain passes through the middle
					if (difference == 0)
						difference = terrainSurface; //[BUG] Would this break then if the terrainSurface was anything other than 0.5?
					else
						difference = (terrainSurface - vert1Sample) / difference; //not really a difference here but more of a ratio

					// Calculate the point along the edge that passes through
					vertPosition = vert1 + ((vert2 - vert1) * difference);
				} 
				else 
				{ 
					// Get midpoint between the two vertices of this edge
					vertPosition = (vert1 + vert2) / 2f;
				}

				//add the index of the last thing added to the vertices list and increment edgeIndex
				if (flatShaded)
				{
					vertices.Add(vertPosition);
					triangles.Add(vertices.Count - 1);
					uvs.Add(new Vector2(terrainMap[position.x, position.y, position.z].textureID, 0));
				}
                else
                {
					triangles.Add(VertForIndice(vertPosition, position));
				}
				edgeIndex++;
			}
		}
	}

	//Tetrahedralization approach for volume calculation of Marching Cube Cell Configuration
	//Input position of cell to analyze
	//Output volume bounded by mesh and cell walls
	public float TetraCellVolume(Vector3Int position)
	{
		position -= chunkPosition; //adjust for chunk system

		float totalCellVolume = 0f;

		//Sample terrain values at each corner of the cube
		float[] cube = new float[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + GameData.CornerTable[i]);
		}

		int configIndex = GetCubeConfiguration(cube);

		//ignore entries in the TriangleTable that have all -1 value entries)
		if (configIndex == 0)
			return 1; //corners are all zero which means all dirt, return full volume cube
		if (configIndex == 255) 
			return 0; //corners are all ones which means all air, return no volume


		int tetraIndex = 0;
		for (int i = 0; i < 7; i++) //never more than 7? tetrahedrons in each cell
		{
			//Reset list of tetrahedron points
			Vector3[] tetraPoints = new Vector3[4];

			for (int p = 0; p < 4; p++) //loop through 4 points of the tetrahedron
			{
				int indice = GameData.TetrahedronTable[configIndex, tetraIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line
				Vector3 vertPosition = Vector3.zero;

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					goto AfterLoop;

				if (indice < 12) //if it's an edge vertex
				{
					// Get the vertices for the start and end of this edge					
					Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
					Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

					
					if (smoothTerrain)
					{
						// Get the terrain values at either end of our current edge from the cube array created above
						float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
						float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

						// Calculate the difference between the terrain values
						float difference = vert2Sample - vert1Sample;

						// If the difference is 0, then the terrain passes through the middle
						if (difference == 0)
							difference = terrainSurface; //[BUG] Would this break then if the terrainSurface was anything other than 0.5?
						else
							difference = (terrainSurface - vert1Sample) / difference; //not really a difference here but more of a ratio

						// Calculate the point along the edge that passes through
						vertPosition = vert1 + ((vert2 - vert1) * difference);
					}
					else
					{
						// Get midpoint between the two vertices of this edge
						vertPosition = (vert1 + vert2) / 2f;
					}
				}
				if (indice > 11) //if it's a cube corner
                {
					vertPosition = position + GameData.CornerTable[indice - 12];

				}

				//Add vertex point to tetrahedron group
				tetraPoints[p] = vertPosition;
				
				tetraIndex++;
			}

			//Volume of current tetrahedron
			float currentTetrahedronVolume = CalculateTetrahedronVolume(tetraPoints[0], tetraPoints[1], tetraPoints[2], tetraPoints[3]);

			//Add to running total volume of the cell
			totalCellVolume = totalCellVolume + currentTetrahedronVolume;
		}

		//Point to jump to if/when it sees a -1 in the TetrahedronTable
		AfterLoop:

		//If cell case is normal return volume, if inverted return 1 - the volume
		if (GameData.CaseInversionTable[configIndex] == 0)
			return totalCellVolume;
		if (GameData.CaseInversionTable[configIndex] == 1)
			return 1 - totalCellVolume;
		else
			Debug.Log("How did you get here?");
			return 0;
	}


	//position is the position of the given cube, cube is an array
	//Compute marching cube algorithm on given cube cell, add vertices and triangles to list
	//modified to provide hooks for volume calculation by sub-tetrahedrons
	//Return the cube values, the TriangleTable array value that identify's edges that the vertices are on, and a corresponding list of coordinates for the vertex locations
	(float[] cube, int[] EdgesWithVertices, Vector3[] CoordsOfVertices) MarchCube_Volume(Vector3Int position)
	{
		//Sample terrain values at each corner of the cube
		float[] cube = new float[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + GameData.CornerTable[i]);
		}

		int configIndex = GetCubeConfiguration(cube);

		//Initialize EdgesWithVertices and CoordsOfVertices
		int[] UniqueEdgesWithVertices = new int[12];
		Vector3[] UniqueCoordsOfVertices = new Vector3[12];
		int[] AllEdgesWithVertices = new int[15]; //includes some duplicated edge and vertice information
		Vector3[] AllCoordsOfVertices = new Vector3[15]; //includes some duplicated edge and vertice information

		if (configIndex == 0 || configIndex == 255) //ignore entries in the TriangleTable that have all -1 value entries
			return (cube, UniqueEdgesWithVertices, UniqueCoordsOfVertices);

		//read out the TriangleTable line entry and write the values to EdgesWithVertices
		//for (int i = 0; i < 12; i++)
		//LEFT OFF HERE

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++) //never more than 5 triangles in each cube
		{
			for (int p = 0; p < 3; p++) //loop through 3 points of the triangle
			{
				int indice = GameData.TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return (cube, AllEdgesWithVertices, AllCoordsOfVertices);

				// Get the vertices for the start and end of this edge
				Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				if (smoothTerrain)
				{
					// Get the terrain values at either end of our current edge from the cube array created above
					float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
					float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

					// Calculate the difference between the terrain values
					float difference = vert2Sample - vert1Sample;

					// If the difference is 0, then the terrain passes through the middle
					if (difference == 0)
						difference = terrainSurface; //[BUG] Would this break then if the terrainSurface was anything other than 0.5?
					else
						difference = (terrainSurface - vert1Sample) / difference; //not really a difference here but more of a ratio

					// Calculate the point along the edge that passes through
					vertPosition = vert1 + ((vert2 - vert1) * difference);
				}
				else
				{
					// Get midpoint between the two vertices of this edge
					vertPosition = (vert1 + vert2) / 2f;
				}

				//add the index of the last thing added to the vertices list and increment edgeIndex
				if (flatShaded)
				{
					vertices.Add(vertPosition);
					triangles.Add(vertices.Count - 1);
				}
				else
				{
					triangles.Add(VertForIndice(vertPosition, position));
				}

				AllEdgesWithVertices[edgeIndex] = GameData.TriangleTable[configIndex, edgeIndex];
				AllCoordsOfVertices[edgeIndex] = vertPosition;

				edgeIndex++;
			}
		}
		return (cube, AllEdgesWithVertices, AllCoordsOfVertices);

	}



	//position is the position of the given cube, configIndex will index the TriangleTable
	void MarchCube_debug (Vector3 position, int configIndex)
    {
		if (configIndex == 0 || configIndex == 255) //ignore entries in the TriangleTable that have all -1 value entries
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++) //never more than 5 triangles in each cube
        {
			for (int p =0; p<3; p++) //loop through 3 points of the triangle
            {
				int indice = GameData.TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return;

				Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];
				Vector3 vertPosition = (vert1 + vert2) / 2f; //midpoint between the two vertices

				//add the index of the last thing added to the vertices list and increment edgeIndex
				vertices.Add(vertPosition);					
				triangles.Add(vertices.Count - 1);				
				edgeIndex++;
			}
        }
	}

	public Vector3 WorldToVoxel(Vector3 world_point)
    {
		//Initialize voxel vector
		Vector3 voxel_point = Vector3.zero;

		//Convert world vector to voxel
		voxel_point = world_point; //NO SCALING YET

		return voxel_point;
    }


	public GameObject spherePrefab; 

	public void DebugTerrain (Vector3 pos)
    { 
		Debug.Log(pos.x + " " + pos.y + " " + pos.z);

		//Convert world point coord to voxel coord
		//Vector3 vox_point = WorldToVoxel(pos);

		//Find marching cube cell of the point on the surface that you are looking at
		//The cube are created all in the +x, +y, +z direction from a starting position using the CornerTable
		//Therefore if you round down the point vector that you are looking at to the nearest starting cube position,
		// you can just calculate the cube corners from the CornerTable
		Vector3Int cube_start_pos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

		Vector3Int[] cube_pos = new Vector3Int[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube_pos[i] = cube_start_pos + GameData.CornerTable[i];
		}

		//https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
		//https://forum.unity.com/threads/draw-cylinder-between-2-points.23510/
		//https://answers.unity.com/questions/21174/create-cylinder-primitive-between-2-endpoints.html

		//CreateCylinderBetweenPoints(cube_pos[0], cube_pos[1], 0.05f);

		//Debug.Log("HERE");

		GameObject DebugCube = new GameObject();
		for (int i = 0; i < 12; i++) //12edges in the cube
        {
			CreateCylinderBetweenPoints(cube_pos[GameData.EdgeIndexes[i,0]], cube_pos[GameData.EdgeIndexes[i, 1]], 0.05f, DebugCube);
		}
		var sphere = GameObject.Instantiate(spherePrefab, cube_start_pos, Quaternion.identity); //Instantiate is a function of MonoBehaviors, add GameObject. before Instantiate to use it here
		sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		sphere.transform.parent = DebugCube.transform;
	}


	//https://answers.unity.com/questions/21174/create-cylinder-primitive-between-2-endpoints.html
	public GameObject cylinderPrefab; //assumed to be 1m x 1m x 2m default unity cylinder to make calculations easy

	void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width, GameObject ParentObject)
	{
		var offset = end - start;
		var scale = new Vector3(width, offset.magnitude / 2.0f, width);
		var position = start + (offset / 2.0f);

		var cylinder = GameObject.Instantiate(cylinderPrefab, position, Quaternion.identity);  //Instantiate is a function of MonoBehaviors, add GameObject. before Instantiate to use it here
		cylinder.transform.up = offset;
		cylinder.transform.localScale = scale;
		cylinder.transform.parent = ParentObject.transform;
	}

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
		for (int x = Mathf.Clamp(Mathf.CeilToInt(pos.x - editSphereRadius), 0, width+1); x < Mathf.Clamp(Mathf.FloorToInt(pos.x + editSphereRadius)+1, 0, width + 1); x++) //bound grid search by the sphere position and diameter
		{
			//for (int y = 0; y < height; y++)
			for (int y = Mathf.Clamp(Mathf.CeilToInt(pos.y - editSphereRadius), 0, height + 1); y < Mathf.Clamp(Mathf.FloorToInt(pos.y + editSphereRadius)+1, 0, height + 1); y++)
			{
				//for (int z = 0; z < width; z++)
				for (int z = Mathf.Clamp(Mathf.CeilToInt(pos.z - editSphereRadius), 0, width + 1); z < Mathf.Clamp(Mathf.FloorToInt(pos.z + editSphereRadius)+1, 0, width + 1); z++)
				{
					//if point within sphere, add to list
					if (Vector3.Distance(new Vector3Int(x,y,z), pos) < editSphereDiameter/2)
                    {
						EditVerticesGroup.Add(new Vector3Int(x, y, z));
                    }
				}
			}
		}

		return EditVerticesGroup;
	}

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
			Vector3Int Neighbor_0 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, width), Mathf.Clamp(vertLocation.y - 1, 0, height - 1), Mathf.Clamp(vertLocation.z - 1, 0, width));
			Vector3Int Neighbor_1 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, width), Mathf.Clamp(vertLocation.y - 1, 0, height - 1), Mathf.Clamp(vertLocation.z - 1, 0, width));
			Vector3Int Neighbor_2 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, width), Mathf.Clamp(vertLocation.y - 1, 0, height - 1), Mathf.Clamp(vertLocation.z, 0, width));
			Vector3Int Neighbor_3 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, width), Mathf.Clamp(vertLocation.y - 1, 0, height - 1), Mathf.Clamp(vertLocation.z, 0, width));
			Vector3Int Neighbor_4 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, width), Mathf.Clamp(vertLocation.y, 0, height - 1), Mathf.Clamp(vertLocation.z - 1, 0, width));
			Vector3Int Neighbor_5 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, width), Mathf.Clamp(vertLocation.y, 0, height - 1), Mathf.Clamp(vertLocation.z - 1, 0, width));
			Vector3Int Neighbor_6 = new Vector3Int(Mathf.Clamp(vertLocation.x - 1, 0, width), Mathf.Clamp(vertLocation.y, 0, height - 1), Mathf.Clamp(vertLocation.z, 0, width));
			Vector3Int Neighbor_7 = new Vector3Int(Mathf.Clamp(vertLocation.x, 0, width), Mathf.Clamp(vertLocation.y, 0, height - 1), Mathf.Clamp(vertLocation.z, 0, width));
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

	//Go though each cell position in the group of affects cells for terrain edit
	//Calculate volume of each cell depending on the cube configuration and return sum of volumes
	public float FindVolumeOfCells()
	{
		float cellsVolume = 0;

		// Loop through each cell
		foreach (Vector3Int vertLocation in EditCellsPositions)
		{
			//cube corner value average to approximate cell
			float cellValueAverage = 0f;

			//Sample terrain values at each corner of the cube
			float[] cube = new float[8]; //8 corners in a cube
			for (int i = 0; i < 8; i++)
			{
				cube[i] = SampleTerrain(vertLocation + GameData.CornerTable[i]);
				if (cube[i] > terrainSurface) //remember y values are distorted by the height scalar in the terrainMap
				{
					cellValueAverage = cellValueAverage + 1;
				}
                else
                {
					cellValueAverage = cellValueAverage + 0;
				}
			}

			//average the 8 corner values, 1 is air, 0 is dirt, volume is absolute
			cellValueAverage = cellValueAverage / 8;

			//Find cell configuration
			int configIndex = GetCubeConfiguration(cube);

			//Find volume for this cell configuration
			//cellsVolume = cellsVolume + configIndex; //Go into lookup table, or for this test just use the config index as the unique value
			cellsVolume = cellsVolume + cellValueAverage;
		}

		return cellsVolume;

	}

	// Input a position and diameter of the Edit Sphere
	// Edit the terrainMap to change the encompassed vertices to 0f
	public float PlaceManyTerrain(Vector3 pos, float diameter)
	{
		pos -= chunkPosition; //adjust for chunk system

		// Find list of vertex locations that are within the Edit Sphere
		List<Vector3Int> EditVerticesPositions = FindEditVertices(pos, diameter);

		//Create list of cells affected by the EditSphere
		FindEditCells(EditVerticesPositions);
		//Debug.Log("EditCellsPositions: " + EditCellsPositions.Count);

		//Calculate current volume of terrain of affected cells
		float currentVolume = FindVolumeOfCells();

		// Edit terrainMap values at each of the vertex locations
		foreach (Vector3Int vertLocation in EditVerticesPositions)
		{
			terrainMap[vertLocation.x, vertLocation.y, vertLocation.z].dstToSurface = 0f;
		}

		//Debug.Log("EditVerticesPositions: " + EditVerticesPositions.Count);

		//Calculate new volume of terrain of affected cells
		float newVolume = FindVolumeOfCells();

		//Regenerate Mesh
		CreateMeshData();

		//int EditCellsCount = EditCellsPositions.Count;
		EditCellsPositions = new List<Vector3Int>(); //reset list to be used again
		return newVolume - currentVolume;
	}



	// Input a position and diameter of the Edit Sphere
	// Edit the terrainMap to change the encompassed vertices to 1f
	public float RemoveManyTerrain(Vector3 pos, float diameter)
	{
		pos -= chunkPosition; //adjust for chunk system

		// Find list of vertex locations that are within the Edit Sphere
		List<Vector3Int> EditVerticesPositions = FindEditVertices(pos, diameter);

		//Create list of cells affected by the EditSphere
		FindEditCells(EditVerticesPositions);
		//Debug.Log("EditCellsPositions: " + EditCellsPositions.Count);

		//Calculate current volume of terrain of affected cells
		float currentVolume = FindVolumeOfCells();

		// Edit terrainMap values at each of the vertex locations
		foreach (Vector3Int vertLocation in EditVerticesPositions)
		{
			terrainMap[vertLocation.x, vertLocation.y, vertLocation.z].dstToSurface = 1f;
		}
		
		//Debug.Log("EditVerticesPositions: " + EditVerticesPositions.Count);

		//Calculate new volume of terrain of affected cells
		float newVolume = FindVolumeOfCells();

		//Regenerate Mesh
		CreateMeshData();

		//int EditCellsCount = EditCellsPositions.Count;
		EditCellsPositions = new List<Vector3Int>(); //reset list to be used again
		return newVolume - currentVolume;
	}

	public void PlaceTerrain (Vector3 pos)
    {
		// Simple edit single nearest vertex point
		Vector3Int v3Int = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
		v3Int -= chunkPosition; //adjust for chunk system

		//Debug.Log(string.Format("PlaceTerrain Position: {0}, {1}, {2} ", v3Int.x, v3Int.y, v3Int.z));


		terrainMap[v3Int.x, v3Int.y, v3Int.z].dstToSurface = 0f;

		
		//Regenerate Mesh
		//CreateMeshData();
	}

	public void RemoveTerrain(Vector3 pos)
	{
		Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		v3Int -= chunkPosition; //adjust for chunk system
		terrainMap[v3Int.x, v3Int.y, v3Int.z].dstToSurface = 1f;
		//CreateMeshData();
	}

	float SampleTerrain (Vector3Int point)
	{
		return terrainMap[point.x, point.y, point.z].dstToSurface;
	}

	int VertForIndice (Vector3 vert, Vector3Int point)
    {
		// Loop through all vertices currently in the vertices list
		for (int i = 0; i < vertices.Count; i++)
        {
			// If we find a vert that matches our, then simply return this index
			if (vertices[i] == vert)
				return i;
        }

		// If we didn't find a match, add this vert to the list and return last index
		vertices.Add(vert);
		uvs.Add(new Vector2(terrainMap[point.x, point.y, point.z].textureID, 0));
		return vertices.Count - 1;
    }

    void ClearMeshData()
    {
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
	}
    
	void BuildMesh()
    {
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;

		meshCollider.sharedMesh = mesh;
		
    }

    
}
