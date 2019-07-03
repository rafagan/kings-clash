using UnityEngine;
using System.Collections;

public class AttackCommand : AbstractAttackCommand {

    //Chamado no Start do pai
    protected override void Init() {
        CommandID = 1;
    }

    //Chamado no Execute do pai
    protected override void Process() {
        switch (OwnerUnit.UnitType) {
            case ObjectType.CHARACTER: {
                var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
                fsm.SendMessageToState(IA_Messages.MOVE_TO_ATTACK);
            }
            break;
            case ObjectType.STRUCTURE: {
                var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_StructureUnitManager>().CurrentStateMachine;
                fsm.SendMessageToState(IA_Messages.MOVE_TO_ATTACK);
            }
            break;
        }
    }
}
