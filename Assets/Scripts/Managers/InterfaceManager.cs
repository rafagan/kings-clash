using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterfaceManager : MonoBehaviour {
	public static InterfaceManager Manager;
	
	public List<Unit> UnitsPriority;
	private List<BaseUnit> _selectedUnits;
	public List<BaseUnit> SameUnitsToSendInput;
	private int _abilityClicked;
	
	void Awake () {
		if (Manager != null)
			Destroy(Manager.gameObject);
		Manager = this;
	}

	void Start () {
		_selectedUnits = GameManager.Manager.unitsManager.SelectedUnits;
		SameUnitsToSendInput = new List<BaseUnit>();
		_abilityClicked = -1;
	}
	
	// Update is called once per frame
	void Update () {
		CheckSelectedUnits();
		SetSelectedAbilityToUnits();
	}

	void CheckSelectedUnits()
	{
		if (_selectedUnits != null && _selectedUnits.Count > 0) {
		    SameUnitsToSendInput.Clear();
		    foreach (Unit t in UnitsPriority) {
		        foreach (BaseUnit _currentUnit in _selectedUnits) {
		            if (_currentUnit.IsEnemy || _currentUnit.UnitClass != t || _currentUnit.GetAllUnitComponents.Count <= 0)
		                continue;
		            //Armazena a referência de todas as unidades do mesmo tipo selecionadas para envio de Input
		            foreach (BaseUnit sameTypeUnit in _selectedUnits) {
		                if (!SameUnitsToSendInput.Contains(sameTypeUnit) && !sameTypeUnit.IsEnemy && sameTypeUnit.UnitClass == _currentUnit.UnitClass)
		                    SameUnitsToSendInput.Add(sameTypeUnit);
		            }
		            return;
		        }
		    }
		} else {
			SameUnitsToSendInput.Clear();
		}
	}
	
	void SetSelectedAbilityToUnits() {
		if (SameUnitsToSendInput.Count > 0 && _abilityClicked >= 0) {
            Debug.Log("Setando");
			foreach(BaseUnit _unit in SameUnitsToSendInput) {
                Debug.Log("Enviando para unidades: " + _unit);
				_unit.GetUnitComponent<AbilityLibraryComponent>().SelectAbility(_abilityClicked);
			}
				
			_abilityClicked = -1;
		}
	}
	
	void OnGUI () {
	    if (SameUnitsToSendInput.Count <= 0) {
	        return;
	    }
	    foreach(AbstractUnitComponent _component in SameUnitsToSendInput[0].GetAllUnitComponents) {
	        _component.GUIPriority();
	    }
				
	    GetUserInputForUnits(SameUnitsToSendInput);
	}

    public BaseUnit GetPriorityUnit()
    {
        if (_selectedUnits.Count > 0)
            return _selectedUnits[0];
        return null;
    }
	
	void GetUserInputForUnits(List<BaseUnit> units) {
		foreach(BaseUnit _currentUnit in units) {
		    if (PlayerManager.Player.PlayerRoles.Contains(_currentUnit.UnitRole))
		    {
		        //Verifica o input do jogador
		        foreach (AbstractUnitComponent _component in _currentUnit.GetAllUnitComponents)
		        {
		            _component.UserInputPriority();
		        }
		    }
		}
	}
	
	public void SetSelectedAbility(int ability) {
		_abilityClicked = ability;
	}

    public List<Ability> GetUnitAbilities() {
        if (SameUnitsToSendInput.Count <= 0) return null;

        return SameUnitsToSendInput[0].GetUnitComponent<AbilityLibraryComponent>().UnitAbilities;
    }

    public List<Pool> GetUnitPools() {
        if (SameUnitsToSendInput.Count <= 0) return null;
        var unitsConstructor = SameUnitsToSendInput[0].GetUnitComponent<ConstructorComponent>();
        return unitsConstructor == null ? null : unitsConstructor.BuildingsPools;
    }

    public List<Pool> GetStructurePools() {
        if (SameUnitsToSendInput.Count <= 0) return null;
        var structureProducer = SameUnitsToSendInput[0].GetUnitComponent<ProducerComponent>();
        return structureProducer == null ? null : structureProducer.Tier1UnitsPool;
    }

    public static BaseUnit GetSelectedBaseUnit() {
        if (InterfaceManager.Manager.SameUnitsToSendInput.Count <= 0) return null;

        return InterfaceManager.Manager.SameUnitsToSendInput[0];
    }

    public void SetAbilityClicked(Ability ability) {
        if (ability != null) {
            foreach (BaseUnit _currentUnit in SameUnitsToSendInput) {
                if (PlayerManager.Player.PlayerRoles.Contains(_currentUnit.UnitRole))
                {
                    var _abilityLibrary = _currentUnit.GetUnitComponent<AbilityLibraryComponent>();
                    if (_abilityLibrary != null)
                    {
                        _abilityLibrary.SelectAbility(ability);
                    }
                }
            }
        }
    }


}
