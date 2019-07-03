using UnityEngine;
using System.Collections;

public class AbilityFlamingRocks : Ability {
	
	void Awake(){
		abilityName = "Flaming Rocks";
		abilityDescription = "Knockback the enemies around an unit area";
	}

    new void Start() {
        base.Start();
    }
	
	public override void Use(BaseUnit owner, BaseUnit target) {
		if (owner != null) {
            Debug.Log("Usando flaming rocks");
		}
	}
	
}
