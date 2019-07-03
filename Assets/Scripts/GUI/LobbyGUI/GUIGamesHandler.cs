using System.Collections.Generic;
using Net;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GUIGamesHandler : MonoBehaviour {

    private UIGrid grid;
    public GameObject entryServerPrefab;
    private List<GameObject> serverList; 

    private int _lastSize = 0;

	void Start () {
	    grid = GetComponent<UIGrid>();
	    serverList = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
        if (_lastSize != NetManager.serverList.size) {
            ClearButtons();
            for (int i = 0; i < NetManager.serverList.size; ++i) {
                ServerList.Entry ent = NetManager.serverList[i];
                var queueButton = NGUITools.AddChild(gameObject, entryServerPrefab);
                var buttonServer = queueButton.GetComponent<GUIServerGame>();
                buttonServer._server = ent;
                serverList.Add(queueButton);
            }
            grid.Reposition();
            _lastSize = NetManager.serverList.size;
        }
	}

    private void ClearButtons() {
        if (serverList.Count > 0) {
            for (int i = serverList.Count - 1; i >= 0; i--) {
                var _buttonToDestroy = serverList[i];
                serverList.RemoveAt(i);
                Destroy(_buttonToDestroy);
            }
        }
    }
}
