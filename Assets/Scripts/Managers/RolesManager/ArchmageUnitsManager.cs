using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArchmageUnitsManager : AbstractRoleManager {
	public override void AddUnit(BaseUnit unit) {
		selfCharacters.Add(unit);
	}
	
	public override void RemoveUnit(BaseUnit unit) {
		selfCharacters.Remove(unit);
	}
	
}
