using UnityEngine;
using System.Collections;

public class GUIWindowManager : MonoBehaviour {


    public UITweener MainPanelTweener;
    public UITweener LobbyPanelTweener;
    public UITweener RoomPanelTweener;
    public bool _mainMenu = true;

	
	void Update () {
        if (MainPanelTweener != null) {
            if (_mainMenu) {
                MainPanelTweener.PlayForward();
                LobbyPanelTweener.PlayReverse();
                RoomPanelTweener.PlayReverse();
            }
            else {
                MainPanelTweener.PlayReverse();

                if (NetManager.isConnected) {
                    RoomPanelTweener.PlayForward();
                    LobbyPanelTweener.PlayReverse();
                }
                else {
                    RoomPanelTweener.PlayReverse();
                    LobbyPanelTweener.PlayForward();
                }
            }
        }
	}

    public void PlayButtonPressed() {
        _mainMenu = false;
    }
    public void BackButtonPressed() {
        _mainMenu = true;
    }

    /// <summary>
    /// Retorna ao menu principal se o jogador deixar o canal.
    /// Esta mensagem é enviada quando o jogador deixa um canal.
    /// </summary>

    void OnNetworkLeaveChannel() {
        if(Application.loadedLevelName.Equals(NetManager.gameScene))
            Application.LoadLevel(NetManager.menuScene);
    }
}
