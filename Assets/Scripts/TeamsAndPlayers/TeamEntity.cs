using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamEntity : MonoBehaviour {
	public int UniqueID;
	public Color TeamColor;
	public List<PlayerEntity> TeamPlayers;
	
	private UniqueIDsManager idsManager;
	
	public void Awake () {
		if (idsManager == null)
			idsManager = gameObject.AddComponent<UniqueIDsManager>();
	}

	public bool AddPlayer(GameObject player, bool playerIsBot) {
		idsManager = transform.GetComponent<UniqueIDsManager>();
		if (idsManager == null)
			idsManager = gameObject.AddComponent<UniqueIDsManager>();
			
		if (TeamPlayers == null)
			TeamPlayers = new List<PlayerEntity>();
	
		//Inicia armazenando a referência da classe PlayerEntity
		PlayerEntity _playerEntity = player.GetComponent<PlayerEntity>();
		
		//Verifica as condições de erro ao adicionar um jogador
		if (player == null || _playerEntity == null || TeamPlayers.Count >= 3)
			return false;
		
		//Parenteia o gameObject do jogador, dentro do time	
		player.transform.parent = this.transform;
		
		//Configura o player
		_playerEntity.SetupPlayer(playerIsBot, idsManager.GetNewUniqueID(), UniqueID);
		player.name = "Player0" + _playerEntity.UniqueID + "OfTeam0" + this.UniqueID;
		
		//Insere o player, no topo da Queue
		TeamPlayers.Insert(0, _playerEntity);
		
		//Ajusta as roles dos jogadoes
		AdjustPlayersRolesInTeam();
		
		return true;
	}
	
	public void AddPlayer(PlayerEntity player, bool playerIsBot) {
		idsManager = transform.GetComponent<UniqueIDsManager>();
		if (idsManager == null)
			idsManager = gameObject.AddComponent<UniqueIDsManager>();
	
		this.AddPlayer(player.gameObject, playerIsBot);
	}
	
	public void RemoveAllPlayers () {
		if (TeamPlayers != null && TeamPlayers.Count > 0) {
			for (int i = TeamPlayers.Count - 1; i  >= 0 ; i--) {
				TeamPlayers[i].DestroyPlayer();
			}
			TeamPlayers.Clear();
		}
	}
	
	public bool RemovePlayer(GameObject player) {
		//Inicia armazenando a referência da classe PlayerEntity
		PlayerEntity _playerEntity = player.GetComponent<PlayerEntity>();
		
		//Verifica as condições de erro ao remover um jogador
		if (player == null || _playerEntity == null || TeamPlayers.Count <= 1 || !TeamPlayers.Contains(_playerEntity))
			return false;
		
		//Remove da Queue e destroi o gameObject
		TeamPlayers.Remove(_playerEntity);
		idsManager.RemoveUniqueID(_playerEntity.UniqueID);
		_playerEntity.DestroyPlayer();
		
		//Reordena os roles do jogador
		AdjustPlayersRolesInTeam();
		return true;
	}
	
	public void RemovePlayer(PlayerEntity player) {
		this.RemovePlayer(player.gameObject);
	}
	
	private void AdjustPlayersRolesInTeam () {
		//Verifica quantos jogadores tem no time
		int _playersInTeam = TeamPlayers.Count; 
		
		//Faz os ajustes necessários
		switch (_playersInTeam) {
			case 1: {
				TeamPlayers[0].PlayerRoles = new PlayerRole[]{ PlayerRole.MONARCH, PlayerRole.WARLEADER, PlayerRole.ARCHMAGE };
				break;
			}
			case 2: {
				TeamPlayers[0].PlayerRoles = new PlayerRole[]{ PlayerRole.WARLEADER, PlayerRole.ARCHMAGE };
				TeamPlayers[1].PlayerRoles = new PlayerRole[]{ PlayerRole.MONARCH };
				break;
			}
			case 3: {
				TeamPlayers[0].PlayerRoles = new PlayerRole[]{ PlayerRole.ARCHMAGE };
				TeamPlayers[1].PlayerRoles = new PlayerRole[]{ PlayerRole.WARLEADER };
				TeamPlayers[2].PlayerRoles = new PlayerRole[]{ PlayerRole.MONARCH };
				break;
			}
		}
	}
}
