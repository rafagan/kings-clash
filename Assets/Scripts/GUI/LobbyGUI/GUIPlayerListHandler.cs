using System.Collections.Generic;
using Net;
using UnityEngine;

public class GUIPlayerListHandler : MonoBehaviour {

    private UIGrid grid;
    public GameObject playerOnlinePrefab;
    private List<GameObject> playerList;
    private static GUIPlayerListHandler mInstance;

    void Start() {
        grid = GetComponent<UIGrid>();
        playerList = new List<GameObject>();
    }

    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }

    public static void RepositionGrid() {
        if (mInstance == null)
            return;
        mInstance.grid.Reposition();
    }

    void OnNetworkPlayerJoin(Player p) {
        var newPlayerObject = NGUITools.AddChild(gameObject, playerOnlinePrefab);
        var playerInfoGUI = newPlayerObject.GetComponent<GUIPlayerOnline>();
        playerInfoGUI.player = p;
        playerList.Add(newPlayerObject);
        grid.Reposition();
    }

    void OnNetworkPlayerLeave(Player p) {
        foreach (var searchPlayer in playerList) {
            var playerID = searchPlayer.GetComponent<GUIPlayerOnline>().player.id;
            if (playerID == p.id) {
                playerList.Remove(searchPlayer);
                grid.Reposition();
                return;
            }
        }
    }


}
