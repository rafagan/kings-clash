using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects, CustomEditor(typeof(Pool))]
public class PoolEditor : Editor {

	public SerializedProperty Prototype;
	public SerializedProperty PreviewStructure;
	public SerializedProperty CapMaxObjects;
	public SerializedProperty MaxObjects;
	public SerializedProperty PoolUnitClass;
	public Pool _pool;
    public Vector3 PositionToSpawn = Vector3.zero;

	private bool showAvaliableItens = false;
	private bool showInUseItens = false;

	public void OnEnable() {
		Prototype = serializedObject.FindProperty("Prototype");
		CapMaxObjects = serializedObject.FindProperty("CapMaxObjects");
		MaxObjects = serializedObject.FindProperty("MaxObjects");
		PreviewStructure = serializedObject.FindProperty("PreviewStructure");
		_pool = target as Pool;
		PositionToSpawn = _pool.transform.position;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
				
		EditorGUILayout.Space();
		
		GUILayout.Label("Unique Pool ID: " + _pool.PoolUniqueID, "BoldLabel");
		
		EditorGUILayout.PropertyField(CapMaxObjects, new GUIContent("Cap Max Objects:", "Limita a capacidade máxima de objetos no pool"));
		EditorGUILayout.PropertyField(MaxObjects, new GUIContent("Max Objects:", "Define a capacidade máxima de objetos no pool"));
		EditorGUILayout.PropertyField(Prototype, new GUIContent("Prototype:"));
		EditorGUILayout.PropertyField(PreviewStructure, new GUIContent("Preview Structure:"));
		
		EditorGUILayout.Space();
		
		EditorGUILayout.LabelField("Units Lists:");
		if(_pool.AvaliableItens != null) {
			showAvaliableItens = EditorGUILayout.Foldout(showAvaliableItens, "Avaliable: (" + _pool.AvaliableItens.Count + ")");
			if (showAvaliableItens) {
				foreach (var availableItem in _pool.AvaliableItens) {
					EditorGUILayout.ObjectField(availableItem, typeof (GameObject), true);
				}
			}
		}
		if(_pool.InUseItens != null) {
			showInUseItens = EditorGUILayout.Foldout(showInUseItens, "In Use: (" + _pool.InUseItens.Count + ")");
			if (showInUseItens) {
				foreach (var inUseItem in _pool.InUseItens) {
					EditorGUILayout.ObjectField(inUseItem, typeof (GameObject), true);
				}
			}
		}
			
		EditorGUILayout.Space();
		
		EditorGUILayout.LabelField("Pool Options:");
		GUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Populate List", GUILayout.MaxWidth(140), GUILayout.Width(140)))
				_pool.PopulateList();
			if(GUILayout.Button("Clear List"))
				_pool.ClearList();
		} GUILayout.EndHorizontal();
				
		GUILayout.BeginHorizontal(); {
			if (_pool.Prototype != null && !_pool.Prototype.GetComponent<BaseUnit>().IsResource) {
				if(GUILayout.Button("Spawn " + _pool.Prototype.GetComponent<AttributesComponent>().UnitName + " as Team 0"))
					_pool.Spawn(PositionToSpawn, 0, false);
				if(GUILayout.Button("Spawn " + _pool.Prototype.GetComponent<AttributesComponent>().UnitName + " as Team 1"))
          _pool.Spawn(PositionToSpawn, 1, false);
			} else if (_pool.Prototype != null) {
				if(GUILayout.Button("Spawn " + _pool.Prototype.name))
                    _pool.Spawn(PositionToSpawn, 0, false);
			}
		} GUILayout.EndHorizontal();
		if(GUILayout.Button("Remove All Spawned"))
				_pool.DespawnAll();
		
		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();
	}
}
