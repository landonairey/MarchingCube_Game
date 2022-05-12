using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//How to Make 7 Days to Die in Unity - 01 - Marching Cubes by b3agz
//https://github.com/b3agz/how-to-make-7-days-to-die-in-unity

//LEFT OFF:
//https://youtu.be/PgZDp5Oih38?t=868

//adding new chunk system

public class Marching : MonoBehaviour
{
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();

	MeshFilter meshFilter;
	MeshCollider meshCollider;

	[SerializeField] float terrainSurface = 0.5f;
	[SerializeField] int width = 32; //number of cubes in x/z dim we are going to march through
	[SerializeField] int height = 8; //number of cubes tall we are going to march through
	float[,,] terrainMap; //3D storage for terrain values that we will sample when generating the mesh

	int _configIndex = 0; //debug stepping through marching cube configurations see Update()

	[SerializeField] bool smoothTerrain; //toggle for using linearly interpolated mesh positions or not for the marching cubes
	[SerializeField] bool flatShaded; //toggle for using duplicate vertices or using shared vertices which blends shading

	List<Vector3Int> EditCellsPositions = new List<Vector3Int>(); //list of cells affected in the edit methods

	bool DEBUG_MARCHING_EN = false; //enable debug marching mode which removes the auto regeneration within the update loop and let's you cycle through cases

	private void Start()
    {
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		transform.tag = "Terrain";
		terrainMap = new float[width + 1, height + 1, width + 1]; //needs to be plus one or you'll get an index out of range error

		if (!DEBUG_MARCHING_EN)
		{
			PopulateTerrainMap(); //COMMENT WHEN USING DEBUG_MARCHING
		}
		CreateMeshData();
		BuildMesh();
	}

    private void Update()
    {
		// Uncomment these if you want to change the width, height, terrainSurface at run time 
		/*
		
		terrainMap = new float[width + 1, height + 1, width + 1]; //needs to be plus one or you'll get an index out of range error
		PopulateTerrainMap();
		*/
		/* //COMMENT WHEN USING DEBUG_MARCHING
		ClearMeshData();
		CreateMeshData();
		BuildMesh();
		//*/

		// Uncomment these if you want to change the width, height, terrainSurface at run time 
		//COMMENT WHEN USING DEBUG_MARCHING
		if (!DEBUG_MARCHING_EN)
		{
			// Uncomment these if you want to change the width, height, terrainSurface at run time 
			/*
			terrainMap = new float[width + 1, height + 1, width + 1]; //needs to be plus one or you'll get an index out of range error
			PopulateTerrainMap();
			*/

			ClearMeshData();
			CreateMeshData();
			BuildMesh();
		}


		GetComponent<MeshCollider>().sharedMesh = null;
		GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;


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
				

				/*
				//https://www.reddit.com/r/Unity3D/comments/flwreg/how_do_i_make_a_trs_matrix_manually/
				Matrix4x4 testMat = new Matrix4x4(
				new Vector4(0f, 1f, 0f, 0f),
				new Vector4(0f, 0f, 1f, 0f),
				new Vector4(0f, 0f, 0f, 1f),
				new Vector4(1f, 1f, 1f, 1f));

				float tetrahedronVol = (1 / 6) * testMat.determinant;
				Debug.Log(tetrahedronVol);
				

				Vector3 a = new Vector3(0f, 0f, 0f);
				Vector3 b = new Vector3(1f, 0f, 0f);
				Vector3 c = new Vector3(0f, 1f, 0f);
				Vector3 d = new Vector3(0f, 0f, 1f);

				Debug.Log("Vt: " + CalculateTetrahedronVolume(a, b, c, d));
				*/

				float cellVolume = TetraCellVolume(Vector3Int.zero);
				Debug.Log("Cell Volume :" + cellVolume);

				_configIndex++;

			}


