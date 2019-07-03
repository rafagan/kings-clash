using Net;
using UnityEngine;
using System.Collections;

public class GUIServerGame : MonoBehaviour {

    public ServerList.Entry _server;
    public UILabel _gameLabel;
    public UILabel _hostLabel;
    public UISprite _StatusSprite;

    
    void OnClick() {
        if(_server != null)
            GUISelectedGameInfo.SetSelectedGame(_server);
    }

    void Update() {
        if (_server == null)
            return;

        if (_gameLabel != null)
            _gameLabel.text = _server.name;
        if (_hostLabel != null)
            _hostLabel.text = _server.host;
        if (_StatusSprite != null)
            _StatusSprite.color = _server.joinInGame ? Color.green : Color.red;
    }
}
