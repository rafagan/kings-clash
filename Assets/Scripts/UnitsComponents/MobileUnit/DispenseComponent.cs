using UnityEngine;
using System.Collections;

public class DispenseComponent : AbstractUnitComponent {

	private ClickController Controller;
		
	void Start () {
		if(baseUnit == null)
			enabled = false;
		
		Controller = PlayerManager.Player.clickController;
	}
	
	// Update is called once per frame
	void Update () {
		CheckClick();
	}
	
	private void CheckClick()
	{
		if(baseUnit.IsSelected && !baseUnit.IsEnemy){	
			var target = Controller.CheckClickOnAlly();
			
			if (target != null) {
				Upgrade upgrade = UpgradesManager.Manager.UnitHasUpgrade(target.UnitClass);
				if(upgrade != null){
					target.GetUnitComponent<UpgradeComponent>().AddUpgradeToUnit(upgrade.UpgradeID);
				}	
			}
		}
	}
	
	 public override void GUIPriority() {}
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
