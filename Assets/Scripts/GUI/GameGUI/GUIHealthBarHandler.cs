using UnityEngine;
using System.Collections;

public class GUIHealthBarHandler : MonoBehaviour {

    public UISprite BackgroundSprite = null;
    public UISprite ForegroundSprite = null;
    public UISlider slider = null;
    private Transform progressBar = null;

	void Start () {
        progressBar = transform.Find("Progress Bar");
        BackgroundSprite = progressBar.Find("Background").GetComponent<UISprite>();
        ForegroundSprite = progressBar.Find("Foreground").GetComponent<UISprite>();
        slider = progressBar.GetComponent<UISlider>();
	}

    public void ChangeValue(float currentLife, float maxLife) {

        // Size change
        var barPercent = ((100.0f * currentLife) / maxLife);
        if (slider != null) {
            if (currentLife < 2F) {
                slider.value = 0F;
                slider.ForceUpdate();
            }
            else
                slider.value = barPercent/100.0f;
        }

        // Color change
        Color c1 = new Color(0.90f, 0.21f, 0.0f);
        Color c2 = new Color(0.47f, 0.86f, 0.0f);
        Color c = Color.Lerp(c1, c2, (barPercent / 100.0f));

        if(ForegroundSprite != null)
            ForegroundSprite.color = c;
    }

    public void SetEnable(bool state) {
        if (BackgroundSprite != null && ForegroundSprite != null) {
            BackgroundSprite.enabled = state;
            ForegroundSprite.enabled = state;
        }
    }

    public void changeBackgroundColor(Color c) {
        if (BackgroundSprite != null ) {
            // Color change
            BackgroundSprite.color = c;
        }
    }

}
