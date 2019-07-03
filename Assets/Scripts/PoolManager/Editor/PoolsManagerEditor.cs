using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects, CustomEditor(typeof(PoolsManager))]
public class PoolsManagerEditor : Editor {
	private List<Transform> currentPools;
	//private Transform characterPrototypeContainer;
	//private Transform structurePrototypeContainer;
	private PoolsManager _poolManager;
	
	
	public void OnEnable() {
		_poolManager = target as PoolsManager;
		
		if (_poolManager.iDsManager == null)
			_poolManager.iDsManager = GameObject.Find("POOLMANAGERS").GetComponent<UniqueIDsManager>();
		
		UpdatePools ();
	}

	void UpdatePools ()
	{
		currentPools = new List<Transform>();
		Transform _poolsContainer = GameObject.Find("POOLMANAGERS").transform;
		if (_poolsContainer.childCount > 0) {
			for (int i = 0; i < _poolsContainer.childCount; i++)
				currentPools.Add(_poolsContainer.GetChild(i));
		}
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		
		GUILayout.BeginHorizontal(); {
			GUILayout.Label("Current Pools:", "BoldLabel", GUILayout.Width(150));
			GUILayout.Label("Unique Pools ID:", "BoldLabel", GUILayout.Width(150));
		} GUILayout.EndHorizontal();	
		
		if(currentPools.Count > 0) {
			foreach(Transform _pool in currentPools) {
				GUILayout.BeginHorizontal(); {
					GUILayout.Label(_pool.name, GUILayout.Width(150));
					GUILayout.Label(_pool.GetComponent<Pool>().PoolUniqueID.ToString(), GUILayout.Width(150));
				} GUILayout.EndHorizontal();
			}
		}
		
		GUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Update Pools List"))
				UpdatePools();
			if(GUILayout.Button("Update Pools IDs"))
				AddIDsToPools();
		} GUILayout.EndHorizontal();
		
		
		serializedObject.ApplyModifiedProperties();
	}

	public void AddIDsToPools ()
	{
		_poolManager.iDsManager.ClearAllIDs();
		
		if(currentPools.Count > 0) {
			foreach(Transform _pool in currentPools) {
				_pool.GetComponent<Pool>().PoolUniqueID = _poolManager.iDsManager.GetNewUniqueID();
			}
		}
	}
}
