using UnityEngine;
using System.Collections;

//Mensagem que representa a captura da árvore derrubada
public class GatherLoggedTreeCommand : AbstractGatherNodeCommand {
    protected override void Init() {
        CommandID = 7;
    }

    protected override void Process() {
        var fsm = OwnerUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>().CurrentStateMachine;
        fsm.SendMessageToState(IA_Messages.GATHERING_LOGGED_TREE);
    }

    protected override void ProcessRoutine() {
        MailMan.Post.NewMail(
                new Mail("Despawn", resourceUnit.UniqueID, resourceUnit.UniqueID,
                    PoolsManager.Manager.GetUnitByID(resourceUnit.UniqueID).GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID));
		resourceUnit = null;
    }
}
