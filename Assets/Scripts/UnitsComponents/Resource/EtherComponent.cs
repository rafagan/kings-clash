using UnityEngine;
using System.Collections;

public class EtherComponent : AbstractPrimaryResource {

	public void Start () {
		ResourceName = ResourceType.Ether;
		ResourcesLeft = 100;
	}
}
