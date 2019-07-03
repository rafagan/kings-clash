using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEntity : MonoBehaviour {
	public int UniqueID;
	public int TeamID;
	public string PlayerName;
	public PlayerRole[] PlayerRoles;
	public bool PlayerIsBot;
	
	public void SetupPlayer (bool playerIsBot, int playerID, int teamID) {
		this.UniqueID = playerID;
		this.TeamID = teamID;
		this.PlayerIsBot = playerIsBot;
	}
	
	public void DestroyPlayer () {
		//Executa lógicas de finalização de player na rede, caso esteja conectado
	
		DestroyImmediate(this.gameObject);
	}
	
	public static PlayerEntity CreateNewPlayer () {
		GameObject _newPlayer = new GameObject();
		_newPlayer.transform.position = Vector3.zero;
		
		PlayerEntity _playerEntity = _newPlayer.AddComponent<PlayerEntity>();		
		return _playerEntity;
	}
}
