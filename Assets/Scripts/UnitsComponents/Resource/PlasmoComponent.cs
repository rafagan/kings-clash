using UnityEngine;
using System.Collections;

public class PlasmoComponent : AbstractPrimaryResource {

	void Start () {
		if (baseUnit == null)
			baseUnit = transform.GetComponent<BaseUnit>();
		
		ResourceName = ResourceType.Plasmo;
		ResourcesLeft = 100;
	}
}
