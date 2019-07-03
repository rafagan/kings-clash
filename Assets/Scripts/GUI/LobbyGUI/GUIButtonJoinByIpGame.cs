using UnityEngine;
using System.Collections;

public class GUIButtonJoinByIpGame : MonoBehaviour {

    public UILabel IpLabel;

    void OnClick() {
        if (IpLabel == null)
            return;
        NetManager.Connect(IpLabel.text);
        GUILobbyMessage.SetMessage("Joining...");
    }
}
