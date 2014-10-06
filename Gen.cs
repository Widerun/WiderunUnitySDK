using UnityEngine;
using System.Collections;
using UnityEditor;
public class Gen : MonoBehaviour {

	public static int chunkSize = 500;
	public static int heightmapResolution = 257;

	public static int terrainsCount = 21;
	public static int extendedHeightmapLength = heightmapResolution*terrainsCount;

	static Terrain[] sceneTerrains;

	//public static int pathWidth = 6;

	public static float hm_size_ratio = (float)heightmapResolution / (float)chunkSize;
	public static int RealPositionToHeightmap (float pos) {
		return (int)(hm_size_ratio * pos);
	}


	[MenuItem("Widerun/GEN")]
	public static void CreateWizard(MenuCommand command)
	{

		Generate ();

		PathReader path = new PathReader ("path.csv");

		int maxHeight = path.GetMaxHeight ();

		int terrainsHeight = maxHeight * 2;

		float groundOffset = 1f / 4f;

		//GET TERRAINS ORDERED
		sceneTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
		if (sceneTerrains.Length == 0) {
			return;
		}
		System.Array.Sort(sceneTerrains, SortByZ);

		//SET TERRAINS NEW HEIGHT
		for (int i = 0; i < sceneTerrains.Length; i++) {
			sceneTerrains[i].terrainData.size = new Vector3 (chunkSize, terrainsHeight, chunkSize);
		}

		
		float[,] extendedHeightmap = new float[heightmapResolution,extendedHeightmapLength];

		float value = 0f;
		int projectionStart = 0;
		int projectionEnd = 0;
		//START SEGMENT
		int firstPosition = RealPositionToHeightmap (path.points [0].positionProjection);
		for (int z = 0; z < firstPosition; z++) {
			for(int x = 0; x < heightmapResolution; x++) {
				extendedHeightmap[x, z]=path.points [0].height/terrainsHeight+groundOffset;
			}
		}
		for (int p = 0; p < path.points.Count-1; p++) {
			PathReader.Point s = path.points[p];
			PathReader.Point e = path.points[p+1];

			projectionStart = RealPositionToHeightmap(s.positionProjection);
			projectionEnd = RealPositionToHeightmap(e.positionProjection);

			float segmentLength = projectionEnd - projectionStart;

			for(int z = projectionStart; z < projectionEnd; z++) {
				value = Mathf.Lerp(s.height, e.height, (float)(z-projectionStart)/segmentLength);
				//print ((float)z/segmentLength);
				for(int x = 0; x < heightmapResolution; x++) {
					extendedHeightmap[x, z] = value/terrainsHeight+groundOffset;
				}

			}
			//extendedHeightmap[0, projectionStart]=1;
		}
		print (value);
		//END SEGMENT
		for (int z = projectionEnd; z < extendedHeightmapLength; z++) {
			for(int x = 0; x < heightmapResolution; x++) {
				extendedHeightmap[x, z]=value/terrainsHeight+groundOffset;
			}
		}


		//APPLY NOISE
		float[,] noiseMap = NoiseGenerator.generatePerlin (1289f, heightmapResolution, extendedHeightmapLength);

		int pathWidth = 2;
		float noiseReduction = 0.1f;
		//APPLY THE COMPUTED HEIGHTMAP TO THE TERRAINS
		for (int i = 0; i < sceneTerrains.Length; i++) {
			
			float[,] slopes = getTerrainHeightmap(extendedHeightmap,i);
			float[,] noise = getTerrainHeightmap(noiseMap,i);
			
			float[,] mergedHeightMap = new float[heightmapResolution,heightmapResolution];
			for(int w = 0 ; w < heightmapResolution ; w++) {
				int leftPathBorder = (heightmapResolution/2)-pathWidth;
				int rightPathBorder = (heightmapResolution/2)+pathWidth;
				for(int h = 0 ; h < leftPathBorder ; h++) {
					/*float ratio = Mathf.SmoothStep(1,0,(float)h/(float)leftPathBorder);
					float noiseValue = (noise[w,h])*noiseReduction;
					mergedHeightMap[w,h] = slopes[w,h]+ratio*noiseValue;*/

					float ratio = Mathf.SmoothStep(0.7f,0,(float)h/(float)leftPathBorder);
					float noiseValue = noise[w,h];
					float cliff = Mathf.SmoothStep(0.25f, slopes[w,h],(float)h/(float)leftPathBorder);
					mergedHeightMap[w,h] = cliff-ratio*noiseValue;

				}
				for(int h = rightPathBorder ; h <heightmapResolution  ; h++) {
					float ratio = Mathf.SmoothStep(0,1,(float)(h-pathWidth-leftPathBorder)/(float)(leftPathBorder));
					float noiseValue = (noise[w,h])*noiseReduction;
					mergedHeightMap[w,h] = slopes[w,h]+ratio*noiseValue;
				}
				
				
				for(int h = leftPathBorder ; h < rightPathBorder  ; h++) {
					mergedHeightMap[w,h] = slopes[w,h];
				}
			}
			
			sceneTerrains[i].terrainData.SetHeights(0,0, mergedHeightMap);
		}


		Debug.Log ("DONE");

	}

	
	static float[,] getTerrainHeightmap(float[,] extended, int index) {
		float[,] result = new float[heightmapResolution,heightmapResolution];
		for (int i=0; i<heightmapResolution; i++) {
			for (int j=0; j<heightmapResolution; j++) {
				result[j,i]=extended[i, j+(index*(heightmapResolution-1))];
			}
		}
		return result;
	}
	/***********************************/
	public static void Generate()
	{
		for(int i = 0; i < terrainsCount ; i++) {
			var _terrainData = new TerrainData();
			var _terrain = Terrain.CreateTerrainGameObject (_terrainData);
			_terrain.name = "T"+i;
			
			_terrainData.heightmapResolution = heightmapResolution;
			_terrainData.size = new Vector3 (chunkSize, 1, chunkSize);
			_terrainData.baseMapResolution = 256;
			_terrainData.SetDetailResolution(1024,8);
			_terrainData.alphamapResolution = 256;

			_terrain.transform.position=new Vector3(0, 0, i*chunkSize);
			AssetDatabase.CreateAsset(_terrainData, "Assets/terrains/T"+i+".asset");
			
			
		}
	}
	/***************************/
	static int SortByZ (Terrain a , Terrain b ) {
		if (a.GetPosition().z > b.GetPosition().z) {
			return 1;
		}
		else if (a.GetPosition().z < b.GetPosition().z) {
			return -1;
		}
		return 0;
	}



}
