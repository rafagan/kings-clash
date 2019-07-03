using UnityEngine;
using System.Collections;

public abstract class AbstractAttackCommand : AbstractCommand {
    private BaseUnit _targetUnit;

    protected abstract void Init();
    void Start() {
        if (OwnerUnit == null) {
            Debug.Log("BaseUnit não encontrado: " + transform.name);
            enabled = false;
        }

        this.Init();
    }

    void OnDisable() {
        StopCoroutine("CommandRoutine");
    }

    protected abstract void Process();

    #region implemented abstract members of AbstractCommand
    public override void Execute(Mail mail) {
        _targetUnit = PoolsManager.Manager.GetUnitByID((int)mail.TargetID);

        CommandEnded = false;

        if (_targetUnit != null) {
            var attCmp = OwnerUnit.GetUnitComponent<AttackComponent>();
            attCmp.AttackPoint = _targetUnit.transform.position;
            attCmp.EnemyBeingAttacked = _targetUnit;
        }
        this.Process();
        StartCoroutine("CommandRoutine");
    }

    public override IEnumerator CommandRoutine() {
        if (OwnerUnit.GetUnitComponent<AttackComponent>() != null) {
            if (_targetUnit != null) {
			    bool isInRange;
                var attackCmp = OwnerUnit.GetUnitComponent<AttackComponent>();
			    do {
                    isInRange = attackCmp.IsInRangeOfAbility(_targetUnit) || attackCmp.IsInFrontOfMe(_targetUnit);
	                yield return null;
	            } while(isInRange == false);
			}

            OwnerUnit.GetUnitComponent<AttackComponent>().PrepareAttack(_targetUnit);
            _targetUnit = null;
        }
        CommandEnded = true;
    }
    #endregion
}
