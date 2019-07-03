using System.Collections.Generic;
using UnityEngine;

public class HUDActionRoot : MonoBehaviour {

    private ActionBarRow _actionBarRow;
    private ConstructorComponent constructorFromSelected;
    private StructureComponent structureFromSelected;
    private BaseUnit _selected;

    private bool hasBuiltStructure = false;


    void Start() {
        _actionBarRow = GetComponent<ActionBarRow>();

        if (_actionBarRow == null) {
            Debug.LogError("Não foi encontrado o componente ActionBarRow ou não foi referenciado o prefab no game object HUDActionButtons");
            Destroy(this);
            return;
        }
    }


    void Update() {
        if (InterfaceManager.GetSelectedBaseUnit() == null && _selected != null) {
            _selected = null;
            hasBuiltStructure = false;
            //Desativa os botoes
            _actionBarRow.DisableButtons();
            return;
        }


        if (_selected != InterfaceManager.GetSelectedBaseUnit()) {
            _selected = InterfaceManager.GetSelectedBaseUnit();
            constructorFromSelected = _selected.GetUnitComponent<ConstructorComponent>();

            structureFromSelected = _selected.GetComponent<StructureComponent>();
            if (structureFromSelected != null) {
                if (!structureFromSelected.built) {
                    _actionBarRow.DisableButtons();
                    return;
                }
            }

            Refresh();
        }
        else {
            if (structureFromSelected != null) {
                if (structureFromSelected.built) {
                    if (!hasBuiltStructure && _selected != null) {
                        Refresh();
                        hasBuiltStructure = true;
                    }
                }
            }
            if (constructorFromSelected == null)
                return;
            if (constructorFromSelected.ShowStructures)
                Refresh();
        }
    }

    void Refresh() {
        var attributesFromSelected = _selected.GetUnitComponent<AttributesComponent>();
        _actionBarRow.ActionList.Clear();
        switch (attributesFromSelected.UnitType) {
            case ObjectType.CHARACTER:
                if (constructorFromSelected != null) {
                    if (constructorFromSelected.ShowStructures) {
                        CreateActions(InterfaceManager.Manager.GetUnitPools());
                        constructorFromSelected.ShowStructures = false;
                    }
                    else {
                        CreateActions(InterfaceManager.Manager.GetUnitAbilities());
                    }
                }
                else {
                    CreateActions(InterfaceManager.Manager.GetUnitAbilities());
                }
                break;
            case ObjectType.STRUCTURE:
                CreateActions(InterfaceManager.Manager.GetStructurePools());
                break;
        }
    }

    void CreateActions(List<Pool> actions) {
        if (actions == null)
            return;
        if (actions.Count <= 0)
            return;
        foreach (var pool in actions) {
            var newAction = new ActionBarInfo {
                Atlas =  0,
                Icon = pool.Prototype.GetComponent<BaseUnit>().UnitIconName,
                Name = pool.Prototype.name,
                PlayCooldownAnimation = false,
                _ability = null,
                _pool = pool
            };
            _actionBarRow.ActionList.Add(newAction);
        }
        _actionBarRow.UpdateActionButtons();
    }

    void CreateActions(List<Ability> actions) {
        if (actions == null)
            return;
        if (actions.Count <= 0)
            return;
        foreach (var ability in actions) {
            var newAction = new ActionBarInfo {
                Atlas = 0,
                Name = ability.abilityName,
                Description = ability.abilityDescription,
                Icon = ability.AbilityIconName,
                PlayCooldownAnimation = true,
                HotKey = ability.HotKey,
                _ability = ability,
                _pool = null,
            };
            _actionBarRow.ActionList.Add(newAction);
        }
        _actionBarRow.UpdateActionButtons();
    }
}
