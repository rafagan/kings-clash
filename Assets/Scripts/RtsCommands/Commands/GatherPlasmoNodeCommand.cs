using UnityEngine;
using System.Collections;

public class GatherPlasmoNodeCommand : AbstractGatherNodeCommand {
    //Chamado no Start do pai
    protected override void Init() {
        CommandID = 2;
    }

    //Chamado no Execute do pai
    protected override void Process() {
        var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
        fsm.SendMessageToState(IA_Messages.NODE_GATHERING_PLASMO);
    }

    protected override void ProcessRoutine() {
        resourceUnit.GetUnitComponent<PlasmoComponent>().AddResource(OwnerUnit.TeamID, amountToGather);
        resourceUnit = null;
    }
}
