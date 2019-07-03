using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public abstract class AbstractMoveCommand : AbstractCommand {
    protected UnityEngine.AI.NavMeshAgent NavMeshComponent;
    protected BaseUnit TargetUnit;
    protected MobileComponent Mobile;

    protected abstract void Init();
    void Start() {
        if (OwnerUnit == null)
        {
            Debug.Log("BaseUnit não encontrado: " + transform.name);
            enabled = false;
        }

        Mobile = OwnerUnit.GetUnitComponent<MobileComponent>();
        if (Mobile == null) enabled = false;

        InitPathfinder();
        NavMeshComponent.ResetPath();

        this.Init();
	}

    protected abstract void Process();
	void Update () {}

    #region implemented abstract members of AbstractCommand
    public override void Execute(Mail mail)
    {
        if (mail.TargetID != 9999)
        {
            TargetUnit = PoolsManager.Manager.GetUnitByID(mail.TargetID);
            MoveTo(TargetUnit.transform.position);
        }
        else if (mail.TargetPosition != Vector3.zero)
            MoveTo(mail.TargetPosition);
    }

    public override IEnumerator CommandRoutine() {
        throw new System.NotImplementedException();
    }

    public void MoveTo(Vector3 position) {
        Mobile.Destiny = position;
        this.Process();
    }

    public void CancelMoveAction() {
        Mobile.Destiny = Vector3.zero;
        NavMeshComponent.ResetPath();
    }

    private void InitPathfinder() {
        NavMeshComponent = transform.parent.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (NavMeshComponent) return;
        transform.parent.gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        NavMeshComponent = transform.parent.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    #endregion
}
