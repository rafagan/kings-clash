using UnityEngine;
using System.Collections;
public class AbilityRaiseShield : Ability {
	
	private BaseUnit baseUnit;
	private float defaultPierceAmount;
	private float defaultSpeed;
	private float defaultSlashAmount;
	
	void Awake () {
		this.abilityName = "Raise Shield";
		this.abilityDescription = "(30% less speed, -80% pierce (arrow) damage, -30% fire)";
	}
	
	public override void Use(BaseUnit owner, BaseUnit target) {
		if (owner != null) {
            if (!usingAbility)
		    {
		        RaiseShield(owner);
                StartCooldown();
		    }
		}
	}
	
	public void RaiseShield(BaseUnit owner)
	{
		SaveAmount(owner);
		var _unitArmor = owner.GetUnitComponent<ArmorComponent>();
		owner.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 1;
		_unitArmor.DefAmount[0] = 30;
		_unitArmor.DefAmount[1] = 80;
		StartCoroutine("Timer");
		
	}
	
	private void SaveAmount(BaseUnit owner)
	{	
		baseUnit = owner;
		defaultSlashAmount = owner.GetUnitComponent<ArmorComponent>().DefAmount[0];
		defaultPierceAmount = owner.GetUnitComponent<ArmorComponent>().DefAmount[1];
		defaultSpeed = owner.GetComponent<UnityEngine.AI.NavMeshAgent>().speed;
	}

	private IEnumerator Timer () {
		usingAbility = true;
		yield return new WaitForSeconds(abilityTime);
		usingAbility = false;
		baseUnit.GetUnitComponent<ArmorComponent>().DefAmount[0] = defaultSlashAmount;
		baseUnit.GetUnitComponent<ArmorComponent>().DefAmount[1] = defaultPierceAmount;
		baseUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = defaultSpeed;
	}
}