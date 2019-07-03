using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects, CustomEditor(typeof(PoolItemComponent))]
public class PoolItemEditor : Editor {

	public PoolItemComponent _poolItem;

	public void OnEnable() {
		_poolItem = target as PoolItemComponent;
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.Space();

		GUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Create Pool"))
				CreatePool();
		} GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Destroy This Unit"))
				_poolItem.MyPoolManager.Despawn(_poolItem);
		} GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
	}
	
	private void CreatePool() {
		//Armazena referência do container de pools
		Transform _poolManagersContainer = GameObject.Find("POOLMANAGERS").transform;
		//Se for nulo, cria um novo
		if (_poolManagersContainer == null)
			_poolManagersContainer = new GameObject("POOLMANAGERS").transform;
			
		//Armazena a referência do componente ATTRIBUTES da unidade em questão
		AttributesComponent _attributes = _poolItem.transform.GetComponent<AttributesComponent>();
		if (_attributes == null) { Debug.Log(_poolItem.name + ": Attribute Component não encontrado"); return; }
		
		var _poolName = "name";
		if (_attributes.UnitType == ObjectType.CHARACTER)
			_poolName = "c" + _poolItem.name + "Pool";
		else if (_attributes.UnitType == ObjectType.STRUCTURE)
			_poolName = "s" + _poolItem.name + "Pool";
		else if (_attributes.UnitType == ObjectType.RESOURCE)
			_poolName = "r" + _poolItem.name + "Pool";
			
		//Verifica se já existe um Pool com este nome
		if (_poolManagersContainer.FindChild(_poolName) != null) {
			Debug.Log("Falha ao criar " + _poolName + "! Pool já existente!");
			return;
		}
		
		//Cria o novo pool com base no tipo de unidade
		Transform _newPoolObject = new GameObject().transform;
		_newPoolObject.parent = _poolManagersContainer;
		_newPoolObject.name = _poolName;
		
		//Adiciona o componente Pool e atribui o prototype
		Pool _newPool = _newPoolObject.gameObject.AddComponent<Pool>();
		_newPool.Prototype = _poolItem.gameObject;
		_newPool.MaxObjects = 100;
		_newPool.CapMaxObjects = true;
		_newPool.PopulateList();
		_poolManagersContainer.GetComponent<PoolsManager>().UpdateIDs();
		Debug.Log(_poolName + " criado com sucesso!");
	}
}
