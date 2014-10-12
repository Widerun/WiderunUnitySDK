using UnityEditor;
using UnityEngine;
public class EditorWizard : ScriptableWizard {
	public float seed = 12345f;

	[Range(-1, 1)]
	public float leftHeight = 0;
	[Range(-1, 1)]
	public float rightHeight = 0;


	private string heightsFile = "";
	private string terrainsPath;
	private Terrain[] sceneTerrains;
	private int chunksCount;
	
	private readonly int chunkSize = 500;
	private readonly int heightmapResolution = 257;


	[MenuItem ("Widerun/Create Terrain")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<EditorWizard>("Create Widerun terrain", "Create Terrains");
	}

	void OnWizardCreate () {
		terrainsPath = EditorUtility.OpenFolderPanel("Terrain directory", Application.dataPath, "");
		terrainsPath = "Assets"+terrainsPath.Substring(Application.dataPath.Length);


		heightsFile = EditorUtility.OpenFilePanel("Select heights definitions", Application.dataPath, "");
		PathReader path = new PathReader (heightsFile);

		float pathLength = path.GetMaxValue ();
		chunksCount = ((int)(pathLength+1)) / chunkSize;

		GenerateTerrains (chunksCount);

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

}