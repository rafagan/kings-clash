using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonarchUnitsManager : AbstractRoleManager {
	public List<BaseUnit> selfStructures;

	public override void AddUnit(BaseUnit unit) {
		switch (unit.UnitType) {
			case ObjectType.CHARACTER:
				selfCharacters.Add(unit);
				break;
			case ObjectType.STRUCTURE:
				selfStructures.Add(unit);
				break;
		}
	}
	
	public override void RemoveUnit(BaseUnit unit) {
		switch (unit.UnitType) {
			case ObjectType.CHARACTER:
				selfCharacters.Remove(unit);
				break;
			case ObjectType.STRUCTURE:
				selfStructures.Remove(unit);
				break;
		}
	}
}
