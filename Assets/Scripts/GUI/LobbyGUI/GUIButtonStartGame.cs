using UnityEngine;
using System.Collections;

public class GUIButtonStartGame : MonoBehaviour {

    void OnClick() {
        NetManager.StartGame();
    }
}
