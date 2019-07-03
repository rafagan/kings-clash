using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* 	
	Esta classe é responsável por gerênciar todas as unidades do jogo, inimigas e aliadas.
	Isso faz-se necessário, devido ao fato que temos que poder selecionar toda e qualquer unidade,
	independente se ela é categorizada como PROPRIA, ALIADA, INIMIGA ou NPC. O que deve ser filtrado,
	é o que o jogador pode fazer com essa unidade selecionada.
*/
using System.IO;

public class GameManager : MonoBehaviour {
	public static GameManager Manager;			//Atributo static para acesso fora da classe
	public float WaitTimeForInitialSetup = 15.0f;
	public bool ShowHealthBar = false;
	public bool IsConnected = false;
	public UnitsManager unitsManager;
	public PlayerManager player;
	public PoolsManager Pools;
	public ResourcesManager Resources;
	public bool GameOver = false;
    public Transform spectatorPosition;
	
	void Awake () {
		if (Manager != null)
			Destroy(Manager.gameObject);
		Manager = this;
		
		if (unitsManager == null)
			unitsManager = transform.parent.Find("UnitsManager").GetComponent<UnitsManager>();
		
		if (player == null)
			player = transform.parent.Find("PlayerManager").GetComponent<PlayerManager>();
		
		if (Pools == null)
			Pools = GameObject.Find("POOLMANAGERS").GetComponent<PoolsManager>();
			
		if (Resources == null)
			Resources = GameObject.Find("MANAGERS/ResourcesManager").GetComponent<ResourcesManager>();
	}
	
	void Start () {
		StartCoroutine("DoInitialSetup");
	}
	
	public void SetGameOver(int castleDestroyedTeamID) {
		GameOver = true;
	    Time.timeScale = 0;
        GUIEnd.SetCondition(castleDestroyedTeamID == PlayerManager.Player.PlayerTeam);
	}
	
	IEnumerator DoInitialSetup() {
	    GUILoading.ShowScreen();
		yield return new WaitForSeconds(WaitTimeForInitialSetup);

        //Atribui a role de acordo com o servidor
        PlayerManager.Player.PlayerRoles.Clear();

	    if (NetManager.isConnected)
	    {
	        while (NetManager.playerSynchronized == false)
	        {
	            yield return null;
	        }
	    }

	    switch (NetManager.playerRole)
	    {
            //Todas as roles
            case 0:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.MONARCH);
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.WARLEADER);
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.ARCHMAGE);
	            break;
            //Monarch
            case 4:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.MONARCH);
                break;
            //War/Arch
            case 1:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.WARLEADER);
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.ARCHMAGE);
                break;
            //War
            case 5:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.WARLEADER);
                break;
            //Arch
            case 2:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.ARCHMAGE);
                break;
            //Spectator
            case 3:
                PlayerManager.Player.PlayerRoles.Add(PlayerRole.SPECTATOR);
                break;
	    }
		
		//Posiciona a câmera sobre o castelo aliado
		FocusCameraToCastle();

        GUILoading.RemoveScreen();
	}
	
	void FocusCameraToCastle() {
	    if (PlayerManager.Player.Spectator)
	    {
            PlayerManager.Player.cameraController.FocusPosition(spectatorPosition.position);
	        return;
	    }

		//Procura castelo com o time adequado
		var _castlePoolId = PoolsManager.Manager.GetPoolIDByName("sCastlePool");
		var _castlePool = PoolsManager.Manager.GetPoolByID(_castlePoolId);
		if (_castlePool.InUseItens.Count > 0) {
			foreach (PoolItemComponent _poolItem in _castlePool.InUseItens) {
				BaseUnit _unit = _poolItem.baseUnit;
				if (_unit != null && _unit.UnitClass == Unit.Castle && _unit.TeamID == PlayerManager.Player.PlayerTeam) {
					PlayerManager.Player.cameraController.FocusUnit(_unit);
					return;
				}	
			}
		}
	}
}
