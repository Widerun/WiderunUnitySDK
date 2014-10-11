using UnityEditor;
using UnityEngine;
public class EditorWizard : ScriptableWizard {
	public float seed = 12345f;
	public string heightsFile = "";
	TextAsset txt = new TextAsset();
	[Range(-1, 1)]
	public float leftHeight = 0;
	[Range(-1, 1)]
	public float rightHeight = 0;


	[MenuItem ("Widerun/Create Terrain")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<EditorWizard>("Create Widerun terrain", "Create");
	}
	void OnWizardCreate () {
		
	}  
	void OnWizardUpdate () {
		helpString = "Please set the terrain details!";
	}   

}