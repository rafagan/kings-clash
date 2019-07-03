using UnityEngine;
using System.Collections;

public class VeterancyComponent : MonoBehaviour {

	// Use this for initialization
	BaseUnit baseUnit ;
	public float EtherToVeteran;
	public float EtherToElite; //so vale pro Monarca
	
	[HideInInspector] public bool isVeteran = false;
	
	void Start () {
		baseUnit = this.gameObject.GetComponent<BaseUnit>();
	}
	
	// Update is called once per frame
	void Update () {
		if(baseUnit != null && !isVeteran) CheckUnitLevel();
	}
	
	
	private void CheckUnitLevel(){
		float exp = baseUnit.GetUnitComponent<AttributesComponent>().Ether;		
		if(exp >= EtherToVeteran)
			isVeteran = true;
	}
		
}
