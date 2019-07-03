using UnityEngine;

public class AbilityAssistStructure : Ability {

    void Awake() {
        abilityName = "Assist";
        abilityDescription = "Assist or Repair an structure";
    }

     new void Start() {
         base.Start();
     }

    public override void Use(BaseUnit owner, BaseUnit target) {
        Debug.Log("Usando");
        var structure = target.GetUnitComponent<StructureComponent>();
        var constructor = owner.GetUnitComponent<ConstructorComponent>();
        var fsm = owner.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;

        if (owner == null) return;

        if(structure != null) {
            if (structure.built)
            {
                constructor.StructureBeingRepaired = target;
                fsm.SendMessageToState(IA_Messages.REPAIR);
            }
            else
            {
                constructor.StructureBeingBuilt = target;
                fsm.SendMessageToState(IA_Messages.BUILD);
            }
        }
    }
}
