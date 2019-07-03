using UnityEngine;
using System.Collections;

public class GUILoading : MonoBehaviour {

    public UISprite BackgroundSprite;
    public UISprite MaskSprite;
    public UILabel LoadingLabel;

    private static GUILoading mInstance;

    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }
    
    public static void ShowScreen() {
        if (mInstance == null)
            return;

        if (mInstance.BackgroundSprite == null || mInstance.MaskSprite == null || mInstance.LoadingLabel == null)
            return;
        mInstance.BackgroundSprite.enabled = true;
        mInstance.MaskSprite.enabled = true;
        mInstance.LoadingLabel.enabled = true;
    }

    public static void RemoveScreen() {
        if (mInstance == null)
            return;

        if (mInstance.BackgroundSprite == null || mInstance.MaskSprite == null || mInstance.LoadingLabel == null)
            return;
        mInstance.BackgroundSprite.enabled = false;
        mInstance.MaskSprite.enabled = false;
        mInstance.LoadingLabel.enabled = false;
    }
}
