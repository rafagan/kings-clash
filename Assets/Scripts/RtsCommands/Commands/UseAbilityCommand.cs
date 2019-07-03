using UnityEngine;
using System.Collections;

public class UseAbilityCommand : AbstractUseAbilityCommand {
    protected override void Init() {
        CommandID = 8;
    }

    protected override void Process() {
        if (UnitAbilityLibrary.SelectedAbility != null) {
            if (UnitAbilityLibrary.SelectedAbility.name == "Assist") {
                var structure = Target.GetUnitComponent<StructureComponent>();
                var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;

                fsm.SendMessageToState(!structure.built ? IA_Messages.BUILD : IA_Messages.REPAIR);
            }
        }
    }
}
