using UnityEngine;

public class AbstractPrimaryResource : AbstractResource {
	public float ResourcesLeft;
    public bool depleated = false;
	protected bool occupied = false;

    public virtual float GatherAll() {
		if (ResourcesLeft >= 0) {
			var _amount = ResourcesLeft;
			ResourcesLeft = 0;
            if (baseUnit.UnitType == ObjectType.RESOURCE) {
                occupied = false;
                depleated = true;
                MailMan.Post.NewMail(
                    new Mail("Despawn", baseUnit.UniqueID,
                        baseUnit.UniqueID,
                        baseUnit.GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID));
            }
			return _amount;
		}
		return 0;
	}

    public virtual float Gather(float amount) {
		if (amount <= ResourcesLeft) {
			ResourcesLeft -= amount;
			return amount;
		} else {
			ResourcesLeft = 0;
            if (baseUnit.UnitType == ObjectType.RESOURCE)
            {
                occupied = false;
                depleated = true;
                MailMan.Post.NewMail(
                    new Mail("Despawn", baseUnit.UniqueID,
                        baseUnit.UniqueID,
                        baseUnit.GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID));
            }
			return 0;
		}
	}
	
	public virtual void AddResource (int teamIDtoAdd, int amountToGather) {
        if (amountToGather <= 0) {
            MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, teamIDtoAdd, (int)ResourceName, (int)GatherAll()));
        } else
            MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, teamIDtoAdd, (int)ResourceName, (int)Gather(amountToGather)));
	}
	
	public override void Reset() {}
}
