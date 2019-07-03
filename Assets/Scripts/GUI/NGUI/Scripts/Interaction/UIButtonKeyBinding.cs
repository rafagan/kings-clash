//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This class makes it possible to activate a button by pressing a key (such as space bar for example).
/// </summary>

[AddComponentMenu("Game/UI/Button Key Binding")]
public class UIButtonKeyBinding : MonoBehaviour
{
    public KeyCode keyCode = KeyCode.None;

    public enum Trigger {
        OnClick,
        OnRelease,
    }

    public Trigger trigger = Trigger.OnClick;

	void Update ()
	{
		if (!UICamera.inputHasFocus)
		{
			if (keyCode == KeyCode.None) return;
			
			if (Input.GetKeyDown(keyCode)) {
                switch (trigger) {
                    case Trigger.OnRelease:
                        SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                        break;
                    case Trigger.OnClick:
                        SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                        break;
                }
			}

			if (Input.GetKeyUp(keyCode)) {
                switch (trigger) {
                    case Trigger.OnRelease:
                        SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                        SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                        break;
                }
			}
		}
	}
}
