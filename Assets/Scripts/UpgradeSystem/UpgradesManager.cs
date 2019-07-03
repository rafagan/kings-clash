using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpgradesManager : MonoBehaviour {
	public static UpgradesManager Manager;
	public List<Upgrade> UpgradesContainer;
	
	private UniqueIDsManager uniqueIDsManager;

	
	void Awake()
	{
		if(Manager != null)
			Destroy(Manager.gameObject);
		
		Manager = this;
	}
	
	
	void Start () {
		uniqueIDsManager = transform.GetComponent<UniqueIDsManager>();
		SetupContainer ();
	}
	
	void Update () {
		
	
	}
	
	private void SetupContainer () {
		var _upgrades = transform.GetComponentsInChildren<Upgrade> ();
		if (_upgrades.Length > 0) {
			UpgradesContainer.Clear();
			uniqueIDsManager.ClearAllIDs();
			for (int i = 0; i < _upgrades.Length; i++) {
				_upgrades[i].UpgradeID = uniqueIDsManager.GetNewUniqueID();
				UpgradesContainer.Add(_upgrades[i]);
			}
		}
	}
	
	public Upgrade GetUpgradeByName (string name) {
		if (UpgradesContainer.Count > 0) {
			foreach (Upgrade _upgrade in UpgradesContainer) {
				if (_upgrade.name == name) return _upgrade;
			}
		}
		return null;
	}
	
	public Upgrade GetUpgradeByID (int ID) {
		if (UpgradesContainer.Count > 0) {
			foreach (Upgrade _upgrade in UpgradesContainer) {
				if (_upgrade.UpgradeID == ID) return _upgrade;
			}
		}
		return null;
	}
	
	public bool CheckUpgraded(int ID)
	{
		if (UpgradesContainer.Count > 0) {
			foreach (Upgrade _upgrade in UpgradesContainer) {
				if (_upgrade.UpgradeID == ID) 
				{
					return _upgrade.Upgraded;	
				}
			}
		}
		return false;
	}
	
	
	public void ResearchUpgrade(int ID)
	{
		if (UpgradesContainer.Count > 0) {
			foreach (Upgrade _upgrade in UpgradesContainer) {
				if (_upgrade.UpgradeID == ID) 
				{
					_upgrade.Research();
				}
			}
		}	
	}
	
	
	public void AddUpgradeToEntirePool(Upgrade upgrade, Pool unitPool)
	{
		if(unitPool.InUseItens.Count > 0)
		{
			foreach(PoolItemComponent item in unitPool.InUseItens)
			{
				BaseUnit unit = item.baseUnit;
				if(unit != null &&  unit.IsEnemy == false)
					item.baseUnit.GetUnitComponent<UpgradeComponent>().AddUpgradeToUnit(upgrade.UpgradeID);
			}
		}
		
		if(unitPool.AvaliableItens.Count > 0)
		{
			foreach(PoolItemComponent item in unitPool.AvaliableItens)
			{
				BaseUnit unit = item.gameObject.GetComponent<BaseUnit>();
				if(unit != null && unit.IsEnemy == false)
					unit.transform.Find("Components").Find("UpgradeSystem").GetComponent<UpgradeComponent>().AddUpgradeToUnit(upgrade.UpgradeID);
			}
		}
	}
	
	public bool UnitHasUpgrade(int ID)
	{
		if(UpgradesContainer.Count > 0)
		{
			foreach(Upgrade upgrade in UpgradesContainer)
			{
				if(upgrade.UpgradeID == ID)
					return true;
			}
		}	
		return false;
	}
	
	
	public Upgrade UnitHasUpgrade(Unit unitType)
	{
		if(UpgradesContainer.Count > 0)
		{
			foreach(Upgrade upgrade in UpgradesContainer)
			{
				foreach(Pool pool in upgrade.PoolUnits)
				{
					if(pool.Prototype.GetComponent<BaseUnit>().UnitClass == unitType && upgrade.Upgraded)
						return upgrade;
				}
			}
		}	
		return null;
	}
}
