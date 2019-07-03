using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Lista de todas as unidades controláveis do jogo, para controle de Upgrades
public enum Unit { 
	Blacksmith, Gladiator, Monarch, Crow, Bowman, Sourceress, CougarRide, Catapult, DrakeKnight, Sharka, Assassin, Titan, 
	Barrack, Castle, Forge, DefenseTower, Cauldron, Temple
     			 }

public class UnitsManager : MonoBehaviour {

	public List<BaseUnit> CharactersInScene;	//Lista que armazena refência de todas as unidades móveis da cena
	public List<BaseUnit> StructuresInScene;	//Lista que armazena refência detodas as estruturas da cena
	public List<BaseUnit> SelectedUnits;		//Lista de unidades selecionadas pelo jogador
	public List<BaseUnit> NpcsInScene;			//Lista de unidades NPCs
	
	private MonarchUnitsManager monarchManager;
	private WarleaderUnitsManager warleaderManager;
	private ArchmageUnitsManager archmageManager;
	private SquadManager squadManager;

	void Awake () {
		CharactersInScene = new List<BaseUnit>();
		StructuresInScene = new List<BaseUnit>();
		SelectedUnits = new List<BaseUnit>();
		NpcsInScene = new List<BaseUnit>();
		
		monarchManager = transform.GetComponent<MonarchUnitsManager>();
		warleaderManager = transform.GetComponent<WarleaderUnitsManager>();
		archmageManager = transform.GetComponent<ArchmageUnitsManager>();
		squadManager = transform.GetComponent<SquadManager>();
	}

	void Start () {
		squadManager.manager = GameManager.Manager;
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape) && SelectedUnits.Count > 0) {
			ClearSelectedList();
		}
	}

	public void AddUnitInScene(BaseUnit unit) {
		if (unit == null) return;
		
		switch (unit.UnitType) {
		case ObjectType.CHARACTER:
			CharactersInScene.Add(unit);
			break;
		case ObjectType.STRUCTURE:
			StructuresInScene.Add(unit);
			break;
		}
	
		switch (unit.UnitRole) {
		case PlayerRole.MONARCH:
			monarchManager.AddUnit(unit);
			break;
		case PlayerRole.WARLEADER:
			warleaderManager.AddUnit(unit);
			break;
		case PlayerRole.ARCHMAGE:
			archmageManager.AddUnit(unit);
			break;
		}
	}

    public void RemoveUnitInScene(BaseUnit unit)
    {
        if (CharactersInScene.Contains(unit))
        {
            CharactersInScene.Remove(unit);
        } else if (StructuresInScene.Contains(unit))
        {
            StructuresInScene.Remove(unit);
        }

        if (monarchManager.selfCharacters.Contains(unit) || monarchManager.selfStructures.Contains(unit))
        {
            monarchManager.RemoveUnit(unit);
        } else if (warleaderManager.selfCharacters.Contains(unit))
        {
            warleaderManager.RemoveUnit(unit);
        } else if (archmageManager.selfCharacters.Contains(unit))
        {
            archmageManager.RemoveUnit(unit);
        }
    }

	public void AddToSelectedList(BaseUnit unit) {
		if (unit == null) return;
			
		unit.IsSelected = true;
		SelectedUnits.Add(unit);
	}
	
	public void RemoveOfSelectedList(BaseUnit unit) {
		if (unit == null) return;

		unit.IsSelected = false;
		SelectedUnits.Remove(unit);
	}

	public void AddToSelectedList(BaseUnit[] unit) {
		if (unit.Length <= 0) return;

		foreach (BaseUnit _unit in unit) {
			_unit.IsSelected = true;
			SelectedUnits.Add(_unit);
		}
	}

	public void ClearSelectedList() {
		if (SelectedUnits.Count <= 0) return;

		foreach (BaseUnit _unit in SelectedUnits)
			_unit.IsSelected = false;

		SelectedUnits.Clear();
	}
}
