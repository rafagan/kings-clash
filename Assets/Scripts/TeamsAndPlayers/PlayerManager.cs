using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerRole { MONARCH = 1, WARLEADER = 2, ARCHMAGE = 3, NPC = 4, RESOURCE = 5, SPECTATOR = 6 }

public class PlayerManager : MonoBehaviour {
	public static PlayerManager Player;	
	public int PlayerID = 1;
	public int PlayerTeam = 1;
	public List<PlayerRole> PlayerRoles;
	public ResourcesManager PlayerResources;
	public RTSController rtsController;
	public ClickController clickController;
	public CameraController cameraController;

    private bool spectator = false;
	private bool canSelect = false;
	
	public bool CanSelect { get { return canSelect; } 
		set {
			canSelect = value;
			rtsController.ClearMousePositions();
			rtsController.CanControl = canSelect;
		} }
    public bool Spectator { 
        get { return spectator; } 
        set { spectator = value;
            if (spectator) PlayerTeam = 5;
        } }
	
	void Awake () {
		if (Player != null)
			Destroy(Player.gameObject);
		Player = this;
		
		if (rtsController == null)
			rtsController = transform.GetComponent<RTSController>();
		
		if (PlayerResources == null)
			PlayerResources = GameObject.Find("MANAGERS/ResourcesManager").GetComponent<ResourcesManager>();
		
		if (clickController == null)
			clickController = transform.GetComponent<ClickController>();
			
		if (cameraController == null)
			cameraController = transform.GetChild(0).GetComponent<CameraController>();
		
		CanSelect = true;

        PlayerTeam = NetManager.playerTeamID;
	}

    public void Start()
    {
        if (PlayerRoles.Contains(PlayerRole.SPECTATOR))
        {
            Spectator = true;
        }
    }
	
	public List<PlayerRole> GetPlayerRoles() {
		return PlayerRoles;	
	}

	/*public void AddUserRole(PlayerRole role) {
		if (!PlayerRoles.Contains(role))
			PlayerRoles.Add(role);
	}
	
	public void RemovePlayerRole(PlayerRole role) {
		if (PlayerRoles.Contains(role))
			PlayerRoles.Remove(role);
	}*/
	
	public List<int> GetPlayerRole() {
		List<int> _roles = new List<int>();
		if (PlayerRoles.Count > 0) {
			foreach (PlayerRole role in PlayerRoles)
				_roles.Add((int)role);
		}
		return _roles;
	}
	
	public bool CheckPlayerRole(PlayerRole role) {
		return PlayerRoles.Contains(role);
	}
	
	public bool CheckPlayerRole(int role) {
		return PlayerRoles.Contains((PlayerRole)role);
	}
}
