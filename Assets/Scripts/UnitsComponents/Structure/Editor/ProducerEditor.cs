using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

//[CustomEditor(typeof(Producer))]
public class ProducerEditor : Editor {
	private SerializedProperty unitsPoolList;
	private List<string> avaliablePoolsString;
	private List<string> inUsePoolsString;
	private List<Transform> unitsPools;
	private int index;
	public ProducerComponent _producer;
	
	void OnEnable () {
		_producer = target as ProducerComponent;
		unitsPoolList = serializedObject.FindProperty("UnitsPool");
		
		if (unitsPools == null)
			unitsPools = new List<Transform>();
		UpdateUnitsPools ();
		
		if (inUsePoolsString == null)
			inUsePoolsString = new List<string>();
		if (avaliablePoolsString == null)
			avaliablePoolsString = new List<string>();
		UpdateAvaliablePoolsString();
	}
	
	public override void OnInspectorGUI () {
		serializedObject.Update();
		
		EditorGUILayout.Space();
		
		#region ROW 1
		GUILayout.BeginHorizontal(); {
			#region COLUMN 1
			GUILayout.BeginVertical(); {
				
			}  GUILayout.EndVertical();
			#endregion
			
			#region COLUMN 2
			GUILayout.BeginVertical(GUILayout.Width(300)); {
				GUILayout.Label("Avaliable Unit Pools: ");
				GUI.enabled = avaliablePoolsString.Count > 0;
				string[] _currentNames = new string[avaliablePoolsString.Count];
				for (int i = 0; i < avaliablePoolsString.Count; i++)
					_currentNames[i] = avaliablePoolsString[i];
				
				GUILayout.BeginHorizontal(); {
					index = EditorGUILayout.Popup(index, _currentNames, GUILayout.Width(150));
					if (GUILayout.Button("Add Unit Pool", GUILayout.Width(150))) {
						
					}
				}GUILayout.EndHorizontal();
				
				GUI.enabled = true;
			}GUILayout.EndVertical();
			#endregion
			
			#region COLUMN 3
			GUILayout.BeginVertical(); {
				
			}GUILayout.EndVertical();
			#endregion
		}GUILayout.EndHorizontal();
		#endregion
		serializedObject.ApplyModifiedProperties();
	}

	void UpdateUnitsPools () {
		var _unitsPoolContainer = GameObject.Find("POOL MANAGERS/ALLYTEAM/UnitsPoolManagers").transform;
		var _count = _unitsPoolContainer.childCount;
		if (_count > 0) {
			unitsPools.Clear();
			for (int i = 0; i < _count; i++)
				unitsPools.Add(_unitsPoolContainer.GetChild(i).transform);
		}
	}
	
	private void UpdateAvaliablePoolsString () {
		avaliablePoolsString.Clear();
		var _unitsPoolContainer = GameObject.Find("POOL MANAGERS/ALLYTEAM/UnitsPoolManagers").transform;
		
		for (int i = 0; i < _unitsPoolContainer.childCount; i++) {
			var _pool = _unitsPoolContainer.GetChild(i);
			if (_pool != null && !CheckInUsePoolsString(_pool))
				avaliablePoolsString.Add(_pool.name);
		}
	}
	
	private bool CheckInUsePoolsString (Transform pool) {
		return (inUsePoolsString.Count > 0 && inUsePoolsString.Contains(pool.name));
	}
	
	private void AddUnitsPool () {
		unitsPoolList.arraySize++;
					
	}
}
