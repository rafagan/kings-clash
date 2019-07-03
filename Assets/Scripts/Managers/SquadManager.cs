using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadManager : MonoBehaviour {
	public List<BaseUnit>[] Squads;
	public GameManager manager;
	
	private List<BaseUnit> selectedUnits;

	void Start () {
		Squads = new List<BaseUnit>[10];
		for (int i = 0; i < 10; i++)
			Squads[i] = new List<BaseUnit>();
		
		selectedUnits = transform.GetComponent<UnitsManager>().SelectedUnits;
	}
	
	void Update () {
		CreateSquad();
		GetSquad();
		CheckSquadRole();
	}

	private void CreateSquad() {
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)) {
			int _key = ReturnAlphaNumber();

			if (_key != 99 && selectedUnits.Count > 0) {
				Squads[_key].Clear();

				foreach(BaseUnit _unit in selectedUnits) {
					if (!_unit.IsEnemy || PlayerManager.Player.CheckPlayerRole(_unit.UnitRole))
						Squads[_key].Add(_unit);
				}
			}
		}
	}

	private void GetSquad() {
		int _key = ReturnAlphaNumber();

		if (_key != 99 && Squads[_key].Count > 0) {
			manager.unitsManager.ClearSelectedList();

			foreach(BaseUnit _unit in Squads[_key])
				manager.unitsManager.AddToSelectedList(_unit);
		}
	}
	
	private void CheckSquadRole() {
		if(Squads.Length > 0) {
			foreach(List<BaseUnit> _squads in Squads) {
				if (_squads.Count > 0) {
					for (int i = _squads.Count - 1; i >= 0; i--) {
						if (!PlayerManager.Player.CheckPlayerRole(_squads[i].UnitRole))
							_squads.Remove(_squads[i]);
					}
				}
			}
		}
	}

	private int ReturnAlphaNumber() {
		if (Input.GetKeyUp(KeyCode.Alpha1))
			return 1;
		else if (Input.GetKeyUp(KeyCode.Alpha2))
			return 2;
		else if (Input.GetKeyUp(KeyCode.Alpha3))
			return 3;
		else if (Input.GetKeyUp(KeyCode.Alpha4))
			return 4;
		else if (Input.GetKeyUp(KeyCode.Alpha5))
			return 5;
		else if (Input.GetKeyUp(KeyCode.Alpha6))
			return 6;
		else if (Input.GetKeyUp(KeyCode.Alpha7))
			return 7;
		else if (Input.GetKeyUp(KeyCode.Alpha8))
			return 8;
		else if (Input.GetKeyUp(KeyCode.Alpha9))
			return 9;
		else if (Input.GetKeyUp(KeyCode.Alpha0))
			return 0;
		else
			return 99;
	}
}
