using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolsManager : MonoBehaviour {
	public static PoolsManager Manager;
	public int desiredID = 102;
	public UniqueIDsManager iDsManager;
	public BaseUnit LastSpawnedUnit;
	
	private Transform projectiles;
	
	void Awake() {
		if (Manager != null)
			Destroy(Manager.gameObject);
		Manager = this;
		LastSpawnedUnit = null;
	}

	public void AddNewPool(Transform pool) {
		pool.parent = transform;
	}
	
	public void RemovePool(Pool pool) {
		if (pool != null) {
			GameObject.Destroy(pool.Prototype);
			GameObject.Destroy(pool);
		}
	}
	
	public void UpdateIDs() {
		iDsManager.ClearAllIDs();
		
		if(transform.childCount > 0) {
			for(int i = 0; i < transform.childCount; i++) {
				transform.GetChild(i).GetComponent<Pool>().PoolUniqueID = iDsManager.GetNewUniqueID();
			}
		}
	}
	
	public BaseUnit GetUnitByID(int ID) {
		if (transform.childCount > 0) {
			for (int i = 0; i < transform.childCount; i++) {
				//Busca no pool de unidades na ordem;
				var _currentPool = transform.GetChild(i);	
				if (_currentPool.childCount > 0) {
					//Pega o primeiro e o ultimo BaseUnit e verifica se está no range da ID
					var _firstUnit = _currentPool.GetChild(0).GetComponent<PoolItemComponent>();
					var _lastUnit = _currentPool.GetChild(_currentPool.childCount - 1).GetComponent<PoolItemComponent>();
					if (_firstUnit != null && _lastUnit != null) {
						//Se está no range, inicia a busca
						if (ID >= _firstUnit.PoolItemID || ID <= _lastUnit.PoolItemID) {
							for (int j = 0; j < _currentPool.childCount; j++) {
								var _currentUnit = _currentPool.GetChild(j).GetComponent<PoolItemComponent>();
								if (_currentUnit.PoolItemID == ID) {
									return _currentPool.GetChild(j).GetComponent<BaseUnit>();
								}
							}
						}
					}	
				}
			}
		}
		
		return null;
	}
	
	public PoolItemComponent GetItemByID(int ID) {
		if (transform.childCount > 0) {
			for (int i = 0; i < transform.childCount; i++) {
				//Busca no pool de unidades na ordem;
				var _currentPool = transform.GetChild(i);	
				if (_currentPool.childCount > 0) {
					//Pega o primeiro e o ultimo poolitem e verifica se está no range da ID
					var _firstUnit = _currentPool.GetChild(0).GetComponent<PoolItemComponent>();
					var _lastUnit = _currentPool.GetChild(_currentPool.childCount - 1).GetComponent<PoolItemComponent>();
					if (_firstUnit != null && _lastUnit != null) {
						//Se está no range, inicia a busca
						if (ID >= _firstUnit.PoolItemID || ID <= _lastUnit.PoolItemID) {
							for (int j = 0; j < _currentPool.childCount; j++) {
								var _currentItem = _currentPool.GetChild(j).GetComponent<PoolItemComponent>();
								if (_currentItem.PoolItemID == ID)
									return _currentItem;
							}
						}
					}	
				}
			}
		}
		
		return null;
	}
	
	public Pool GetPoolByID(int ID) {
		for (int i = 0; i < transform.childCount; i++) {
			Pool _desiredPool = transform.GetChild(i).GetComponent<Pool>();
			if (_desiredPool != null && _desiredPool.PoolUniqueID == ID)
				return _desiredPool;
		}
		return null;
	}
	
	public int GetPoolIDByName(string name) {
		for (int i = 0; i < transform.childCount; i++) {
			Pool _desiredPool = transform.GetChild(i).GetComponent<Pool>();
			if (_desiredPool != null && _desiredPool.name == name)
				return _desiredPool.PoolUniqueID;
		}
		return 999;
	}
}
