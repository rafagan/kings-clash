using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GUIButtonAction : MonoBehaviour {


    private Pool _pool = null;
    private Ability _ability = null;
    

    public void Create(Pool pool) {
        _ability = null;
        _pool = pool;
    }

    public void Create(Ability ability) {
        _pool = null;
        _ability = ability;
    }


    void OnClick() {
        if (InterfaceManager.GetSelectedBaseUnit() == null) {
            Destroy(gameObject);
            return;
        }
        if (_ability != null) {
            InterfaceManager.Manager.SetAbilityClicked(_ability);
        }
        if (_pool != null) {
            if (_pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.CHARACTER)
                InterfaceManager.GetSelectedBaseUnit().GetUnitComponent<ProducerComponent>().TrainUnit(_pool.PoolUniqueID);
            else if (_pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.STRUCTURE)
                _pool.PreviewStructure.Spawn(InterfaceManager.GetSelectedBaseUnit());
        }
    }
}
