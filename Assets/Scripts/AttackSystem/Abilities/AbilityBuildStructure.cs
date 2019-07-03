using UnityEngine;

public class AbilityBuildStructure : Ability {


    void Awake() {
        abilityName = "Build";
        abilityDescription = "Build an structure";
    }

    new void Start() {
        base.Start();
    }

    public override void Use(BaseUnit owner, BaseUnit target) {
        if (owner != null) {
            var constructor = owner.GetUnitComponent<ConstructorComponent>();
            if (constructor != null) {
                constructor.ShowStructures = true;
            } else {
                Debug.Log("Constructor Component não foi encontrado no " + owner.UnitName);
            }
        }
    }
}
