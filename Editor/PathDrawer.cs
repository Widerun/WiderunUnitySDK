using UnityEditor;
using UnityEngine;
public class PathDrawer : ScriptableWizard {

	public Texture2D pathTexture;
	public Texture2D normalMap;
	public int pathWidth = 4;
	public int tileSize = 16;

	private Terrain[] sceneTerrains;

	[MenuItem ("Widerun/Draw Path")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<PathDrawer>("Create path", "Create Path");
	}

	void OnWizardCreate () {

		sceneTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];

		SplatPrototype pathSlatPrototype = makeSplatPrototype (pathTexture, normalMap); 

		for (int t = 0; t < sceneTerrains.Length; t++) {
			Terrain terrain = sceneTerrains[t];
			SplatPrototype[] texturesArray = terrain.terrainData.splatPrototypes;

			TerrainData data = terrain.terrainData;

			float [,,] alphamapLayers = data.GetAlphamaps (0, 0, data.alphamapWidth, data.alphamapHeight);


			/**********************
			 * Get the texture position
			 *  - it is added if  not present
			 * *******************/
			SplatPrototype[] newTexturesArray;
			int currentSplatPosition = getCurrentSplatPosition(texturesArray);

			if(currentSplatPosition<0) {
				newTexturesArray = new SplatPrototype[1+texturesArray.Length];
				for(int i = 0; i < texturesArray.Length ; i++) {
					newTexturesArray[i] = texturesArray[i];
				}
				alphamapLayers = addAlphamapsLayer(terrain, newTexturesArray.Length);
				currentSplatPosition = texturesArray.Length;
			}
			else 
				newTexturesArray = texturesArray;

		
			/**************
			 * Set the new textures array
			 * ***********/
			newTexturesArray[currentSplatPosition] = pathSlatPrototype;
			terrain.terrainData.splatPrototypes = newTexturesArray;

			/**************
			 * Draw path and assign the h-map to the terrain
			 * ***********/
			terrain.terrainData.SetAlphamaps(0,0, setPathLevel(alphamapLayers, currentSplatPosition));




		}


	}



	/******************
	 * Draw the path
	 * ***************/
	private float[,,] setPathLevel(float[,,] alphamapLayers, int layerIndex) {

		int pathOffset = (alphamapLayers.GetLength (0) - pathWidth) / 2;

		for (int x = 0; x < alphamapLayers.GetLength(1); x++) {
			for (int z = 0; z < alphamapLayers.GetLength(0); z++) {
				alphamapLayers[z,x,layerIndex] = 0;
			}
		}
		for (int x = 0; x < pathWidth; x++) {
			for (int z = 0; z < alphamapLayers.GetLength(0); z++) {
				alphamapLayers[z,x+pathOffset,layerIndex] = 100;
			}
		}

		return normalize(alphamapLayers);
	}

	/******************
	 * Normalize the sum of the layers to 1
	 * ***************/
	private float[,,] normalize(float[,,] heightsMap) {
		for (int x = 0; x < heightsMap.GetLength(0); x++) {
			for (int z = 0; z < heightsMap.GetLength(1); z++) {
				float total = 0f;
				for (int l = 0; l < heightsMap.GetLength(2); l++) {
					total+=heightsMap[x,z,l];
				}
				for (int l = 0; l < heightsMap.GetLength(2); l++) {
					heightsMap[x,z,l]=heightsMap[x,z,l]/total;
				}
			}
		}
		return heightsMap;
	}



	/********************
	 * Add a layer at the end of the array
	 * *****************/
	private float[,,] addAlphamapsLayer(Terrain terrain, int layersNumber) {
		TerrainData data = terrain.terrainData;

		float[,,] alphaMaps = data.GetAlphamaps (0, 0, data.alphamapWidth, data.alphamapHeight);

		int h = data.alphamapHeight;
		int w = data.alphamapWidth;

		float[,,] newAlphaMaps = new float[w, h, layersNumber];

		for (int l = 0; l < layersNumber-1; l++) {
			for (int x = 0; x < h; x++) {
				for (int z = 0; z < w; z++) {
					newAlphaMaps[z,x,l] = alphaMaps[z,x,l];
				}
			}
		}

		return newAlphaMaps;
	
	}



	/****************************
	 * Get the position of the SplatPrototype
	 * It returns -1 if it does not exist
	 * **************************/
	private int getCurrentSplatPosition(SplatPrototype[] texturesArray) {
		int currentSplatPosition = -1;
		for (int sp = 0 ; sp < texturesArray.Length ; sp++) {
			if (texturesArray[sp].texture.Equals(pathTexture))
				currentSplatPosition = sp;
		}
		return currentSplatPosition;
	}

	private SplatPrototype makeSplatPrototype(Texture2D pathTexture, Texture2D pathTextureNormal) {
		SplatPrototype prt = new SplatPrototype ();
		prt.texture = pathTexture;
		prt.normalMap = pathTextureNormal;
		prt.tileSize = new Vector2(tileSize, tileSize);
		prt.tileOffset = new Vector2(0, 0);
		return prt;
	}



}