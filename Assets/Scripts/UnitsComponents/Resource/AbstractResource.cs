using UnityEngine;
using System.Collections;

public enum ResourceType { Steam = 0, Plasmo = 1, Ether = 2, Tree = 3 }
public class AbstractResource : AbstractUnitComponent {
    public ResourceType ResourceName;
     public override void GUIPriority(){}
     
     public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
