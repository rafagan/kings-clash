using Net;
using UnityEngine;

public class GUIChatInput : NetBehaviour {

    public UITextList textList;

    private UIInput mInput;

    void Start() {
        mInput = GetComponent<UIInput>();
    }

    /// <summary>
    /// Pressing 'enter' should immediately give focus to the input field.
    /// </summary>

    void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            if (!mInput.isSelected) {
                mInput.label.maxLineCount = 1;
                mInput.isSelected = true;
            }
        }
    }

    /// <summary>
    /// Submit notification is sent by UIInput when 'enter' is pressed or iOS/Android keyboard finalizes input.
    /// </summary>

    public void OnSubmit() {
        if (textList != null) {
            // It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
            string text = NGUIText.StripSymbols(mInput.value);

            if (!string.IsNullOrEmpty(text)) {
                Package.Send("OnChat", Target.All, NetManager.playerID, text);
                mInput.value = "";
                mInput.isSelected = false;
            }
        }
    }

    
    /// <summary>
    /// Notification of a new player joining the channel.
    /// </summary>

    void OnNetworkPlayerJoin(Player p) {
        textList.Add("[3A9E00]" + p.name + " has joined. [-]");
    }	
    
    
    /// <summary>
    /// Notification of another player leaving the channel.
    /// </summary>

    void OnNetworkPlayerLeave(Player p) {
        textList.Add("[C90000]" + p.name + " has left. [-]");
    }



    /// <summary>
    /// This is our chat callback. As messages arrive, they simply get added to the list.
    /// </summary>

    [RFC]
    void OnChat(int playerID, string text) {
        // Figure out who sent the message and add their name to the text
        Player player = NetManager.GetPlayer(playerID);
        if (player.id == NetManager.playerID) {
            textList.Add("[0015B8]" + player.name + ": [-]" + text);
        } else {
            textList.Add("[005BD1]" + player.name + ": [-]" + text);
        }
    }

}
