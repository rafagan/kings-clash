using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ProducerParticle : MonoBehaviour {
	public List<GameObject> ParticlesArray;
	// Use this for initialization
	void Start () {
	
	}
	void setParticlesOn(bool isOn){
		foreach(GameObject particle in ParticlesArray){
			if(particle != null)
				particle.SetActive(isOn);
		}
	}
	// Update is called once per frame
	void Update () {
		//bool isBuilt = transform.GetComponent<StructureComponent>().built;
		if(transform.GetComponent<ProducerComponent>().unitTrainQueue.Count>0){
			setParticlesOn(true);
		}else{
			setParticlesOn(false);
		}
	}
}
