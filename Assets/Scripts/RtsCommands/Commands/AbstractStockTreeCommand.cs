using UnityEngine;
using System.Collections;

public abstract class AbstractStockTreeCommand : AbstractCommand {
    private CollectorComponent _collector;

    protected abstract void Init();
	void Start () {
	    _collector = OwnerUnit.GetUnitComponent<CollectorComponent>();
        Init();
	}

    protected abstract void Process();
    public override void Execute(Mail mail) {
        _collector.DropPointInUse = PoolsManager.Manager.GetUnitByID(mail.TargetID);
        Process();
    }

    public override IEnumerator CommandRoutine() {
        yield break;
    }
}
