using UnityEngine;
using System.Collections;
using UnityEditor;
public class CopyConfig : MonoBehaviour {

	static Terrain[] sceneTerrains;

	[MenuItem("Widerun/COPY CONFIG T0 -> Tn")]
	public static void Copy(MenuCommand command)
	{
		sceneTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
		if (sceneTerrains.Length == 0) {
			return;
		}
		System.Array.Sort(sceneTerrains, SortByZ);


		Terrain T0 = sceneTerrains [0];


		for (int i = 1; i < sceneTerrains.Length; i++) {

			Terrain current = sceneTerrains[i];

			TreePrototype[] workTreePrototypes = new TreePrototype[T0.terrainData.treePrototypes.Length];

			for (int tp = 0; tp < workTreePrototypes.Length; tp++){
				TreePrototype clonedTreePrototype = new TreePrototype();
				
				// prefab
				clonedTreePrototype.prefab = T0.terrainData.treePrototypes[tp].prefab;
				// bendFactor
				clonedTreePrototype.bendFactor = T0.terrainData.treePrototypes[tp].bendFactor;
				
				workTreePrototypes[tp] = clonedTreePrototype;
			}
			
			current.terrainData.treePrototypes = workTreePrototypes;





			DetailPrototype[] workPrototypes = new DetailPrototype[T0.terrainData.detailPrototypes.Length];
			
			for (int dp = 0; dp < workPrototypes.Length; dp++){
				DetailPrototype clonedPrototype = new DetailPrototype();
				
				// prototype
				clonedPrototype.prototype = T0.terrainData.detailPrototypes[dp].prototype;
				// prototypeTexture
				clonedPrototype.prototypeTexture = T0.terrainData.detailPrototypes[dp].prototypeTexture;
				// minWidth
				clonedPrototype.minWidth = T0.terrainData.detailPrototypes[dp].minWidth;
				// maxWidth
				clonedPrototype.maxWidth = T0.terrainData.detailPrototypes[dp].maxWidth;
				// minHeight
				clonedPrototype.minHeight = T0.terrainData.detailPrototypes[dp].minHeight;
				// maxHeight
				clonedPrototype.maxHeight = T0.terrainData.detailPrototypes[dp].maxHeight;
				// noiseSpread
				clonedPrototype.noiseSpread = T0.terrainData.detailPrototypes[dp].noiseSpread;
				// bendFactor
				clonedPrototype.bendFactor = T0.terrainData.detailPrototypes[dp].bendFactor;
				// healthyColor
				clonedPrototype.healthyColor = T0.terrainData.detailPrototypes[dp].healthyColor;
				// dryColor
				clonedPrototype.dryColor = T0.terrainData.detailPrototypes[dp].dryColor;
				// renderMode
				clonedPrototype.renderMode = T0.terrainData.detailPrototypes[dp].renderMode;
				
				workPrototypes[dp] = clonedPrototype;
			}
			
			current.terrainData.detailPrototypes = workPrototypes;




			SplatPrototype[] workSplatPrototypes = new SplatPrototype[T0.terrainData.splatPrototypes.Length];
			
			for (int sp = 0; sp < workSplatPrototypes.Length; sp++){
				SplatPrototype clonedSplatPrototype = new SplatPrototype();
				// texture
				clonedSplatPrototype.texture = T0.terrainData.splatPrototypes[sp].texture;
				clonedSplatPrototype.normalMap = T0.terrainData.splatPrototypes[sp].normalMap;
				// tileSize
				clonedSplatPrototype.tileSize = T0.terrainData.splatPrototypes[sp].tileSize;
				// tileOffset
				clonedSplatPrototype.tileOffset = T0.terrainData.splatPrototypes[sp].tileOffset;
				
				workSplatPrototypes[sp] = clonedSplatPrototype;
			}
			
			current.terrainData.splatPrototypes = workSplatPrototypes;




			current.materialTemplate =	T0.materialTemplate;






			current.terrainData.RefreshPrototypes();
			current.Flush();



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
