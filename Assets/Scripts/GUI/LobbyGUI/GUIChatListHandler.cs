using UnityEngine;
using System.Collections;

public class GUIChatListHandler : MonoBehaviour {

    private UITextList textList;
    private static GUIChatListHandler mInstance;

    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }

    public static void ClearList() {
        if (mInstance == null)
            return;
        if (mInstance.textList == null)
            return;
        mInstance.textList.Clear();
    }

	// Use this for initialization
	void Start () {
	    textList = GetComponent<UITextList>();
	}
	
}
