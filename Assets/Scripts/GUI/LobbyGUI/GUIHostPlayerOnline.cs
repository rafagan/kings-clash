using System;
using UnityEngine;
using System.Collections;

public class GUIHostPlayerOnline : MonoBehaviour {

    public UILabel NameLabel;
    public UILabel PingLabel;
    public UILabel RoleLabel;
    public UILabel TeamLabel;

    // Update is called once per frame
    void Update() {

        if (NameLabel != null)
            NameLabel.text = NetManager.playerName;
        if (PingLabel != null)
            PingLabel.text = NetManager.player.ping.ToString();
        var role = "ERROR";
        if (RoleLabel != null) {
            switch (NetManager.playerRole) {
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
                TeamLabel.text = (NetManager.playerTeamID + 1).ToString();

            }
        }
    }
}
