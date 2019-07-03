using UnityEngine;
using System.Collections;

public class AbilityDispense : Ability {

	// Use this for initialization
	
	new void Start () {
		abilityName = "Dispense";
		abilityDescription = "Adds one upgrade point to the targeted allied unit";
        base.Start();
	}
	
	public override void Use (BaseUnit owner, BaseUnit target) { 
		if(target != null){
			StartAbility();
			DispenseOn(target);
		}
	}
	
	private void DispenseOn(BaseUnit target)
	{
		StartCooldown();
		Upgrade upgrade = UpgradesManager.Manager.UnitHasUpgrade(target.UnitClass);
		if(upgrade != null)
			target.GetUnitComponent<UpgradeComponent>().AddUpgradeToUnit(upgrade.UpgradeID);
		else
			Debug.Log("Target sem upgrades disponiveis");	
	}
}
