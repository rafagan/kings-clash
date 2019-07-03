using UnityEngine;
using System.Collections;

public abstract class AbstractUnitComponent : MonoBehaviour {
	[HideInInspector] public BaseUnit baseUnit;
	public abstract void GUIPriority();
	public abstract void UserInputPriority();
	public abstract void Reset();
}
