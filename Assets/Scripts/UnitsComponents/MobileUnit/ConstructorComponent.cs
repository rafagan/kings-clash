using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructorComponent : AbstractUnitComponent {
	//Public Attributes
	public List<Pool> BuildingsPools;
    public bool ShowStructures = false;
    public BaseUnit StructureBeingBuilt = null, StructureBeingRepaired = null;
    public int BuildRange = 3;
		
	public void Start () {
		if (baseUnit == null) enabled = false;	
	}

    public void Update() {
        var target = PlayerManager.Player.clickController.CheckClickOnAlly();
        if (target == null || target.UnitType != ObjectType.STRUCTURE || !baseUnit.IsSelected) return;

        //Se tiver carregando uma árvore derrubada, não pode reparar
        if (baseUnit.GetUnitComponent<CollectorComponent>().MyLumber.gameObject.activeInHierarchy) return;

        var structureAtt = target.GetUnitComponent<AttributesComponent>();

        if (Math.Abs(structureAtt.CurrentLife - structureAtt.MaxLife) < Mathf.Epsilon) {
            return;
        }

        var _abilityLibrary = baseUnit.GetUnitComponent<AbilityLibraryComponent>();
        MailMan.Post.NewMail(new Mail("UseAbility", baseUnit.UniqueID, target.UniqueID, _abilityLibrary.UnitAbilities.IndexOf(_abilityLibrary.GetAbilityInLibrary("Assist"))));
    }
	
	public override void GUIPriority() {}

    public override void UserInputPriority() {}

    public override void Reset() {}

    public bool IsInRangeOfBuild(Vector3 target)
    {
        RaycastHit hit;
        var _ray = new Ray(baseUnit.transform.position, (target - baseUnit.transform.position).normalized);
        if (Physics.Raycast(_ray, out hit, Mathf.Infinity))
        {
            var distance = Vector3.Distance(baseUnit.transform.position, hit.point);
            var isInRange = distance <= BuildRange;
            return isInRange;
        }
        return false;
    }

    public bool IsInRangeOfBuild(BaseUnit target) {
        if (target != null)
        {
            RaycastHit hit;
            var _ray = new Ray(baseUnit.transform.position,
                (target.transform.position - baseUnit.transform.position).normalized);
            if (target != null)
            {
                if (Physics.Raycast(_ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.GetComponent<BaseUnit>() == target)
                    {
                        var distance = Vector3.Distance(baseUnit.transform.position, hit.point);
                        var isInRange = distance <= BuildRange;
                        return isInRange;
                    }
                }
            }
        }
        return false;
    }
}
