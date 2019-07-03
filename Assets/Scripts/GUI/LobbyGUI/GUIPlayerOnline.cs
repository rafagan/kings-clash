using Net;
using UnityEngine;

public class GUIPlayerOnline : MonoBehaviour {

    public UILabel NameLabel;
    public UILabel PingLabel;
    public UILabel RoleLabel;
    public UILabel TeamLabel;
    public Player player;

	
	// Update is called once per frame
	void Update () {
	    if (player == null)
	        return;

        if (NameLabel != null)
            NameLabel.text = player.name;
        if (PingLabel != null)
            PingLabel.text = player.ping.ToString();
        var role = "ERROR";
	    if (RoleLabel != null) {
            switch (player.role) {
                case 0:
                    role = "MON/WAR/ARC";
                    break;
                case 1:
                    role = "WARL/ARCH";
                    break;
                case 2:
                    role = "ARCHMAGE";
                    break;
                case 3:
                    role = "SPECTATOR";
                    break;
                case 4:
                    role = "MONARCH";
                    break;
                case 5:
                    role = "WARLEADER";
                    break;
            }
            RoleLabel.text = role;
	    }
	    if (TeamLabel != null) {
            if (role.Equals("SPECTATOR"))
	            TeamLabel.text = "";
            else {
                TeamLabel.text = (player.teamId + 1).ToString();
                
            }
	    }
	}


    void OnNetworkPlayerLeave(Player p) {
        if (player.id == p.id) {
            Destroy(gameObject);
            GUIPlayerListHandler.RepositionGrid();
        }
    }
}
