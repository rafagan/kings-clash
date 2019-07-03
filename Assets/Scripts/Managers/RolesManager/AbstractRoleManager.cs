using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractRoleManager : MonoBehaviour {
	public List<BaseUnit> selfCharacters;
	
	public virtual void AddUnit(BaseUnit unit) {}
	public virtual void RemoveUnit(BaseUnit unit) {}
	public virtual bool Contains(BaseUnit unit) { return false; }
}
