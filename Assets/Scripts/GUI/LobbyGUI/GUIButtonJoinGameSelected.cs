using UnityEngine;
using System.Collections;

public class GUIButtonJoinGameSelected : MonoBehaviour {

    void OnClick() {
        var gameSelected = GUISelectedGameInfo.GetSelectedGame();
        if (gameSelected != null) {
            if (NetManager.serverList.Contains(gameSelected)) {
                if (NetManager.ConnectGame(gameSelected, "")) {
                    GUILobbyMessage.SetMessage("Connecting...");
                    GUIChatListHandler.ClearList();
                }
                else {
                    GUILobbyMessage.SetMessage("Wrong password...");
                }
            }
            else {
                GUILobbyMessage.SetMessage("The server selected no longer exists");
            }
        }
        else {
            GUILobbyMessage.SetMessage("You have no selected game");
        }
    }
}
