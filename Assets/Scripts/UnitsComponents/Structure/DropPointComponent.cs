using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DropPointComponent : AbstractUnitComponent {
    //O start é chamado somente se o objeto estiver enabled
    void Start() {
        if (baseUnit == null) {
            baseUnit = gameObject.GetComponent<BaseUnit>();
        }
        InsertDropPoint();
    }

	void OnEnable() {
        if (baseUnit == null) {
            baseUnit = gameObject.GetComponent<BaseUnit>();
        }
        InsertDropPoint();
	}

    void OnDisable() {
        if (DropPointManager.DropPoints == null)
            return;
        if (baseUnit.IsEnemy) {
            return;
        }

        DropPointManager.DropPoints.Remove(baseUnit);
    }

    private void InsertDropPoint()
    {
        if (DropPointManager.DropPoints == null)
            DropPointManager.DropPoints = new HashSet<BaseUnit>();
        if (baseUnit.IsEnemy) {
            return;
        }

        DropPointManager.DropPoints.Add(baseUnit);
    }
    
     public override void GUIPriority(){}
     
     public override void UserInputPriority() {
	
	}
	public override void Reset() {}
}
