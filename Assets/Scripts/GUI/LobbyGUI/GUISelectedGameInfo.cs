using Net;
using UnityEngine;
using System.Collections;

public class GUISelectedGameInfo : MonoBehaviour {

    private static GUISelectedGameInfo mInstance = null;
    public UILabel _gameLabel;
    public UILabel _hostLabel;
    public UILabel _playeysLabel;

    private ServerList.Entry _serverSelected;

    public static void SetSelectedGame(ServerList.Entry server) {
        if (mInstance == null)
            return;
        mInstance._serverSelected = server;
    }

    public static ServerList.Entry GetSelectedGame() {
        if (mInstance == null)
            return null;
        return mInstance._serverSelected;
    }

    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }

	// Use this for initialization
    void Start() {
	
	}
	
    void Update() {
        if (mInstance == null || _serverSelected == null)
            return;
            mInstance._gameLabel.text = mInstance._serverSelected.name;
        if (mInstance._hostLabel != null)
            mInstance._hostLabel.text = mInstance._serverSelected.host;
        if (mInstance._playeysLabel != null)
            mInstance._playeysLabel.text = mInstance._serverSelected.playerCount.ToString();

    }
}
