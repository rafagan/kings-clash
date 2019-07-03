using UnityEngine;
using System.Collections;

public class GUIButtonDisconnectGame : MonoBehaviour {

	
    void OnClick() {
        NetManager.Disconnect();
        GUILobbyMessage.SetMessage("You have been disconnected");
    }
}
