using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpgradeComponent : AbstractUnitComponent {
	public List<Upgrade> AvaliableUpgrades;	//Lista de atualizações possíveis para a unidade
	public List<Upgrade> UpgradedList; //Lista de atualizações atuais da unidade
		
	void Awake () {
		if (AvaliableUpgrades.Count > 0) {
			foreach (Upgrade _upgrade in AvaliableUpgrades) {
				if (_upgrade.Upgraded && !UpgradedList.Contains(_upgrade))
					AddUpgradeToUnit(_upgrade.UpgradeID);
			}
		}
	}
	
	void Start () {
		if (baseUnit == null) enabled = false;
	}
	
	public void AddUpgradeToUnit(int ID) {
		Upgrade upgrade = UpgradesManager.Manager.GetUpgradeByID(ID);
		
		if(UpgradedList.Contains(upgrade) == false)
			UpgradedList.Add(upgrade);	
	}
	
	public bool CheckIfUpgraded(int ID) {
		
		Upgrade upgrade = UpgradesManager.Manager.GetUpgradeByID(ID);
		
		if(UpgradedList.Count > 0)
			return UpgradedList.Contains(upgrade);
		
		return false;
	}
	
	 public override void GUIPriority() {}
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
