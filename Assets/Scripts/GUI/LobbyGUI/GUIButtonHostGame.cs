using Net;
using UnityEngine;
using System.Collections;

public class GUIButtonHostGame : MonoBehaviour {

    private UISprite sprite;
    public UILabel playerName;
    public UILabel gameName;
    public UILabel gamePassword;

	// Use this for initialization
	void Start () {
	    sprite = GetComponent<UISprite>();
        if (sprite != null) {
            if (sprite.hasBoxCollider) {
                sprite.autoResizeBoxCollider = true;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick() {
        if (gameName == null || playerName == null || gamePassword == null)
            return;
        NetManager.playerName = playerName.text;
        NetManager.HostGame(NetUdpLobbyClient.Port, gameName.text, gamePassword.text, 7, true);
        GUIChatListHandler.ClearList();
        GUILobbyMessage.SetMessage("The game has been created");
    }
}
