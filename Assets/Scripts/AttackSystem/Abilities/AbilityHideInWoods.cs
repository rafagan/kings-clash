using System.Collections.Generic;
using UnityEngine;

public class AbilityHideInWoods : Ability {
    private BaseUnit _ownerUnit;
    private AttributesComponent ownerAttributes;
    private List<CrudeResourceComponent> treeList;
    public bool onTree { get { return treeList.Count > 0; } }

    void Awake()
    {
        abilityName = "Hide in Woods";
        abilityDescription = "Become invisible over trees.";
        treeList = new List<CrudeResourceComponent>();
    }

    new void Start() {
        base.Start();
    }

    public override void Use(BaseUnit owner, BaseUnit target)
    {
        if (_ownerUnit == null) {
            _ownerUnit = owner;
            ownerAttributes = owner.GetUnitComponent<AttributesComponent>();
        }

        if (ownerAttributes.IsFlying) {
            StartCooldown();
            if (ownerAttributes.Invisible == false && onTree)
                GoInvisible();
            else if (ownerAttributes.Invisible)
                GoVisible();
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        var treeBaseUnit = col.GetComponent<BaseUnit>();
        if (treeBaseUnit != null) {
            var _tree = treeBaseUnit.GetUnitComponent<CrudeResourceComponent>();
            if (col.gameObject.layer == 12 && _tree != null) {
                if (treeList.Count == 0)
                    treeList.Add(_tree);
                else if (treeList.Count > 0 && treeList.Contains(_tree) == false)
                    treeList.Add(_tree);
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        var _treeBaseUnit = col.GetComponent<BaseUnit>();
        if (_treeBaseUnit != null)
        {
            var _tree = col.GetComponent<BaseUnit>().GetUnitComponent<CrudeResourceComponent>();
            if (col.gameObject.layer == 12 && _tree != null)
            {
                treeList.Remove(_tree);
            }

            if (_ownerUnit != null)
            {
                if (onTree == false && ownerAttributes.Invisible == true)
                {
                    GoVisible();
                }
            }
        }
    }

    private void GoInvisible()
    {
        ownerAttributes.Invisible = true;
        _ownerUnit.MeshGO.GetComponent<Renderer>().enabled = false;
    }

    private void GoVisible()
    {
        ownerAttributes.Invisible = false;
        _ownerUnit.MeshGO.GetComponent<Renderer>().enabled = true;
    }
}
