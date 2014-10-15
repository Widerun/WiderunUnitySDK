using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;

public class TextureSetter : MonoBehaviour {
	
	static Terrain[] sceneTerrains;
	
	static TextureConfig cliff;
	static List<TextureConfig> textures;
	
	[MenuItem("Terrain/APPLY")]
	public static void Apply(MenuCommand command)
	{
		textures = new List<TextureConfig>();
		sceneTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
		
		string json = File.ReadAllText(Application.dataPath + "/textures.json");
		var config = JSON.Parse(json);
		cliff = TextureConfig.make (config["cliff"]);
		for (int i = 0; i < config["textures"].AsArray.Count; i++) {
			textures.Add(TextureConfig.make(config["textures"].AsArray[i]));
		}
		
		
		
		SplatPrototype[] texturesArray = new SplatPrototype[1+textures.Count];
		texturesArray [0] = getSplatPrototype (cliff);
		for (int i = 0; i < textures.Count; i++) {
			texturesArray[i+1] = getSplatPrototype(textures[i]);
		}
		
		
		for (int t = 0; t < sceneTerrains.Length; t++) {
			Terrain terrain = sceneTerrains[t];
			
			terrain.terrainData.splatPrototypes = texturesArray;
			
			float[,,] al = getAlphaLayers(terrain.terrainData/*terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapHeight, terrain.terrainData.heightmapWidth),
			                              terrain.terrainData.alphamapWidth*/);
			//Debug.Log(terrain.terrainData.GetAlphamaps);
			terrain.terrainData.SetAlphamaps(0,0,al);
			
		}
		
		
		
		Debug.Log("DONE");
	}
	
	
	public static float[,,] getAlphaLayers(TerrainData data/*float[,] heightMap, int alphamapSize*/) {

		//COEFFIECIENTS <layerIndex, height> -> streight
		float [,] strengthCoefficient = new float[textures.Count,1000];
		for (int l = 0; l < textures.Count; l++) {
				TextureConfig textureCfg = textures [l];
				for (int h = 0; h < 1000; h++) {

					float h_01 = (float)h/1000f;

					if (textureCfg.from <= h_01 && textureCfg.to >= (float)h_01) {
						strengthCoefficient [l, h] = textureCfg.strength;
					}
					if(textureCfg.from>h_01 && textureCfg.from-h_01<=textureCfg.margins){
						strengthCoefficient [l, h] = Mathf.SmoothStep(0, textureCfg.strength, 1-((textureCfg.from-h_01)/textureCfg.margins));
					}
					if(textureCfg.to<h_01 && h_01-textureCfg.to<=textureCfg.margins){
					strengthCoefficient [l, h] = Mathf.SmoothStep(textureCfg.strength, 0, (h_01-textureCfg.to)/textureCfg.margins);
					}

				}

		}
				


		float[,] heightMap = data.GetHeights (0, 0, data.heightmapHeight, data.heightmapWidth);
		int alphamapSize = data.alphamapWidth;

		float[,,] result = new float[alphamapSize,alphamapSize, textures.Count+1];

		float ratio = (float)alphamapSize / (float)heightMap.GetLength (0);
		for(int l = 0 ; l < textures.Count ; l++) {
			for (int x = 0; x < alphamapSize; x++) {
				for (int z = 0; z < alphamapSize; z++) {
					
					float height = heightMap[
					                         Mathf.Clamp(Mathf.RoundToInt(ratio*x),0,data.heightmapHeight-1),
					                         Mathf.Clamp(Mathf.RoundToInt(ratio*z),0,data.heightmapWidth-1)];
					TextureConfig textureCfg = textures[l];

					result[x,z,l+1]= textureCfg.strength*
							strengthCoefficient[l, Mathf.Clamp(Mathf.RoundToInt(height*1000), 0, 999)];

					/*if(textureCfg.from < height && textureCfg.to > height)
						result[x,z,l+1]= textureCfg.strength;*/
				}
				
			}
		}


		//SLOPES
		for (int x = 0; x < alphamapSize; x++) {
			for (int z = 0; z < alphamapSize; z++) {

				// Normalise x/y coordinates to range 0-1 
				float x_01 = (float)x/(float)data.alphamapHeight;
				float z_01 = (float)z/(float)data.alphamapWidth;
				float steepness = data.GetSteepness(z_01, x_01);

				TextureConfig textureCfg = cliff;
				if(textureCfg.from < steepness && textureCfg.to > steepness)
					result[x,z,0]= textureCfg.strength;
			}
			
		}



		//NORMALIZATION
		for (int x = 0; x < alphamapSize; x++) {
			for (int z = 0; z < alphamapSize; z++) {
				float total = 0f;
				for (int l = 0; l < result.GetLength(2); l++) {
					total+=result[x,z,l];
				}
				for (int l = 0; l < result.GetLength(2); l++) {
					result[x,z,l]=result[x,z,l]/total;
				}
			}
		}
		
		
		
		return result;
	}
	
	
	
	public static SplatPrototype getSplatPrototype(TextureConfig cfg) {
		SplatPrototype prt = new SplatPrototype ();
		prt.texture = cfg.texture;
		prt.normalMap = cfg.textureNormal;
		prt.tileSize = new Vector2(8, 8);
		prt.tileOffset = new Vector2(0, 0);
		return prt;
	}
	
	
	public class TextureConfig {
		public static TextureConfig make(JSONNode obj) {
			TextureConfig config = new TextureConfig();
			
			config.texture = (Texture2D)Resources.LoadAssetAtPath(obj["texture"].Value, typeof(Texture2D));
			config.textureNormal = (Texture2D)Resources.LoadAssetAtPath(obj["normaltexture"].Value, typeof(Texture2D));
			config.from = obj ["from"].AsFloat;
			config.to = obj ["to"].AsFloat;
			config.margins = obj ["margins"].AsFloat;
			config.strength = obj ["strength"].AsFloat;
			return config;
		}
		public Texture2D texture;
		public Texture2D textureNormal;
		public float from;
		public float to;
		public float margins;
		public float strength;
	}
	
}