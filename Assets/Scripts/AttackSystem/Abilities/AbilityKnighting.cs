using UnityEngine;

public class AbilityKnighting : Ability {

	// Use this for initialization
	new void Start () {
		abilityName = "Knighting";
		abilityDescription = "Levels up one allied unit";
        base.Start();
	}
	
	public override void Use (BaseUnit owner, BaseUnit target) {
		if(target != null){
			StartCooldown();
			UpgradeUnit(target);
		}	
	}
	
	private void UpgradeUnit(BaseUnit target) {
		target.GetUnitComponent<AttributesComponent>().isVeteran = true;
		Debug.Log(target.GetUnitComponent<AttributesComponent>().UnitName + ": Veteran");
	}
}