			//UNCOMMENT FOR USING MARCHING_VOLUME
			if (Input.GetKeyDown(KeyCode.Comma))
			{
				/*
				ClearMeshData();
				terrainMap[0, 0, 0] = 1; //define cube corner values manually
				(float[] cube, int[] EdgesWithVertices, Vector3[] CoordsOfVertices) = MarchCube_Volume(Vector3Int.zero);
				BuildMesh();

				//DEBUG
				string cubesString = "";
				for (int i = 0; i < cube.Length; i++)
				{
					cubesString += " " + (cube[i]).ToString();
				}

				string edgeString = "";
				for (int i = 0; i < EdgesWithVertices.Length; i++)
				{
					edgeString += " " + (EdgesWithVertices[i]).ToString();
				}

				string vposString = "";
				for (int i = 0; i < CoordsOfVertices.Length; i++)
				{
					vposString += " " + (CoordsOfVertices[i]).ToString();
				}

				Debug.Log("cube :" + cubesString);
				Debug.Log("edge :" + edgeString);
				Debug.Log("vpos :" + vposString);

				Vector3 a = new Vector3(0f, 0f, 0f);
				Vector3 b = CoordsOfVertices[0];
				Vector3 c = CoordsOfVertices[2];
				Vector3 d = CoordsOfVertices[1];

				//Debug.Log("CoordsOfVertices[0]: " + CoordsOfVertices[0]);
				//Debug.Log("CoordsOfVertices[1]: " + CoordsOfVertices[1]);
				//Debug.Log("CoordsOfVertices[2]: " + CoordsOfVertices[2]);
				//Debug.Log("Vt: " + CalculateTetrahedronVolume(a, b, c, d));

				int cellCase = 1;
				bool aboveTerrain = false;
				Debug.Log("Vt: " + CalculateMarchCellVolume(cellCase, CoordsOfVertices, aboveTerrain)); ;
				*/

				//New Approach:
				ClearMeshData();
				terrainMap[1, 0, 0] = 1; //define cube corner values manually
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
		//https://keisan.casio.com/exec/system/1223609147
		//float Vp =
		//		  (d.x - a.x) * ((b.y - a.y) * (c.z - a.z) - (b.z - a.z) * (c.y - a.y))
		//		+ (d.y - a.y) * ((b.z - a.z) * (c.x - a.x) - (b.x - a.x) * (c.z - a.z))
		//		+ (d.z - a.z) * ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));
		//
		//float Vt = Vp / 6f;

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

					//create a hole in the terrain:
					if (x > 5 && x < 15 && z > 5 && z < 15)
						thisHeight = 1f;

					else
						//Using clamp to bound PerlinNoise as it intends to return a value 0.0f-1.0f but may sometimes be slightly out of that range
						//Multipying by height will return a value in the range of 0-height					
						thisHeight = (float)height * Mathf.Clamp(Mathf.PerlinNoise((float)x / 16f * 1.5f, (float)z / 16f * 1.5f), 0.0f, 1.0f); //the 16f and 1.5f are made up coefficients

						//y points below thisHeight will be negative (below terrain) and y points above thisHeight will be positve and will render 
						terrainMap[x, y, z] = (float)y - thisHeight;
					
						//Debug
						//Debug.Log(thisHeight);
				}
			}
		}
    }

	//debug function to populate a specified cell configuration at position [0,0,0] int he terrainMap
	void PopulateTerrainMap_debug(int config)
	{
		//byte[] configByte = new byte[8];
		//BitArray config_bits = new BitArray(config);

		//y points below thisHeight will be negative (below terrain) and y points above thisHeight will be positve and will render 
		//terrainMap[x, y, z] = (float)y - thisHeight;

		//string bit_str;

		//for (var i = 0; i < config_bits.Length; i++)
		//{
		//	bit_str = bit_str + config_bits[i].ToString();
		//}

		//int checkBit0 = 0x00000001;
		//bool hasBit0 = (config & checkBit0) == checkBit0;
		//bool hasBit0 = (config & 0x00000001) != 0;

		//int checkBit1 = 0x00000010;
		//bool hasBit1 = (config & checkBit1) == checkBit1;
		//bool hasBit1 = (config & 0x00000010) != 0;

		//int checkBit2 = 0x00000100;
		//bool hasBit2 = (config & checkBit2) == checkBit2;

		var bits = new BitArray(new int[] { config }); //convert into bit array

		terrainMap[0, 0, 0] = System.Convert.ToInt32(bits.Get(0));
		terrainMap[1, 0, 0] = System.Convert.ToInt32(bits.Get(1));
		terrainMap[1, 1, 0] = System.Convert.ToInt32(bits.Get(2));
		terrainMap[0, 1, 0] = System.Convert.ToInt32(bits.Get(3));
		terrainMap[0, 0, 1] = System.Convert.ToInt32(bits.Get(4));
		terrainMap[1, 0, 1] = System.Convert.ToInt32(bits.Get(5));
		terrainMap[1, 1, 1] = System.Convert.ToInt32(bits.Get(6));
		terrainMap[0, 1, 1] = System.Convert.ToInt32(bits.Get(7));

		//Debug.Log("config 0 " + bits.Get(0));
		//Debug.Log("config 1 " + bits.Get(1));
		//Debug.Log("config 2 " + bits.Get(2));
		//Debug.Log(string.Format("config byte: {0:0.}, {1:0.}, {2:0.}", System.Convert.ToInt32(bits.Get(2)), System.Convert.ToInt32(bits.Get(1)), System.Convert.ToInt32(bits.Get(0)))); //Use format for better performance);
		//Debug.Log("config " + config);



	}

	void CreateMeshData()
	{
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
	void MarchCube(Vector3Int position)
	{
		//Sample terrain values at each corner of the cube
		float[] cube = new float[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + CornerTable[i]);
		}

		int configIndex = GetCubeConfiguration(cube);

		if (configIndex == 0 || configIndex == 255) //ignore entries in the TriangleTable that have all -1 value entries
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++) //never more than 5 triangles in each cube
		{
			for (int p = 0; p < 3; p++) //loop through 3 points of the triangle
			{
				int indice = TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return;

				// Get the vertices for the start and end of this edge
				Vector3 vert1 = position + CornerTable[EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + CornerTable[EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				if (smoothTerrain)
				{
					// Get the terrain values at either end of our current edge from the cube array created above
					float vert1Sample = cube[EdgeIndexes[indice, 0]];
					float vert2Sample = cube[EdgeIndexes[indice, 1]];

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
					triangles.Add(VertForIndice(vertPosition));
                }
				edgeIndex++;
			}
		}
	}

	//Tetrahedralization approach for volume calculation of Marching Cube Cell Configuration
	//Input position of cell to analyze
	//Output volume bounded by mesh and cell walls
	float TetraCellVolume(Vector3Int position)
	{
		float totalCellVolume = 0f;

		//Sample terrain values at each corner of the cube
		float[] cube = new float[8]; //8 corners in a cube
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + CornerTable[i]);
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
				int indice = TetrahedronTable[configIndex, tetraIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line
				Vector3 vertPosition = Vector3.zero;

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					goto AfterLoop;

				if (indice < 12) //if it's an edge vertex
				{
					// Get the vertices for the start and end of this edge					
					Vector3 vert1 = position + CornerTable[EdgeIndexes[indice, 0]];
					Vector3 vert2 = position + CornerTable[EdgeIndexes[indice, 1]];

					
					if (smoothTerrain)
					{
						// Get the terrain values at either end of our current edge from the cube array created above
						float vert1Sample = cube[EdgeIndexes[indice, 0]];
						float vert2Sample = cube[EdgeIndexes[indice, 1]];

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
					vertPosition = CornerTable[indice - 12];

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
		if (CaseInversionTable[configIndex] == 0)
			return totalCellVolume;
		if (CaseInversionTable[configIndex] == 1)
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
			cube[i] = SampleTerrain(position + CornerTable[i]);
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
				int indice = TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return (cube, AllEdgesWithVertices, AllCoordsOfVertices);

				// Get the vertices for the start and end of this edge
				Vector3 vert1 = position + CornerTable[EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + CornerTable[EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				if (smoothTerrain)
				{
					// Get the terrain values at either end of our current edge from the cube array created above
					float vert1Sample = cube[EdgeIndexes[indice, 0]];
					float vert2Sample = cube[EdgeIndexes[indice, 1]];

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
					triangles.Add(VertForIndice(vertPosition));
				}

				AllEdgesWithVertices[edgeIndex] = TriangleTable[configIndex, edgeIndex];
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
				int indice = TriangleTable[configIndex, edgeIndex]; //configINdex selects a line from the TriangleTable, edgeIndex indees the singles value in the line

				if (indice == -1) //stop when you hit a -1 value (which means there are no more triangles to draw)
					return;

				Vector3 vert1 = position + CornerTable[EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + CornerTable[EdgeIndexes[indice, 1]];
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
			cube_pos[i] = cube_start_pos + CornerTable[i];
		}

		//https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
		//https://forum.unity.com/threads/draw-cylinder-between-2-points.23510/
		//https://answers.unity.com/questions/21174/create-cylinder-primitive-between-2-endpoints.html

		//CreateCylinderBetweenPoints(cube_pos[0], cube_pos[1], 0.05f);

		Debug.Log("HERE");

		GameObject DebugCube = new GameObject();
		for (int i = 0; i < 12; i++) //12edges in the cube
        {
			CreateCylinderBetweenPoints(cube_pos[EdgeIndexes[i,0]], cube_pos[EdgeIndexes[i, 1]], 0.05f, DebugCube);
		}
		var sphere = Instantiate(spherePrefab, cube_start_pos, Quaternion.identity);
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

		var cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
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
				cube[i] = SampleTerrain(vertLocation + CornerTable[i]);
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
		// Find list of vertex locations that are within the Edit Sphere
		List<Vector3Int> EditVerticesPositions = FindEditVertices(pos, diameter);

		//Create list of cells affected by the EditSphere
		FindEditCells(EditVerticesPositions);
		Debug.Log("EditCellsPositions: " + EditCellsPositions.Count);

		//Calculate current volume of terrain of affected cells
		float currentVolume = FindVolumeOfCells();

		// Edit terrainMap values at each of the vertex locations
		foreach (Vector3Int vertLocation in EditVerticesPositions)
		{
			terrainMap[vertLocation.x, vertLocation.y, vertLocation.z] = 0f;
		}

		Debug.Log("EditVerticesPositions: " + EditVerticesPositions.Count);

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
		// Find list of vertex locations that are within the Edit Sphere
		List<Vector3Int> EditVerticesPositions = FindEditVertices(pos, diameter);

		//Create list of cells affected by the EditSphere
		FindEditCells(EditVerticesPositions);
		Debug.Log("EditCellsPositions: " + EditCellsPositions.Count);

		//Calculate current volume of terrain of affected cells
		float currentVolume = FindVolumeOfCells();

		// Edit terrainMap values at each of the vertex locations
		foreach (Vector3Int vertLocation in EditVerticesPositions)
		{
			terrainMap[vertLocation.x, vertLocation.y, vertLocation.z] = 1f;
		}
		
		Debug.Log("EditVerticesPositions: " + EditVerticesPositions.Count);

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
		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 0f;

		
		//Regenerate Mesh
		CreateMeshData();
	}

	public void RemoveTerrain(Vector3 pos)
	{
		Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 1f;
		CreateMeshData();
	}

	float SampleTerrain (Vector3Int point)
	{
		return terrainMap[point.x, point.y, point.z];
	}

	int VertForIndice (Vector3 vert)
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
		return vertices.Count - 1;
    }

    void ClearMeshData()
    {
		vertices.Clear();
		triangles.Clear();
	}
    
	void BuildMesh()
    {
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;

		meshCollider.sharedMesh = mesh;

    }

    Vector3Int[] CornerTable = new Vector3Int[8]
    {
        new Vector3Int(0,0,0),
        new Vector3Int(1,0,0),
        new Vector3Int(1,1,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,0,1),
        new Vector3Int(1,0,1),
        new Vector3Int(1,1,1),
        new Vector3Int(0,1,1),
    };

	//EdgeTable uses the same Vector3's as what's in the CornerTable. For simplicity, a new lookup called EdgeIndexes will replace this table and point to the entries in CornerTable
	/*
	Vector3[,] EdgeTable = new Vector3[12, 2] {

		{ new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f) },
		{ new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f) },
		{ new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f) },
		{ new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f) },
		{ new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f) },
		{ new Vector3(1.0f, 0.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f) },
		{ new Vector3(0.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f) },
		{ new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 1.0f) },
		{ new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f) },
		{ new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 1.0f) },
		{ new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f) },
		{ new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 1.0f, 1.0f) }

	};
	*/

	int[,] EdgeIndexes = new int[12, 2]
	{
		{0, 1}, {1, 2}, {3, 2}, {0, 3}, {4, 5}, {5, 6}, {7, 6}, {4, 7}, {0, 4}, {1, 5}, {2, 6}, {3, 7}
	};

	private int[,] TriangleTable = new int[,] {

		{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
		{3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
		{3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
		{3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
		{9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
		{9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
		{2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
		{8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
		{3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
		{1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
		{4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
		{4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
		{5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
		{2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
		{9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
		{0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
		{2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
		{10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
		{5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
		{5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
		{9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
		{0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
		{1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
		{10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
		{8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
		{2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
		{7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
		{2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
		{11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
		{5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
		{11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
		{11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
		{1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
		{9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
		{5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
		{2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
		{5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
		{6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
		{3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
		{6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
		{5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
		{1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
		{10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
		{6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
		{8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
		{7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
		{3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
		{5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
		{0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
		{9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
		{8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
		{5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
		{0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
		{6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
		{10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
		{10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
		{8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
		{1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
		{0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
		{10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
		{3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
		{6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
		{9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
		{8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
		{3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
		{6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
		{0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
		{10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
		{10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
		{2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
		{7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
		{7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
		{2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
		{1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
		{11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
		{8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
		{0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
		{7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
		{10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
		{2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
		{7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
		{2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
		{1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
		{10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
		{10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
		{0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
		{7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
		{6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
		{8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
		{9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
		{6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
		{4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
		{10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
		{8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
		{0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
		{1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
		{8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
		{10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
		{4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
		{10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
		{5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
		{11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
		{9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
		{7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
		{3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
		{7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
		{3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
		{6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
		{9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
		{1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
		{4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
		{7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
		{6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
		{3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
		{0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
		{6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
		{0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
		{11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
		{6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
		{5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
		{9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
		{1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
		{1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
		{10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
		{0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
		{5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
		{10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
		{11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
		{9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
		{7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
		{2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
		{8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
		{9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
		{9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
		{1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
		{9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
		{5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
		{0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
		{10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
		{2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
		{0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
		{0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
		{9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
		{5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
		{3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
		{5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
		{8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
		{0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
		{9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
		{1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
		{3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
		{4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
		{9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
		{11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
		{11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
		{2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
		{9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
		{3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
		{1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
		{4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
		{3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
		{0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
		{9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
		{1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}

	};

	//Sets of four vertices that define sub divided tetrahedrons within the cell configuration, 0-11 for edges, 12-19 for corners
	private int[,] TetrahedronTable = new int[,] {
		{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 8, 3, 12, 9, 8, 1, 12, 9, 1, 12, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 12, 1, 2, 10, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 2, 10, 14, 0, 2, 9, 14, 0, 9, 14, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 8, 3, 12, 2, 8, 14, 12, 2, 8, 14, 10, 10, 9, 8, 14, 13, 9, 8, 14, 13, 12, 8, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 11, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 0, 8, 12, 2, 0, 11, 12, 2, 11, 12, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 11, 2, 15, 0, 1, 9, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 11, 2, 15, 1, 11, 13, 15, 1, 11, 13, 9, 9, 8, 11, 13, 12, 8, 11, 13, 12, 15, 11, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 3, 11, 15, 1, 3, 10, 15, 1, 10, 15, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 10, 1, 14, 0, 10, 12, 14, 0, 10, 12, 8, 8, 11, 10, 12, 15, 11, 10, 12, 15, 14, 10, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 9, 0, 13, 3, 9, 15, 13, 3, 9, 15, 11, 11, 10, 9, 15, 14, 10, 9, 15, 14, 13, 9, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 10, 8, 16, 9, 10, 17, 16, 18, 10, 17, 16, 8, 10, 11, 16, 19, 10, 11, 16, 19, 10, 18, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 8, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 4, 7, 16, 0, 4, 3, 16, 0, 3, 16, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 8, 16, 9, 0, 1, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 1, 9, 13, 4, 1, 16, 13, 4, 1, 16, 7, 7, 3, 1, 16, 12, 3, 1, 16, 12, 13, 1, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 8, 16, 10, 1, 2, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 4, 7, 16, 3, 0, 4, 16, 3, 0, 16, 12, 1, 2, 10, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 2, 10, 14, 9, 0, 2, 14, 9, 0, 14, 13, 8, 4, 7, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 7, 3, 15, 2, 7, 19, 15, 10, 9, 2, 7, 10, 2, 7, 19, 10, 7, 18, 19, 10, 7, 9, 18, 9, 7, 4, 18, 9, 4, 17, 18},
		{8, 4, 7, 16, 3, 11, 2, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 4, 7, 16, 11, 4, 15, 16, 11, 4, 15, 2, 2, 0, 4, 15, 12, 0, 4, 15, 12, 16, 4, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 13, 8, 4, 7, 16, 2, 3, 11, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 11, 16, 4, 12, 11, 16, 4, 12, 11, 9, 2, 12, 11, 9, 2, 12, 11, 15, 2, 12, 13, 9, 2, 1, 13, 9, -1, -1, -1, -1},
		{3, 10, 1, 14, 3, 11, 10, 14, 3, 11, 14, 15, 7, 8, 4, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 1, 4, 13, 17, 1, 4, 13, 17, 1, 4, 10, 17, 18, 4, 10, 1, 4, 10, 11, 18, 4, 10, 11, 18, 4, 7, 11, 18, 19, 7, 11},
		{7, 8, 4, 16, 3, 9, 0, 13, 3, 9, 15, 13, 3, 9, 15, 11, 11, 10, 9, 15, 14, 10, 9, 15, 14, 13, 9, 15, -1, -1, -1, -1},
		{4, 11, 7, 19, 4, 11, 17, 19, 4, 11, 17, 9, 9, 10, 11, 17, 18, 10, 11, 17, 18, 19, 11, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 4, 9, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 3, 0, 12, 4, 9, 5, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 0, 1, 13, 4, 0, 5, 13, 4, 5, 13, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 5, 4, 17, 8, 5, 12, 17, 8, 5, 12, 3, 3, 1, 5, 12, 13, 1, 5, 12, 13, 17, 5, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 4, 9, 17, 10, 1, 2, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 8, 12, 1, 2, 10, 14, 4, 9, 5, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 2, 10, 14, 5, 2, 17, 14, 5, 2, 17, 4, 4, 0, 2, 17, 13, 0, 2, 17, 13, 14, 2, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 3, 2, 12, 8, 13, 2, 12, 8, 13, 2, 4, 10, 13, 2, 4, 10, 13, 2, 14, 10, 13, 17, 4, 10, 5, 17, 4, -1, -1, -1, -1},
		{2, 3, 11, 15, 9, 5, 4, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 11, 2, 15, 0, 8, 11, 15, 0, 8, 15, 12, 4, 9, 5, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 5, 4, 17, 0, 1, 5, 17, 0, 1, 17, 13, 2, 3, 11, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 4, 11, 16, 19, 4, 11, 16, 19, 4, 11, 5, 19, 18, 11, 5, 4, 11, 5, 1, 18, 11, 5, 1, 18, 11, 2, 1, 18, 14, 2, 1},
		{10, 3, 11, 15, 10, 1, 3, 15, 10, 1, 15, 14, 9, 5, 4, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 5, 17, 0, 10, 1, 14, 0, 10, 12, 14, 0, 10, 12, 8, 8, 11, 10, 12, 15, 11, 10, 12, 15, 14, 10, 12, -1, -1, -1, -1},
		{5, 11, 10, 18, 5, 11, 19, 18, 4, 0, 5, 11, 4, 5, 11, 19, 4, 11, 16, 19, 4, 11, 0, 16, 0, 11, 3, 16, 0, 3, 12, 16},
		{5, 8, 4, 16, 5, 8, 18, 16, 5, 8, 18, 10, 10, 11, 8, 18, 19, 11, 8, 18, 19, 16, 8, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 7, 8, 16, 5, 7, 9, 16, 5, 9, 16, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 3, 0, 12, 9, 3, 17, 12, 9, 3, 17, 5, 5, 7, 3, 17, 16, 7, 3, 17, 16, 12, 3, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 7, 8, 16, 0, 7, 13, 16, 0, 7, 13, 1, 1, 5, 7, 13, 17, 5, 7, 13, 17, 16, 7, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 1, 7, 19, 5, 1, 18, 19, 14, 1, 18, 19, 7, 1, 3, 19, 15, 1, 3, 19, 15, 1, 14, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 7, 8, 16, 9, 5, 7, 16, 9, 5, 16, 17, 10, 1, 2, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 1, 2, 14, 9, 3, 0, 12, 9, 3, 17, 12, 9, 3, 17, 5, 5, 7, 3, 17, 16, 7, 3, 17, 16, 12, 3, 17, -1, -1, -1, -1},
		{5, 10, 7, 18, 19, 10, 7, 18, 19, 10, 7, 2, 19, 15, 7, 2, 10, 7, 2, 0, 15, 7, 2, 0, 15, 7, 8, 0, 15, 12, 8, 0},
		{2, 5, 10, 18, 2, 5, 15, 18, 2, 5, 15, 3, 3, 7, 5, 15, 19, 7, 5, 15, 19, 18, 5, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 9, 5, 17, 7, 8, 9, 17, 7, 8, 17, 16, 3, 11, 2, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 5, 7, 19, 11, 5, 18, 19, 2, 0, 11, 5, 2, 11, 5, 18, 2, 5, 14, 18, 2, 5, 0, 14, 0, 5, 9, 14, 0, 9, 13, 14},
		{2, 3, 11, 15, 0, 7, 8, 16, 0, 7, 13, 16, 0, 7, 13, 1, 1, 5, 7, 13, 17, 5, 7, 13, 17, 16, 7, 13, -1, -1, -1, -1},
		{11, 1, 2, 14, 11, 1, 19, 14, 11, 1, 19, 7, 7, 5, 1, 19, 18, 5, 1, 19, 18, 14, 1, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 11, 10, 14, 1, 3, 11, 14, 11, 3, 14, 15, 9, 7, 8, 16, 5, 7, 9, 16, 5, 9, 16, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 7, 11, 19, 5, 10, 11, 19, 5, 10, 19, 18, 5, 7, 11, 0, 5, 10, 11, 0, 5, 0, 9, 13, 5, 10, 0, 13, 1, 10, 0, 13},
		{11, 10, 5, 18, 11, 7, 5, 18, 11, 7, 18, 19, 11, 10, 5, 0, 11, 7, 5, 0, 11, 0, 3, 12, 11, 7, 0, 12, 8, 7, 0, 12},
		{5, 7, 11, 19, 10, 5, 11, 19, 10, 5, 19, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 6, 5, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 12, 5, 10, 6, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 13, 5, 10, 6, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 8, 3, 12, 1, 9, 8, 12, 1, 9, 12, 13, 5, 10, 6, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 6, 5, 18, 2, 6, 1, 18, 2, 1, 18, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 6, 5, 18, 1, 2, 6, 18, 1, 2, 18, 14, 3, 0, 8, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 6, 5, 18, 9, 6, 13, 18, 9, 6, 13, 0, 0, 2, 6, 13, 14, 2, 6, 13, 14, 18, 6, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 5, 8, 17, 16, 5, 8, 17, 16, 5, 8, 6, 16, 19, 8, 6, 5, 8, 6, 2, 19, 8, 6, 2, 19, 8, 3, 2, 19, 15, 3, 2},
		{2, 3, 11, 15, 10, 6, 5, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 0, 8, 12, 11, 2, 0, 12, 11, 2, 12, 15, 10, 6, 5, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 2, 3, 15, 10, 6, 5, 18, 0, 1, 9, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 10, 6, 18, 1, 11, 2, 15, 1, 11, 13, 15, 1, 11, 13, 9, 9, 8, 11, 13, 12, 8, 11, 13, 12, 15, 11, 13, -1, -1, -1, -1},
		{6, 3, 11, 15, 6, 3, 18, 15, 6, 3, 18, 5, 5, 1, 3, 18, 14, 1, 3, 18, 14, 15, 3, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 8, 11, 19, 6, 8, 16, 19, 5, 1, 6, 8, 5, 6, 8, 16, 5, 8, 17, 16, 5, 8, 1, 17, 1, 8, 0, 17, 1, 0, 13, 17},
		{3, 11, 6, 15, 3, 14, 6, 15, 3, 14, 6, 0, 5, 14, 6, 0, 5, 14, 6, 18, 5, 14, 13, 0, 5, 9, 13, 0, -1, -1, -1, -1},
		{6, 9, 5, 17, 6, 9, 19, 17, 6, 9, 19, 11, 11, 8, 9, 19, 16, 8, 9, 19, 16, 17, 9, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 8, 4, 16, 6, 5, 10, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 3, 0, 12, 4, 7, 3, 12, 4, 7, 12, 16, 6, 5, 10, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 4, 7, 16, 9, 0, 1, 13, 6, 5, 10, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 5, 10, 18, 4, 1, 9, 13, 4, 1, 16, 13, 4, 1, 16, 7, 7, 3, 1, 16, 12, 3, 1, 16, 12, 13, 1, 16, -1, -1, -1, -1},
		{6, 1, 2, 14, 6, 5, 1, 14, 6, 5, 14, 18, 4, 7, 8, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 7, 3, 12, 0, 4, 7, 12, 7, 4, 12, 16, 1, 6, 5, 18, 2, 6, 1, 18, 2, 1, 18, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 4, 7, 16, 9, 6, 5, 18, 9, 6, 13, 18, 9, 6, 13, 0, 0, 2, 6, 13, 14, 2, 6, 13, 14, 18, 6, 13, -1, -1, -1, -1},
		{7, 3, 2, 15, 7, 6, 2, 15, 7, 6, 15, 19, 7, 3, 2, 9, 7, 6, 2, 9, 7, 9, 4, 17, 7, 6, 9, 17, 5, 6, 9, 17},
		{10, 6, 5, 18, 11, 2, 3, 15, 4, 7, 8, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 6, 5, 18, 11, 4, 7, 16, 11, 4, 15, 16, 11, 4, 15, 2, 2, 0, 4, 15, 12, 0, 4, 15, 12, 16, 4, 15, -1, -1, -1, -1},
		{2, 3, 11, 15, 9, 0, 1, 13, 4, 7, 8, 16, 6, 5, 10, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 7, 11, 16, 4, 12, 11, 16, 4, 12, 11, 9, 2, 12, 11, 9, 2, 12, 11, 15, 2, 12, 13, 9, 2, 1, 13, 9, 6, 5, 10, 18},
		{4, 7, 8, 16, 6, 3, 11, 15, 6, 3, 18, 15, 6, 3, 18, 5, 5, 1, 3, 18, 14, 1, 3, 18, 14, 15, 3, 18, -1, -1, -1, -1},
		{5, 1, 0, 13, 5, 4, 0, 13, 5, 4, 13, 17, 5, 1, 0, 11, 5, 4, 0, 11, 5, 11, 6, 19, 5, 4, 11, 19, 7, 4, 11, 19},
		{6, 5, 9, 18, 6, 14, 9, 18, 6, 14, 9, 11, 0, 14, 9, 11, 0, 14, 9, 13, 0, 14, 15, 11, 0, 3, 15, 11, 4, 7, 8, 16},
		{4, 7, 11, 19, 4, 9, 11, 19, 4, 9, 17, 19, 6, 5, 11, 19, 9, 5, 11, 17, 19, 5, 11, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 4, 9, 17, 6, 4, 10, 17, 6, 10, 17, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 10, 6, 18, 4, 9, 10, 18, 4, 9, 18, 17, 0, 8, 3, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 0, 1, 13, 10, 0, 18, 13, 10, 0, 18, 6, 6, 4, 0, 18, 17, 4, 0, 18, 17, 13, 0, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 3, 1, 14, 10, 3, 15, 14, 6, 4, 10, 3, 6, 10, 3, 15, 6, 3, 19, 15, 6, 3, 4, 19, 4, 3, 8, 19, 4, 8, 16, 19},
		{1, 4, 9, 17, 1, 4, 14, 17, 1, 4, 14, 2, 2, 6, 4, 14, 18, 6, 4, 14, 18, 17, 4, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 0, 8, 12, 1, 4, 9, 17, 1, 4, 14, 17, 1, 4, 14, 2, 2, 6, 4, 14, 18, 6, 4, 14, 18, 17, 4, 14, -1, -1, -1, -1},
		{0, 4, 2, 15, 0, 4, 12, 15, 16, 4, 12, 15, 2, 4, 6, 15, 19, 4, 6, 15, 19, 4, 16, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 2, 3, 15, 8, 2, 16, 15, 8, 2, 16, 4, 4, 6, 2, 16, 19, 6, 2, 16, 19, 15, 2, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 4, 9, 17, 10, 6, 4, 17, 10, 6, 17, 18, 11, 2, 3, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 8, 11, 15, 2, 0, 8, 15, 8, 0, 15, 12, 10, 4, 9, 17, 6, 4, 10, 17, 6, 10, 17, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 2, 3, 15, 10, 0, 1, 13, 10, 0, 18, 13, 10, 0, 18, 6, 6, 4, 0, 18, 17, 4, 0, 18, 17, 13, 0, 18, -1, -1, -1, -1},
		{6, 4, 8, 16, 6, 11, 8, 16, 6, 11, 16, 19, 6, 4, 8, 1, 6, 11, 8, 1, 6, 1, 10, 14, 6, 11, 1, 14, 2, 11, 1, 14},
		{1, 9, 3, 13, 12, 9, 3, 13, 12, 9, 3, 4, 12, 16, 3, 4, 9, 3, 4, 6, 16, 3, 4, 6, 16, 3, 11, 6, 16, 19, 11, 6},
		{8, 11, 6, 19, 8, 4, 6, 19, 8, 4, 19, 16, 8, 11, 6, 1, 8, 4, 6, 1, 8, 1, 0, 13, 8, 4, 1, 13, 9, 4, 1, 13},
		{3, 6, 11, 19, 3, 6, 12, 19, 3, 6, 12, 0, 0, 4, 6, 12, 16, 4, 6, 12, 16, 19, 6, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 11, 6, 19, 4, 8, 6, 19, 4, 8, 19, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 10, 6, 18, 7, 10, 16, 18, 7, 10, 16, 8, 8, 9, 10, 16, 17, 9, 10, 16, 17, 18, 10, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 6, 3, 19, 15, 6, 3, 19, 15, 6, 3, 10, 15, 14, 3, 10, 6, 3, 10, 9, 14, 3, 10, 9, 14, 3, 0, 9, 14, 13, 0, 9},
		{7, 8, 0, 16, 7, 17, 0, 16, 7, 17, 0, 6, 1, 17, 0, 6, 1, 17, 0, 13, 1, 17, 18, 6, 1, 10, 18, 6, -1, -1, -1, -1},
		{10, 7, 6, 19, 10, 7, 14, 19, 10, 7, 14, 1, 1, 3, 7, 14, 15, 3, 7, 14, 15, 19, 7, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 8, 9, 13, 1, 8, 12, 13, 2, 6, 1, 8, 2, 1, 8, 12, 2, 8, 15, 12, 2, 8, 6, 15, 6, 8, 7, 15, 6, 7, 19, 15},
		{2, 6, 7, 19, 2, 3, 7, 19, 2, 3, 19, 15, 2, 6, 7, 9, 2, 3, 7, 9, 2, 9, 1, 13, 2, 3, 9, 13, 0, 3, 9, 13},
		{7, 0, 8, 12, 7, 0, 19, 12, 7, 0, 19, 6, 6, 2, 0, 19, 15, 2, 0, 19, 15, 12, 0, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 3, 2, 15, 6, 7, 2, 15, 6, 7, 15, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 11, 2, 15, 7, 10, 6, 18, 7, 10, 16, 18, 7, 10, 16, 8, 8, 9, 10, 16, 17, 9, 10, 16, 17, 18, 10, 16, -1, -1, -1, -1},
		{2, 0, 9, 13, 2, 10, 9, 13, 2, 10, 13, 14, 2, 0, 9, 7, 2, 10, 9, 7, 2, 7, 11, 19, 2, 10, 7, 19, 6, 10, 7, 19},
		{0, 1, 10, 13, 0, 17, 10, 13, 0, 17, 10, 8, 6, 17, 10, 8, 6, 17, 10, 18, 6, 17, 16, 8, 6, 7, 16, 8, 2, 3, 11, 15},
		{10, 6, 7, 19, 10, 1, 7, 19, 10, 1, 14, 19, 11, 2, 7, 19, 1, 2, 7, 14, 19, 2, 7, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 9, 1, 13, 8, 3, 1, 13, 8, 3, 13, 12, 8, 9, 1, 6, 8, 3, 1, 6, 8, 6, 7, 19, 8, 3, 6, 19, 11, 3, 6, 19},
		{6, 7, 11, 19, 1, 0, 9, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 11, 6, 19, 3, 0, 6, 19, 3, 0, 12, 19, 7, 8, 6, 19, 0, 8, 6, 12, 19, 8, 6, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 7, 11, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 6, 11, 19, 8, 3, 0, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 19, 1, 9, 0, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 1, 9, 13, 8, 3, 1, 13, 8, 3, 13, 12, 11, 7, 6, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 7, 6, 19, 2, 10, 1, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 14, 3, 0, 8, 12, 6, 11, 7, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 9, 0, 13, 2, 10, 9, 13, 2, 10, 13, 14, 6, 11, 7, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 19, 2, 8, 3, 12, 2, 8, 14, 12, 2, 8, 14, 10, 10, 9, 8, 14, 13, 9, 8, 14, 13, 12, 8, 14, -1, -1, -1, -1},
		{7, 2, 3, 15, 6, 2, 7, 15, 6, 7, 15, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 0, 8, 12, 7, 0, 19, 12, 7, 0, 19, 6, 6, 2, 0, 19, 15, 2, 0, 19, 15, 12, 0, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 7, 6, 19, 2, 3, 7, 19, 2, 3, 19, 15, 0, 1, 9, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 9, 8, 16, 7, 9, 17, 16, 6, 2, 7, 9, 6, 7, 9, 17, 6, 9, 18, 17, 6, 9, 2, 18, 2, 9, 1, 18, 2, 1, 14, 18},
		{10, 7, 6, 19, 10, 7, 14, 19, 10, 7, 14, 1, 1, 3, 7, 14, 15, 3, 7, 14, 15, 19, 7, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 7, 12, 0, 15, 7, 12, 0, 15, 7, 1, 6, 15, 7, 1, 6, 15, 7, 19, 6, 15, 14, 1, 6, 10, 14, 1, -1, -1, -1, -1},
		{3, 0, 7, 12, 16, 0, 7, 12, 16, 0, 7, 9, 16, 17, 7, 9, 0, 7, 9, 10, 17, 7, 9, 10, 17, 7, 6, 10, 17, 18, 6, 10},
		{7, 10, 6, 18, 7, 10, 16, 18, 7, 10, 16, 8, 8, 9, 10, 16, 17, 9, 10, 16, 17, 18, 10, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 6, 11, 19, 4, 6, 8, 19, 4, 8, 19, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 6, 11, 19, 3, 6, 12, 19, 3, 6, 12, 0, 0, 4, 6, 12, 16, 4, 6, 12, 16, 19, 6, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 6, 11, 19, 8, 4, 6, 19, 8, 4, 19, 16, 9, 0, 1, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 9, 6, 17, 18, 9, 6, 17, 18, 9, 6, 1, 18, 14, 6, 1, 9, 6, 1, 3, 14, 6, 1, 3, 14, 6, 11, 3, 14, 15, 11, 3},
		{6, 8, 4, 16, 6, 11, 8, 16, 6, 11, 16, 19, 2, 10, 1, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 10, 14, 3, 6, 11, 19, 3, 6, 12, 19, 3, 6, 12, 0, 0, 4, 6, 12, 16, 4, 6, 12, 16, 19, 6, 12, -1, -1, -1, -1},
		{4, 11, 8, 16, 4, 6, 11, 16, 11, 6, 16, 19, 9, 2, 10, 14, 0, 2, 9, 14, 0, 9, 14, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 9, 4, 17, 10, 6, 4, 17, 10, 6, 17, 18, 10, 9, 4, 3, 10, 6, 4, 3, 10, 3, 2, 15, 10, 6, 3, 15, 11, 6, 3, 15},
		{8, 2, 3, 15, 8, 2, 16, 15, 8, 2, 16, 4, 4, 6, 2, 16, 19, 6, 2, 16, 19, 15, 2, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 0, 6, 18, 4, 0, 17, 18, 13, 0, 17, 18, 6, 0, 2, 18, 14, 0, 2, 18, 14, 0, 13, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 1, 13, 8, 2, 3, 15, 8, 2, 16, 15, 8, 2, 16, 4, 4, 6, 2, 16, 19, 6, 2, 16, 19, 15, 2, 16, -1, -1, -1, -1},
		{1, 4, 9, 17, 1, 4, 14, 17, 1, 4, 14, 2, 2, 6, 4, 14, 18, 6, 4, 14, 18, 17, 4, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 1, 3, 12, 8, 1, 13, 12, 4, 6, 8, 1, 4, 8, 1, 13, 4, 1, 17, 13, 4, 1, 6, 17, 6, 1, 10, 17, 6, 10, 18, 17},
		{10, 0, 1, 13, 10, 0, 18, 13, 10, 0, 18, 6, 6, 4, 0, 18, 17, 4, 0, 18, 17, 13, 0, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 6, 10, 18, 4, 9, 10, 18, 4, 9, 18, 17, 4, 6, 10, 3, 4, 9, 10, 3, 4, 3, 8, 12, 4, 9, 3, 12, 0, 9, 3, 12},
		{10, 9, 4, 17, 6, 10, 4, 17, 6, 10, 17, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 19, 5, 4, 9, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 4, 9, 17, 7, 6, 11, 19, 0, 8, 3, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 0, 1, 13, 5, 4, 0, 13, 5, 4, 13, 17, 7, 6, 11, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 7, 6, 19, 8, 5, 4, 17, 8, 5, 12, 17, 8, 5, 12, 3, 3, 1, 5, 12, 13, 1, 5, 12, 13, 17, 5, 12, -1, -1, -1, -1},
		{7, 6, 11, 19, 5, 4, 9, 17, 2, 10, 1, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 7, 19, 1, 2, 10, 14, 0, 8, 3, 12, 4, 9, 5, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{7, 6, 11, 19, 5, 2, 10, 14, 5, 2, 17, 14, 5, 2, 17, 4, 4, 0, 2, 17, 13, 0, 2, 17, 13, 14, 2, 17, -1, -1, -1, -1},
		{2, 10, 5, 14, 2, 13, 5, 14, 2, 13, 5, 3, 4, 13, 5, 3, 4, 13, 5, 17, 4, 13, 12, 3, 4, 8, 12, 3, 6, 11, 7, 19},
		{7, 2, 3, 15, 7, 6, 2, 15, 7, 6, 15, 19, 5, 4, 9, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 4, 9, 17, 7, 0, 8, 12, 7, 0, 19, 12, 7, 0, 19, 6, 6, 2, 0, 19, 15, 2, 0, 19, 15, 12, 0, 19, -1, -1, -1, -1},
		{6, 3, 7, 19, 6, 2, 3, 19, 3, 2, 19, 15, 5, 0, 1, 13, 4, 0, 5, 13, 4, 5, 13, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 2, 1, 14, 6, 5, 1, 14, 6, 5, 14, 18, 6, 2, 1, 8, 6, 5, 1, 8, 6, 8, 7, 16, 6, 5, 8, 16, 4, 5, 8, 16},
		{9, 5, 4, 17, 10, 7, 6, 19, 10, 7, 14, 19, 10, 7, 14, 1, 1, 3, 7, 14, 15, 3, 7, 14, 15, 19, 7, 14, -1, -1, -1, -1},
		{0, 8, 7, 12, 0, 15, 7, 12, 0, 15, 7, 1, 6, 15, 7, 1, 6, 15, 7, 19, 6, 15, 14, 1, 6, 10, 14, 1, 4, 9, 5, 17},
		{4, 0, 3, 12, 4, 7, 3, 12, 4, 7, 12, 16, 4, 0, 3, 10, 4, 7, 3, 10, 4, 10, 5, 18, 4, 7, 10, 18, 6, 7, 10, 18},
		{5, 4, 8, 16, 5, 10, 8, 16, 5, 10, 18, 16, 7, 6, 8, 16, 10, 6, 8, 18, 16, 6, 8, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 9, 5, 17, 6, 9, 19, 17, 6, 9, 19, 11, 11, 8, 9, 19, 16, 8, 9, 19, 16, 17, 9, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 11, 3, 19, 6, 16, 3, 19, 6, 16, 3, 5, 0, 16, 3, 5, 0, 16, 3, 12, 0, 16, 17, 5, 0, 9, 17, 5, -1, -1, -1, -1},
		{0, 11, 8, 12, 0, 11, 15, 12, 1, 5, 0, 11, 1, 0, 11, 15, 1, 11, 14, 15, 1, 11, 5, 14, 5, 11, 6, 14, 5, 6, 18, 14},
		{6, 3, 11, 15, 6, 3, 18, 15, 6, 3, 18, 5, 5, 1, 3, 18, 14, 1, 3, 18, 14, 15, 3, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 10, 1, 14, 6, 9, 5, 17, 6, 9, 19, 17, 6, 9, 19, 11, 11, 8, 9, 19, 16, 8, 9, 19, 16, 17, 9, 19, -1, -1, -1, -1},
		{6, 11, 3, 19, 6, 16, 3, 19, 6, 16, 3, 5, 0, 16, 3, 5, 0, 16, 3, 12, 0, 16, 17, 5, 0, 9, 17, 5, 2, 10, 1, 14},
		{11, 8, 0, 12, 11, 2, 0, 12, 11, 2, 12, 15, 11, 8, 0, 5, 11, 2, 0, 5, 11, 5, 6, 18, 11, 2, 5, 18, 10, 2, 5, 18},
		{6, 11, 3, 15, 6, 5, 3, 15, 6, 5, 18, 15, 2, 10, 3, 15, 5, 10, 3, 18, 15, 10, 3, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{6, 5, 2, 18, 14, 5, 2, 18, 14, 5, 2, 9, 14, 13, 2, 9, 5, 2, 9, 8, 13, 2, 9, 8, 13, 2, 3, 8, 13, 12, 3, 8},
		{9, 6, 5, 18, 9, 6, 13, 18, 9, 6, 13, 0, 0, 2, 6, 13, 14, 2, 6, 13, 14, 18, 6, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 5, 6, 18, 1, 2, 6, 18, 1, 2, 18, 14, 1, 5, 6, 8, 1, 2, 6, 8, 1, 8, 0, 12, 1, 2, 8, 12, 3, 2, 8, 12},
		{1, 5, 6, 18, 2, 1, 6, 18, 2, 1, 18, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 3, 8, 12, 1, 9, 8, 12, 1, 9, 12, 13, 1, 3, 8, 6, 1, 9, 8, 6, 1, 6, 10, 18, 1, 9, 6, 18, 5, 9, 6, 18},
		{10, 1, 0, 13, 10, 6, 0, 13, 10, 6, 18, 13, 9, 5, 0, 13, 6, 5, 0, 18, 13, 5, 0, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 3, 8, 12, 5, 6, 10, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 5, 6, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 11, 7, 19, 10, 11, 5, 19, 10, 5, 19, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 5, 10, 18, 11, 7, 5, 18, 11, 7, 18, 19, 8, 3, 0, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 11, 7, 19, 5, 10, 11, 19, 5, 10, 19, 18, 1, 9, 0, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 7, 5, 18, 10, 11, 7, 18, 7, 11, 18, 19, 1, 8, 3, 12, 9, 8, 1, 12, 9, 1, 12, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 1, 2, 14, 11, 1, 19, 14, 11, 1, 19, 7, 7, 5, 1, 19, 18, 5, 1, 19, 18, 14, 1, 19, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 3, 0, 12, 11, 1, 2, 14, 11, 1, 19, 14, 11, 1, 19, 7, 7, 5, 1, 19, 18, 5, 1, 19, 18, 14, 1, 19, -1, -1, -1, -1},
		{9, 7, 5, 17, 9, 7, 16, 17, 0, 2, 9, 7, 0, 9, 7, 16, 0, 7, 12, 16, 0, 7, 2, 12, 2, 7, 11, 12, 2, 11, 15, 12},
		{7, 5, 9, 17, 7, 8, 9, 17, 7, 8, 17, 16, 7, 5, 9, 2, 7, 8, 9, 2, 7, 2, 11, 15, 7, 8, 2, 15, 3, 8, 2, 15},
		{2, 5, 10, 18, 2, 5, 15, 18, 2, 5, 15, 3, 3, 7, 5, 15, 19, 7, 5, 15, 19, 18, 5, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 10, 0, 14, 13, 10, 0, 14, 13, 10, 0, 5, 13, 17, 0, 5, 10, 0, 5, 7, 17, 0, 5, 7, 17, 0, 8, 7, 17, 16, 8, 7},
		{0, 1, 9, 13, 2, 5, 10, 18, 2, 5, 15, 18, 2, 5, 15, 3, 3, 7, 5, 15, 19, 7, 5, 15, 19, 18, 5, 15, -1, -1, -1, -1},
		{9, 8, 7, 16, 9, 5, 7, 16, 9, 5, 16, 17, 9, 8, 7, 2, 9, 5, 7, 2, 9, 2, 1, 14, 9, 5, 2, 14, 10, 5, 2, 14},
		{1, 5, 3, 12, 1, 5, 13, 12, 17, 5, 13, 12, 3, 5, 7, 12, 16, 5, 7, 12, 16, 5, 17, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 7, 8, 16, 0, 7, 13, 16, 0, 7, 13, 1, 1, 5, 7, 13, 17, 5, 7, 13, 17, 16, 7, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 3, 0, 12, 9, 3, 17, 12, 9, 3, 17, 5, 5, 7, 3, 17, 16, 7, 3, 17, 16, 12, 3, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 8, 7, 16, 5, 9, 7, 16, 5, 9, 16, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 8, 4, 16, 5, 8, 18, 16, 5, 8, 18, 10, 10, 11, 8, 18, 19, 11, 8, 18, 19, 16, 8, 18, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 10, 11, 15, 3, 10, 14, 15, 0, 4, 3, 10, 0, 3, 10, 14, 0, 10, 13, 14, 0, 10, 4, 13, 4, 10, 5, 13, 4, 5, 17, 13},
		{1, 9, 0, 13, 5, 8, 4, 16, 5, 8, 18, 16, 5, 8, 18, 10, 10, 11, 8, 18, 19, 11, 8, 18, 19, 16, 8, 18, -1, -1, -1, -1},
		{10, 11, 3, 15, 10, 1, 3, 15, 10, 1, 15, 14, 10, 11, 3, 4, 10, 1, 3, 4, 10, 4, 5, 17, 10, 1, 4, 17, 9, 1, 4, 17},
		{11, 2, 8, 15, 12, 2, 8, 15, 12, 2, 8, 1, 12, 13, 8, 1, 2, 8, 1, 5, 13, 8, 1, 5, 13, 8, 4, 5, 13, 17, 4, 5},
		{0, 4, 5, 17, 0, 1, 5, 17, 0, 1, 17, 13, 0, 4, 5, 11, 0, 1, 5, 11, 0, 11, 3, 15, 0, 1, 11, 15, 2, 1, 11, 15},
		{0, 2, 11, 15, 0, 8, 11, 15, 0, 8, 15, 12, 0, 2, 11, 5, 0, 8, 11, 5, 0, 5, 9, 17, 0, 8, 5, 17, 4, 8, 5, 17},
		{2, 11, 3, 15, 9, 4, 5, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 8, 15, 2, 19, 8, 15, 2, 19, 8, 10, 4, 19, 8, 10, 4, 19, 8, 16, 4, 19, 18, 10, 4, 5, 18, 10, -1, -1, -1, -1},
		{5, 2, 10, 14, 5, 2, 17, 14, 5, 2, 17, 4, 4, 0, 2, 17, 13, 0, 2, 17, 13, 14, 2, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 8, 15, 2, 19, 8, 15, 2, 19, 8, 10, 4, 19, 8, 10, 4, 19, 8, 16, 4, 19, 18, 10, 4, 5, 18, 10, 0, 1, 9, 13},
		{1, 9, 4, 17, 1, 2, 4, 17, 1, 2, 14, 17, 5, 10, 4, 17, 2, 10, 4, 14, 17, 10, 4, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{8, 5, 4, 17, 8, 5, 12, 17, 8, 5, 12, 3, 3, 1, 5, 12, 13, 1, 5, 12, 13, 17, 5, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 1, 0, 13, 4, 5, 0, 13, 4, 5, 13, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 0, 3, 12, 9, 5, 3, 12, 9, 5, 17, 12, 8, 4, 3, 12, 5, 4, 3, 17, 12, 4, 3, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{5, 9, 4, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 11, 7, 19, 4, 11, 17, 19, 4, 11, 17, 9, 9, 10, 11, 17, 18, 10, 11, 17, 18, 19, 11, 17, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 3, 12, 4, 11, 7, 19, 4, 11, 17, 19, 4, 11, 17, 9, 9, 10, 11, 17, 18, 10, 11, 17, 18, 19, 11, 17, -1, -1, -1, -1},
		{10, 1, 11, 14, 15, 1, 11, 14, 15, 1, 11, 0, 15, 12, 11, 0, 1, 11, 0, 4, 12, 11, 0, 4, 12, 11, 7, 4, 12, 16, 7, 4},
		{3, 1, 10, 14, 3, 11, 10, 14, 3, 11, 14, 15, 3, 1, 10, 4, 3, 11, 10, 4, 3, 4, 8, 16, 3, 11, 4, 16, 7, 11, 4, 16},
		{11, 7, 4, 19, 11, 18, 4, 19, 11, 18, 4, 2, 9, 18, 4, 2, 9, 18, 4, 17, 9, 18, 14, 2, 9, 1, 14, 2, -1, -1, -1, -1},
		{4, 9, 1, 17, 4, 18, 1, 17, 4, 18, 1, 7, 2, 18, 1, 7, 2, 18, 1, 14, 2, 18, 19, 7, 2, 11, 19, 7, 0, 8, 3, 12},
		{11, 4, 7, 16, 11, 4, 15, 16, 11, 4, 15, 2, 2, 0, 4, 15, 12, 0, 4, 15, 12, 16, 4, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 7, 4, 16, 11, 2, 4, 16, 11, 2, 15, 16, 8, 3, 4, 16, 2, 3, 4, 15, 16, 3, 4, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 3, 7, 16, 4, 3, 12, 16, 9, 10, 4, 3, 9, 4, 3, 12, 9, 3, 13, 12, 9, 3, 10, 13, 10, 3, 2, 13, 10, 2, 14, 13},
		{9, 10, 2, 14, 9, 0, 2, 14, 9, 0, 14, 13, 9, 10, 2, 7, 9, 0, 2, 7, 9, 7, 4, 16, 9, 0, 7, 16, 8, 0, 7, 16},
		{3, 7, 4, 16, 3, 0, 4, 16, 3, 0, 16, 12, 3, 7, 4, 10, 3, 0, 4, 10, 3, 10, 2, 14, 3, 0, 10, 14, 1, 0, 10, 14},
		{4, 8, 7, 16, 10, 2, 1, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 1, 9, 13, 4, 1, 16, 13, 4, 1, 16, 7, 7, 3, 1, 16, 12, 3, 1, 16, 12, 13, 1, 16, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 8, 7, 16, 0, 1, 7, 16, 0, 1, 13, 16, 4, 9, 7, 16, 1, 9, 7, 13, 16, 9, 7, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 7, 4, 16, 0, 3, 4, 16, 0, 3, 16, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{4, 8, 7, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 9, 11, 15, 10, 9, 14, 15, 13, 9, 14, 15, 11, 9, 8, 15, 12, 9, 8, 15, 12, 9, 13, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{3, 9, 0, 13, 3, 9, 15, 13, 3, 9, 15, 11, 11, 10, 9, 15, 14, 10, 9, 15, 14, 13, 9, 15, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 10, 1, 14, 0, 10, 12, 14, 0, 10, 12, 8, 8, 11, 10, 12, 15, 11, 10, 12, 15, 14, 10, 12, -1, -1, -1, -1, -1, -1, -1, -1},
		{10, 11, 3, 15, 1, 10, 3, 15, 1, 10, 15, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 11, 2, 15, 1, 11, 13, 15, 1, 11, 13, 9, 9, 8, 11, 13, 12, 8, 11, 13, 12, 15, 11, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 2, 11, 15, 1, 9, 11, 15, 1, 9, 13, 15, 3, 0, 11, 15, 9, 0, 11, 13, 15, 0, 11, 13, -1, -1, -1, -1, -1, -1, -1, -1},
		{11, 8, 0, 12, 2, 11, 0, 12, 2, 11, 12, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 11, 3, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 8, 3, 12, 2, 8, 14, 12, 2, 8, 14, 10, 10, 9, 8, 14, 13, 9, 8, 14, 13, 12, 8, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 10, 2, 14, 0, 9, 2, 14, 0, 9, 14, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{2, 3, 8, 12, 2, 10, 8, 12, 2, 10, 14, 12, 0, 1, 8, 12, 10, 1, 8, 14, 12, 1, 8, 14, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 10, 2, 14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{1, 3, 8, 12, 9, 1, 8, 12, 9, 1, 12, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{9, 1, 0, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{0, 3, 8, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
		{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	};

	//definition for which configuration is an inverted case or not, i.e. 1 for mostly dirt in which you would calculate volume with 1 - sum (tetra volumes)
	private int[] CaseInversionTable = new int[] {
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		1,
		0,
		1,
		1,
		0,
		0,
		1,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		0,
		1,
		1,
		1,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		1,
		0,
		1,
		1,
		1,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		1,
		1,
		1,
		1,
		1,
		1,
		0,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		0,
		1,
		0,
		0,
		0,
		0,
		0,
		1,
		1,
		0,
		0,
		1,
		1,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
	};
}
