using UnityEditor;
using UnityEngine;
public class EditorWizard : ScriptableWizard {
	public float seed = 12345f;

	[Range(-1, 1)]
	public float leftHeight = 0;
	[Range(-1, 1)]
	public float rightHeight = 0;

	public int pathWidth = 2;

	[Range(0, 1)]
	public float noiseReduction = 0.1f;


	private string heightsFile = "";
	private string terrainsPath;
	private Terrain[] sceneTerrains;
	private int chunksCount;
	
	private static readonly int chunkSize = 500;
	private static readonly int heightmapResolution = 257;
	private static readonly float hm_size_ratio = (float)heightmapResolution / (float)chunkSize;
	public static int RealPositionToHeightmap (float pos) {
		return (int)(hm_size_ratio * pos);
	}


	[MenuItem ("Widerun/Create Terrain")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<EditorWizard>("Create Widerun terrain", "Create Terrains", "Edit currents");
	}

	void OnWizardCreate () {
		terrainsPath = EditorUtility.OpenFolderPanel("Terrain directory", Application.dataPath, "");
		terrainsPath = "Assets"+terrainsPath.Substring(Application.dataPath.Length);
		Apply (true);
	}

	void OnWizardOtherButton () {
		Apply (false);
	}


	void Apply (bool createAssets) {


		heightsFile = EditorUtility.OpenFilePanel("Select heights definitions file", Application.dataPath, "");
		PathReader path = new PathReader (heightsFile);

		float pathLength = path.GetMaxValue ();
		chunksCount = ((int)(pathLength+1)) / chunkSize;

		if(createAssets)
			GenerateTerrains (chunksCount);


		/************
		 * GET TERRAINS
		 ********/
		sceneTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
		if (sceneTerrains.Length == 0 || chunksCount != sceneTerrains.Length) {
			EditorUtility.DisplayDialog ("Error", "The number of terrains does not match input file", "Ok");
			return;
		}
		//ORDER BY Z
		System.Array.Sort(sceneTerrains, zComparator);



		/****************************
		 * Your track is between 25% and 75% of the terrain height 
		 * *************************/
		int maxHeight = path.GetMaxHeight ();
		int terrainsHeight = maxHeight * 2;
		float groundOffset = 1f / 4f;
		//SET TERRAINS NEW HEIGHT
		for (int i = 0; i < sceneTerrains.Length; i++) {
			sceneTerrains[i].terrainData.size = new Vector3 (chunkSize, terrainsHeight, chunkSize);
		}



		/****************
		 * Use an extend heightmap for the coherent noise (not chunked)
		 * **************/
		int extendedHeightmapLength = heightmapResolution*chunksCount;
		float[,] extendedHeightmap = new float[heightmapResolution,extendedHeightmapLength];



		float value = 0f;
		int projectionStart = 0;
		int projectionEnd = 0;

		/*****************
		 * If the height definition does not start from 0,
		 * the terrain is set to the height of the first point
		 * **************/
		int firstPosition = RealPositionToHeightmap (path.points [0].positionProjection);
		for (int z = 0; z < firstPosition; z++) {
			for(int x = 0; x < heightmapResolution; x++) {
				extendedHeightmap[x, z]=path.points [0].height/terrainsHeight+groundOffset;
			}
		}


		/******************************
		 * Set the slopes according to the heights definition
		 * ***************************/
		for (int p = 0; p < path.points.Count-1; p++) {
			PathReader.Point s = path.points[p];
			PathReader.Point e = path.points[p+1];
			
			projectionStart = RealPositionToHeightmap(s.positionProjection);
			projectionEnd = RealPositionToHeightmap(e.positionProjection);
			
			float segmentLength = projectionEnd - projectionStart;
			
			for(int z = projectionStart; z < projectionEnd; z++) {
				value = Mathf.Lerp(s.height, e.height, (float)(z-projectionStart)/segmentLength);
				for(int x = 0; x < heightmapResolution; x++) {
					extendedHeightmap[x, z] = value/terrainsHeight+groundOffset;
				}
				
			}
		}

		/****************************
		 * The segment from the last point and the end of the terrain
		 * is set to the last height
		 * *************************/
		for (int z = projectionEnd; z < extendedHeightmapLength; z++) {
			for(int x = 0; x < heightmapResolution; x++) {
				extendedHeightmap[x, z]=value/terrainsHeight+groundOffset;
			}
		}

	}


	void OnWizardUpdate () {
		helpString = "Please set the terrain details!";
	}




	/***********************************/
	private void GenerateTerrains(int n)
	{
		for(int i = 0; i < n ; i++) {
			var _terrainData = new TerrainData();
			var _terrain = Terrain.CreateTerrainGameObject (_terrainData);
			_terrain.name = "TerrainChunk"+(i*chunkSize)+"-"+((1+i)*chunkSize);
			
			_terrainData.heightmapResolution = heightmapResolution;
			_terrainData.size = new Vector3 (chunkSize, 1, chunkSize);
			_terrainData.baseMapResolution = 256;
			_terrainData.SetDetailResolution(1024,8);
			_terrainData.alphamapResolution = 256;
			
			_terrain.transform.position=new Vector3(0, 0, i*chunkSize);



			AssetDatabase.CreateAsset(_terrainData, terrainsPath+"/"+_terrain.name+".asset");
			
			
		}
	}
	/**************
	 * Compare terrain based on z value (used in sorting)
	 * ************/
	private int zComparator (Terrain a , Terrain b ) {
		if (a.GetPosition().z > b.GetPosition().z) {
			return 1;
		}
		else if (a.GetPosition().z < b.GetPosition().z) {
			return -1;
		}
		return 0;
	}


	private float[,] getTerrainHeightmap(float[,] extended, int index) {
		float[,] result = new float[heightmapResolution,heightmapResolution];
		for (int i=0; i<heightmapResolution; i++) {
			for (int j=0; j<heightmapResolution; j++) {
				result[j,i]=extended[i, j+(index*(heightmapResolution-1))];
			}
		}
		return result;
	}


}