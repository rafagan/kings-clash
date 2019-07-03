using UnityEngine;
using System.Collections;

public class GUIEnd : MonoBehaviour {

    public UISprite BackgroundSprite;
    public UISprite MaskSprite;
    public UILabel TextLabel;

    private static GUIEnd mInstance;


    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }

    public static void SetCondition(bool condition) {
        if (mInstance == null)
            return;
        if (mInstance.BackgroundSprite == null || mInstance.MaskSprite == null || mInstance.TextLabel == null)
            return;
            
        mInstance.TextLabel.text = condition ? "You lose" : "You win";
        mInstance.BackgroundSprite.enabled = true;
        mInstance.MaskSprite.enabled = true;
        mInstance.TextLabel.enabled = true;
    }

}
