using UnityEngine;
using System.Collections;

public class GatherCrudeCommand : AbstractGatherCrudeCommand {

    //Chamado no Start do pai
    protected override void Init() {
        CommandID = 6;
    }

    //Chamado no Execute do pai
    protected override void Process() {
        OwnerCollectorComponent.ResourceBeingCollected = null;

        var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
        if (CrudeResource.ResourceType == CrudeResourceType.CrudePlasmo) {
            fsm.SendMessageToState(IA_Messages.CRUDE_GATHERING_PLASMO);
        } else if (CrudeResource.ResourceType == CrudeResourceType.CrudeTree) {
            fsm.SendMessageToState(IA_Messages.CRUDE_GATHERING_TREE);
        }
    }
}
