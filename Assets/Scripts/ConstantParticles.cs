using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ConstantParticles : MonoBehaviour {
	
	public List<GameObject> ParticlesArray;
	//private bool firstCreated;
	#region StructureComponent _structure
	private StructureComponent _structureBk;
	private StructureComponent _structure {
		get {
			if (_structureBk == null)
				_structureBk = transform.GetComponent<StructureComponent>();
			return _structureBk;
		}
		set { _structureBk = value; }
	}
	#endregion

	// Use this for initialization
	void Awake () {
		ResetMe();
		//firstCreated = true;
		_structure = transform.GetComponent<StructureComponent>();
	}

	void ResetMe(){
		if(transform.GetComponent<StructureComponent>().built) return;
		foreach(GameObject particle in ParticlesArray) {
			if(particle != null)
				particle.SetActive(false);
		}
	}

	void OnEnable() {
		if (_structure != null)
			StartCoroutine(UseParticle());
	}

	void OnDisable(){
		ResetMe();
	}
	
	private IEnumerator UseParticle() {

		while(!_structure.built)
			yield return new WaitForEndOfFrame();

		foreach (GameObject particle in ParticlesArray) {
			if (particle != null)
				particle.SetActive(true);
		}

	}

}
