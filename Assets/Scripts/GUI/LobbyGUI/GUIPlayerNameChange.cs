using UnityEngine;
using System.Collections;

public class GUIPlayerNameChange : MonoBehaviour {

    private UIInput playerNameInput;

    void Awake() {
        playerNameInput = GetComponent<UIInput>();
    }

    public void OnSubmit() {
	    if (playerNameInput == null)
	        return;
        NetManager.playerName = playerNameInput.value;

    }

    public void ValidateGroup() {
        playerNameInput = GetComponent<UIInput>();
        if (playerNameInput == null)
            return;
        playerNameInput.value = NetManager.playerName;
    }
}
