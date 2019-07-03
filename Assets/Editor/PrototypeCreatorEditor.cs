using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PrototypeCreator))]
public class PrototypeCreatorEditor : Editor {
	private PrototypeCreator _prototype;
	private Transform prototypesContainer;
	private List<Transform> prefabs;

	public void OnEnable() {
		_prototype = target as PrototypeCreator;
		
		prefabs = new List<Transform>();
		
		prototypesContainer = GameObject.Find("PROTOTYPES").transform;
		if (prototypesContainer == null)
			prototypesContainer = new GameObject("PROTOTYPES").transform;
	}
	
	public override void OnInspectorGUI() {	
		GUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Convert To Prototype")) {
				ConvertPrefabsInPrototypes();
			}
		} GUILayout.EndHorizontal();
	}
	
	private void ConvertPrefabsInPrototypes () {
		if(CheckPrefabs()) {
			foreach(Transform _prefab in prefabs) {
				_prefab.parent = prototypesContainer;
				PrefabUtility.DisconnectPrefabInstance(_prefab.gameObject);
			}
		}
	}
	
	private bool CheckPrefabs () {
		if (_prototype.transform.childCount > 0) {
			prefabs.Clear();
			
			for(int i = 0; i < _prototype.transform.childCount; i++)
				prefabs.Add(_prototype.transform.GetChild(i));
			
			return true;
		}
		return false;
	}
}
