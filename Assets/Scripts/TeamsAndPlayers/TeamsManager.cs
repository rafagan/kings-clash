using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamsManager : MonoBehaviour {
	public int TeamsInGameAmout = 2;
	public List<TeamEntity> TeamsInCurrentGame;
	public UniqueIDsManager idsManager;
	public int teamIdOfLastPlayerAdded = 0;
	
	public void Awake () {
	}

	public void CreateTeams ()
	{
		//Verifica se não existe times criados, caso não exista, cria
		int _currentTeamsCount = transform.Find ("Teams").childCount;
		if (_currentTeamsCount < TeamsInGameAmout) {
			for (int i = _currentTeamsCount; i < TeamsInGameAmout; i++)
				this.CreateNewTeam ();
		}
	}
	
	public void RemoveAllTeams () {
		teamIdOfLastPlayerAdded = 0;
	
		if (TeamsInCurrentGame.Count > 0) {
			for (int i = TeamsInCurrentGame.Count - 1; i >= 0; i--) {
				TeamsInCurrentGame[i].RemoveAllPlayers();
			}	
		}
		
		idsManager.ClearAllIDs();
		var _count = transform.Find("Teams").childCount;
		for (int i = _count - 1; i >= 0; i --) {
			DestroyImmediate(transform.Find("Teams").GetChild(i).gameObject);
		}
		
		TeamsInCurrentGame.Clear();
	}
	
	public void AddNewPlayerToGame () {
		if (TeamsInCurrentGame != null && TeamsInCurrentGame.Count > 0) {
			if (TeamsInCurrentGame[teamIdOfLastPlayerAdded].TeamPlayers != null && TeamsInCurrentGame[teamIdOfLastPlayerAdded].TeamPlayers.Count >= 3)
				return;
			
			//Cria um novo player
			PlayerEntity _newPlayer = PlayerEntity.CreateNewPlayer();
			
			//Adiciona o player ao time
			this.AddPlayerToGame(_newPlayer);
		}
	}
	
	public void AddPlayerToGame (PlayerEntity player) {
		//Adiciona o player no time correto
		TeamsInCurrentGame[teamIdOfLastPlayerAdded].AddPlayer(player, false);
		
		//Soma o contador
		teamIdOfLastPlayerAdded++;
		
		if(teamIdOfLastPlayerAdded >= TeamsInGameAmout)
			teamIdOfLastPlayerAdded = 0;
	}
	
	private TeamEntity CreateNewTeam () {
		if (TeamsInCurrentGame.Count <= TeamsInGameAmout) {
			//Cria o gameObject do time
			GameObject _newTeam = new GameObject ();
			_newTeam.transform.position = Vector3.zero;
			_newTeam.transform.parent = transform.Find("Teams").transform;

			//Adiciona o componente de time
			TeamEntity _teamEntity = _newTeam.AddComponent<TeamEntity> ();
			_teamEntity.UniqueID = idsManager.GetNewUniqueID();
			_teamEntity.name = "Team0" + _teamEntity.UniqueID;

			//Adiciona a lista de times
			TeamsInCurrentGame.Add(_teamEntity);
		}
		
		return null;
	}
	
	public List<PlayerEntity> GetPlayersOfTeamID(int teamID) {
		if (teamID < TeamsInCurrentGame.Count) {
			for (int i = 0; i < TeamsInCurrentGame.Count; i++) {
				if (TeamsInCurrentGame[i].UniqueID == teamID)
					return TeamsInCurrentGame[i].TeamPlayers;
			}
		}
		
		return null;
	}
	
	public TeamEntity GetTeamOfID(int teamID) {
		if (teamID < TeamsInCurrentGame.Count) {
			for (int i = 0; i < TeamsInCurrentGame.Count; i++) {
				if (TeamsInCurrentGame[i].UniqueID == teamID)
					return TeamsInCurrentGame[i];
			}
		}
		
		return null;
	}
	
	public void RemoveTeam(int teamID) {
		if (teamID < TeamsInCurrentGame.Count) {
			for (int i = 0; i < TeamsInCurrentGame.Count; i++) {
				if (TeamsInCurrentGame[i].UniqueID == teamID) {
					TeamsInCurrentGame.RemoveAt(i);
					break;
				}
			}
		}
	}
}
