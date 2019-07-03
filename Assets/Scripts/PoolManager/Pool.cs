using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour {

	public GameObject Prototype;
	public bool CapMaxObjects = false;
	public int MaxObjects;
	public int PoolUniqueID = 0;
	public List<PoolItemComponent> InUseItens;
	public List<PoolItemComponent> AvaliableItens;
	public PreviewStructure PreviewStructure;
	private UniqueIDsManager iDsManager;
	
	void Start () {
		if (PreviewStructure != null) {
			PreviewStructure.PoolToSpawn = this;
		}
	}

	public void Enqueue(List<PoolItemComponent> List, PoolItemComponent Item) {
		List.Add(Item);
	}

	public PoolItemComponent Dequeue(List<PoolItemComponent> List) {
		PoolItemComponent _item = List[0];
		List.RemoveAt(0);
		return _item;
	}

	public void PopulateList() {
		ClearList();
		
		for (int i = 0; i < MaxObjects; i++) {
			var _instance = Instantiate(Prototype) as GameObject;
			
			_instance.name = Prototype.name + "(instance_" + i + ")";
			_instance.transform.parent = transform;

			var _uniqueID = iDsManager.GetNewUniqueID();
			
			var _poolItem = _instance.GetComponent<PoolItemComponent>();
			
			if (_poolItem == null)
				_poolItem = Prototype.AddComponent<PoolItemComponent>();
			
			_poolItem.MyPoolManager = this;	
			_poolItem.InUse = false;
			_poolItem.PoolItemID = _uniqueID;
			
			Enqueue(AvaliableItens, _poolItem);
		}
	}

	public void ClearList() {
		if (iDsManager == null)
			iDsManager = GameObject.Find("MANAGERS/GameManager").GetComponent<UniqueIDsManager>();
		
		if (InUseItens != null) {
			while (InUseItens.Count > 0) {
				var thisItem = Dequeue(InUseItens);
				iDsManager.RemoveUniqueID(thisItem.GetComponent<PoolItemComponent>().PoolItemID);
				DestroyImmediate(thisItem.gameObject);
			}
		}
		if (AvaliableItens != null) {
			while (AvaliableItens.Count > 0) {
				var thisItem = Dequeue(AvaliableItens);
				iDsManager.RemoveUniqueID(thisItem.GetComponent<PoolItemComponent>().PoolItemID);
				DestroyImmediate(thisItem.gameObject);
			}
		}

		InUseItens = new List<PoolItemComponent>();
		AvaliableItens = new List<PoolItemComponent>();
	}

    public PoolItemComponent Spawn(bool isEnemy)
    {
		return Spawn(Vector3.zero, 0, isEnemy);
	}
	
	public PoolItemComponent Spawn(Vector3 position, int teamID, bool isEnemy) {
		if (AvaliableItens.Count != 0) {
			var itemToReturn = Dequeue(AvaliableItens);
			
			itemToReturn.gameObject.SetActive(true);
			itemToReturn.InUse = true;
			itemToReturn.transform.position = position;
			
			var _baseUnit = itemToReturn.GetComponent<BaseUnit>();
			if (_baseUnit != null) {
				_baseUnit.TeamID = teamID;
				var _att = _baseUnit.GetUnitComponent<AttributesComponent>();
				if (_att != null)
					_att.LoadAttributes();
			}
			
			//Verifica se o objeto possui um navmesh, e se possui, ativa-o;
			var _navMeshAgent = itemToReturn.GetComponent<UnityEngine.AI.NavMeshAgent>();
			if (_navMeshAgent!= null && _navMeshAgent.enabled == false)
				_navMeshAgent.enabled = true;
			
			Enqueue(InUseItens, itemToReturn);
			return itemToReturn;
		}
			
		if (CapMaxObjects && MaxObjects > AvaliableItens.Count)
			return null;
			
		var newObject = Instantiate(Prototype, position, Prototype.transform.rotation) as GameObject;
		var poolItem = newObject.GetComponent<PoolItemComponent>();
		
		if (!poolItem) {
			poolItem = newObject.AddComponent<PoolItemComponent>();
			poolItem.InUse = true;
		}
		poolItem.transform.parent = transform;
		poolItem.InUse = true;
		poolItem.gameObject.SetActive(true);
		
		var _uniqueID = iDsManager.GetNewUniqueID();
		var _baseunit = newObject.GetComponent<BaseUnit>();
		if (_baseunit != null) {
			_baseunit.UniqueID = _uniqueID;
		} else {
			poolItem.PoolItemID = _uniqueID;	
		}
		
		var _attributes = _baseunit.GetUnitComponent<AttributesComponent>();
				if (_attributes != null)
					_attributes.LoadAttributes();
		
		Enqueue(InUseItens, poolItem);
		return poolItem;
	}

	public void Despawn(PoolItemComponent Object) {
	    if (!InUseItens.Contains(Object))
	        return;
	    Object.gameObject.SetActive(false);
	    Object.InUse = false;
	    InUseItens.Remove(Object);
	    AvaliableItens.Add(Object);
			
	    Object.transform.position = new Vector3(-9999,-9999,-9999);
			
	    if (Object.gameObject.GetComponent<BaseUnit>().UnitClass == Unit.Castle)
	        GameManager.Manager.SetGameOver(Object.gameObject.GetComponent<BaseUnit>().TeamID);
	}

	public void DespawnAll() {
		if (InUseItens.Count > 0) {
			for (int i = InUseItens.Count - 1; i >= 0; i--)
				Despawn(InUseItens[i]);
		}
	}
	
	public void AddUpdateToActivesUnits(int upgradeID) {
			foreach(PoolItemComponent _poolItem in InUseItens) {
			UpgradeComponent _upgradeComponent = _poolItem.OwnerUnit.GetUnitComponent<UpgradeComponent>();
			_upgradeComponent.AddUpgradeToUnit(upgradeID);
		}
	}

    public BaseUnit GetUnitByID(int ID)
    {
        if (InUseItens.Count <= 0) return null;

        foreach (var poolItemComponent in InUseItens)
        {
            if (poolItemComponent.PoolItemID == ID)
            {
                return poolItemComponent.OwnerUnit;
            }
        }

        return null;
    }
}
