using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class AbilityWarCry : Ability
{
    public float BuffPercent = 25.0f;
    private List<ArmorComponent> unitsArmorInRange;
    private BaseUnit _ownerUnit;

	// Use this for initialization
	new void Start () {
        base.Start();
		abilityName = "War Cry";
		abilityDescription = "Buffs the armor of nearby units at the moment of activation";
        unitsArmorInRange = new List<ArmorComponent>();
	}
	
	public override void Use (BaseUnit owner, BaseUnit target)
	{
	    if (_ownerUnit == null)
	        _ownerUnit = owner;

		if (inCooldown == false) {
	        StartCoroutine(ActiveBuff());
            StartCooldown();
	    }
	}

    public void OnTriggerEnter(Collider col) {
        var _baseUnit = col.GetComponent<BaseUnit>();
        if (_baseUnit != null && _baseUnit.IsDead == false && _ownerUnit != null && _ownerUnit.CheckIfIsMyEnemy(_baseUnit) == false) {
            var _armorComponent = _baseUnit.GetUnitComponent<ArmorComponent>();
            if (_armorComponent != null && unitsArmorInRange.Count == 0) {
                if (usingAbility && _armorComponent.Buffed == false) {
                    _armorComponent.Buffed = true;
                    IncreaseArmor(_armorComponent);
                }
                unitsArmorInRange.Add(_armorComponent);
            }
            else if (_armorComponent != null && unitsArmorInRange.Count > 0 && unitsArmorInRange.Contains(_armorComponent) == false) {
                if (usingAbility && _armorComponent.Buffed == false) {
                    _armorComponent.Buffed = true;
                    IncreaseArmor(_armorComponent);
                }
                unitsArmorInRange.Add(_armorComponent);
            }
        }
    }

    public void OnTriggerStay(Collider col)
    {
        if (unitsArmorInRange != null && unitsArmorInRange.Count > 0 && usingAbility == false)
        {
            foreach (var armorComponent in unitsArmorInRange)
            {
                if (armorComponent.Buffed == true)
                {
                    armorComponent.Buffed = false;
                    DecreaseArmor(armorComponent);
                }
            }
        }
        else if (unitsArmorInRange.Count > 0 && usingAbility == true)
        {
            foreach (var armorComponent in unitsArmorInRange)
            {
                if (armorComponent.Buffed == false)
                {
                    armorComponent.Buffed = true;
                    IncreaseArmor(armorComponent);
                }
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        var _baseUnit = col.GetComponent<BaseUnit>();
        if (_baseUnit != null && _ownerUnit != null && _baseUnit.IsDead == false && _ownerUnit.CheckIfIsMyEnemy(_baseUnit) == false)
        {
            var _armorComponent = _baseUnit.GetUnitComponent<ArmorComponent>();
            if (_armorComponent != null && unitsArmorInRange.Count == 0)
            {
                if (_armorComponent.Buffed)
                {
                    _armorComponent.Buffed = false;
                    DecreaseArmor(_armorComponent);
                }
                unitsArmorInRange.Remove(_armorComponent);
            }
            else if (_armorComponent != null && unitsArmorInRange.Count > 0 && unitsArmorInRange.Contains(_armorComponent) == true)
            {
                if (_armorComponent.Buffed)
                {
                    _armorComponent.Buffed = false;
                    DecreaseArmor(_armorComponent);
                }
                unitsArmorInRange.Remove(_armorComponent);
            }
        }
    }

    private IEnumerator ActiveBuff()
    {
        usingAbility = true;
        yield return new WaitForSeconds(abilityTime);
        usingAbility = false;
    }

    private void IncreaseArmor(ArmorComponent armor)
    {
        if (armor != null && armor.DefAmount.Count > 0)
        {
            for (int i = 0; i < armor.DefAmount.Count; i++)
            {
                armor.DefAmount[i] = armor.DefAmount[i] + ((armor.DefAmount[i] * BuffPercent) / 100);
            }
        }
    }

    private void DecreaseArmor(ArmorComponent armor)
    {
        if (armor != null && armor.DefAmount.Count > 0)
        {
            for (int i = 0; i < armor.DefAmount.Count; i++)
            {
                armor.DefAmount[i] = armor.UnbuffedDefAmount[i];
            }
        }
    }
}
