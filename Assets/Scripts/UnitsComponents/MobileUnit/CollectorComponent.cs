using UnityEngine;
using System.Collections;

public class CollectorComponent : AbstractUnitComponent {
    public ResourceType CurrentResourceType = ResourceType.Steam;
    public float ReloadInSecs = 3.0f;
    public bool CanCollect = true;
    public LumberComponent MyLumber = null;
    public BaseUnit CrudeResourceBeingCollected = null, ResourceBeingCollected = null;
    public BaseUnit DropPointInUse = null;
    public float GatherRange = 2.0f;

    public void Start() {
        if (baseUnit == null)
            enabled = true;
    }

    void Update() {
        if (baseUnit.IsEnemy || !baseUnit.IsSelected || !baseUnit.GetUnitComponent<CollectorComponent>()) {
            return;
        }

        CheckDropPointClick();
        CheckPlasmoNodeClick();
        CheckTreeNodeClick();
        CheckCrudeResourceClick();
    }

    private void CheckDropPointClick() {
        //O alvo é do tipo "Estrutura"?
        var target = PlayerManager.Player.clickController.CheckClickOnAlly();
        if (target == null || target.UnitType != ObjectType.STRUCTURE) return;

        //O alvo é um DropPoint? A unidade esta com madeira nas mãos?
        var dropPoint = target.GetUnitComponent<DropPointComponent>();
        if (dropPoint == null || MyLumber.gameObject.activeInHierarchy == false)
            return;

        //A estrutura esta construída?
        var structure = target.GetUnitComponent<StructureComponent>();
        if(!structure.built)
            return;

        MailMan.Post.NewMail(new Mail("StockTree", baseUnit.UniqueID, target.UniqueID));
    }

    private void CheckPlasmoNodeClick() {
        var _target = PlayerManager.Player.clickController.CheckPlasmoNodeClick();
        if (_target != null) {
            ResourceBeingCollected = _target;
            MailMan.Post.NewMail(new Mail("GatherPlasmoNode", baseUnit.UniqueID, _target.UniqueID));
        }
    }

    private void CheckTreeNodeClick() {
        BaseUnit _target = PlayerManager.Player.clickController.CheckLoggedTreeClick();
        if (_target != null) {
            ResourceBeingCollected = _target;
            MailMan.Post.NewMail(new Mail("GatherLoggedTree", baseUnit.UniqueID, _target.UniqueID));
        }
    }

    private void CheckCrudeResourceClick() {
        BaseUnit _target = PlayerManager.Player.clickController.CheckCrudeResourceClick();
        if (_target != null) {
            CrudeResourceBeingCollected = _target;
            MailMan.Post.NewMail(new Mail("GatherCrudeResource", baseUnit.UniqueID, _target.UniqueID));
        }
    }

    public bool IsInRange(BaseUnit resource) {
        if (resource != null)
            return (Vector3.Distance(baseUnit.transform.position, resource.transform.position) <= GatherRange);

        return false;
    }
    public bool IsInRange(Vector3 destiny) {
        return (Vector3.Distance(baseUnit.transform.position, destiny) <= GatherRange);
    }
    
     public override void GUIPriority() {}
     
     public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
