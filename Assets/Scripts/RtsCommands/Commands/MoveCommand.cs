using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MoveCommand : AbstractMoveCommand {
    //Chamado no Start do pai
    protected override void Init() {
        CommandID = 0;
    }

    //Chamado no Execute do pai
    protected override void Process() {
        var FSM = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
        FSM.SendMessageToState(IA_Messages.MOVING);
    }
}
