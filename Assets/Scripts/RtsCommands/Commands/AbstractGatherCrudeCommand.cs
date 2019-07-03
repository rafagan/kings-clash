using UnityEngine;
using System.Collections;

public abstract class AbstractGatherCrudeCommand : AbstractCommand {
    protected CollectorComponent OwnerCollectorComponent;
    protected MoveCommand moveCommand;
    protected CrudeResourceComponent CrudeResource;

    protected abstract void Init();
	void Start () {
        moveCommand = GetComponent<MoveCommand>();

        if (OwnerUnit == null || moveCommand == null) { enabled = false; return; }

        OwnerCollectorComponent = OwnerUnit.GetUnitComponent<CollectorComponent>();

        if (OwnerCollectorComponent == null) { enabled = false; return; }

        this.Init();
	}

    protected abstract void Process();

    #region implemented abstract members of AbstractCommand
    public override void Execute(Mail mail) {
        var targetResource = PoolsManager.Manager.GetUnitByID(mail.TargetID);

        if (targetResource != null) {
            CrudeResource = targetResource.GetUnitComponent<CrudeResourceComponent>();
            if (CrudeResource != null) {
                OwnerCollectorComponent.CrudeResourceBeingCollected = targetResource;

                CommandEnded = false;
                StartCoroutine("CommandRoutine");

                this.Process();
            }
        }
    }

    public override IEnumerator CommandRoutine() {
        if (CrudeResource != null) {
            do {
                yield return null;
            } while (OwnerCollectorComponent.IsInRange(CrudeResource.ResourceBaseUnit) == false);
            moveCommand.CancelMoveAction();
            CrudeResource.Gather();
            CrudeResource = null;
        }

        StopCoroutine("CommandRoutine");
    }
    #endregion
}