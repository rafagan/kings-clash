using UnityEngine;
using System.Collections;

//A quantidade a coletar está no VALUE1 do mail
public abstract class AbstractGatherNodeCommand : AbstractCommand {
    protected BaseUnit resourceUnit;
    protected MoveCommand moveCommand;
    protected CollectorComponent ownerCollectorComponent;
    protected int amountToGather;

    protected abstract void Init();
    void Start() {
        OwnerUnit = transform.parent.GetComponent<BaseUnit>();
        if (OwnerUnit == null) {
            Debug.Log("BaseUnit não encontrado: " + transform.name);
            enabled = false;
        }

        moveCommand = transform.GetComponent<MoveCommand>();
        if (moveCommand == null) {
            Debug.Log("Movecommand não encontrado: " + transform.name);
            enabled = false;
        }

        ownerCollectorComponent = OwnerUnit.GetUnitComponent<CollectorComponent>();
        this.Init();
    }

    protected abstract void Process();
    #region implemented abstract members of AbstractCommand
    public override void Execute(Mail mail) {
        if (OwnerUnit.GetUnitComponent<CollectorComponent>() == null)
            return;

        resourceUnit = PoolsManager.Manager.GetUnitByID(mail.TargetID);
        if (resourceUnit.IsResource) {
            OwnerUnit.GetUnitComponent<CollectorComponent>().ResourceBeingCollected = resourceUnit;

            CommandEnded = false;
            amountToGather = mail.Value1;
            StartCoroutine("CommandRoutine");

            this.Process();
        }
    }

    protected abstract void ProcessRoutine();
    public override IEnumerator CommandRoutine() {
        do {
            yield return null;
        } while (ownerCollectorComponent.IsInRange(resourceUnit) == false);

        this.ProcessRoutine();
    }
    #endregion
}
