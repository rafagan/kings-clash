using UnityEngine;
using System.Collections;

public abstract class AbstractCommand : MonoBehaviour {
	public int CommandID;
	public BaseUnit OwnerUnit;
	public bool CommandEnded = false;

	public abstract void Execute (Mail mail);
	public abstract IEnumerator CommandRoutine ();

}
