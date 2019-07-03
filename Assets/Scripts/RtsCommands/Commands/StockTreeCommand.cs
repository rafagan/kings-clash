using UnityEngine;
using System.Collections;

public class StockTreeCommand : AbstractStockTreeCommand {
    protected override void Init() {
        CommandID = 9;
    }

    protected override void Process() {
        var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
        fsm.SendMessageToState(IA_Messages.STOCK);
    }
}
