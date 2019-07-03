using UnityEngine;
using System.Collections;

public class PlasmoStageControl : MonoBehaviour {
	public GameObject stage1;
	public GameObject stage2;
	public GameObject stage3;
	private int initNodes;
	// Use this for initialization
	void Start () {
		stage1.SetActive(true);
		stage2.SetActive(false);
		stage3.SetActive(false);
		initNodes = transform.GetComponent<CrudeResourceComponent>().NodesLeft;
	}
	
	// Update is called once per frame
	void Update () {
		float porcent = (transform.GetComponent<CrudeResourceComponent>().NodesLeft*100)/initNodes;
		if(porcent>=75){
			stage1.SetActive(true);
			stage2.SetActive(false);
			stage3.SetActive(false);
		}else if(porcent<75 && porcent>=45){
			stage1.SetActive(false);
			stage2.SetActive(true);
			stage3.SetActive(false);
		}else{
			stage1.SetActive(false);
			stage2.SetActive(false);
			stage3.SetActive(true);
		}
	
	}
}
