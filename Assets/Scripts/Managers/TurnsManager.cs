using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnsManager : MonoBehaviour {
	public List<float> TurnTime; 
	//public OfflineClient ClientComponent;
	public float TurnChangeTime = 0.2f;
	public int CurrentTurn = 0;
	public bool TurnEnded = false;
	
	void Awake () {	
		//if (ClientComponent == null) 
			//ClientComponent = GameObject.Find("SERVERS/OFFLINE/OfflineClient").GetComponent<OfflineClient>();
	}

	void Start () {
		TurnTime = new List<float>();
		StartTurn();
	}
	
	public void StartTurn() {
		StartCoroutine("RunTurn");	
	}
	
	private void AddTurn() {
		TurnTime.Insert(TurnTime.Count, Time.fixedTime);
	}
	
	private IEnumerator RunTurn() {
		TurnEnded = false;
		
		AddTurn();
		CurrentTurn++;
		yield return new WaitForSeconds(TurnChangeTime);
		
		TurnEnded = true;
	}
}
