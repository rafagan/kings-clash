using UnityEngine;
using System.Collections;

public abstract class AbstractUseAbilityCommand : AbstractCommand {

    protected AbilityLibraryComponent UnitAbilityLibrary;
    protected BaseUnit Target;

    protected abstract void Init();
    void Start() {
        this.Init();
    }

    #region implemented abstract members of AbstractCommand

    protected abstract void Process();

    public override void Execute(Mail mail) {
        var ownerUnit = PoolsManager.Manager.GetUnitByID((int)mail.OwnerID);
        Target = mail.TargetID >= 0 ? PoolsManager.Manager.GetUnitByID((int)mail.TargetID) : null;
        var abilityID = mail.Value1;
        UnitAbilityLibrary = ownerUnit.GetUnitComponent<AbilityLibraryComponent>();
        if (UnitAbilityLibrary != null) {
            UnitAbilityLibrary.SelectAbility(abilityID);
            UnitAbilityLibrary.UseSelectedAbility(Target);
        }

        CommandEnded = true;
        this.Process();
    }

    public override IEnumerator CommandRoutine() {
        return null;
    }

    #endregion
}
