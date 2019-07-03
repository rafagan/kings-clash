using UnityEngine;
using System.Collections;

public class HUDResource : MonoBehaviour {

    private UILabel _plasmo;
    private UILabel _steam;
    private GameObject plasmoOb;
    private GameObject steamOB;

    void Start() {
        plasmoOb = transform.Find("Plasmo").gameObject;
        steamOB = transform.Find("Steam").gameObject;
        _plasmo = plasmoOb.transform.Find("PlasmoValue").GetComponent<UILabel>();
        _steam = steamOB.transform.Find("SteamValue").GetComponent<UILabel>();

    }

	void Update () {
	    var ether = ResourcesManager.Account.resourcesAmount[1]/ResourcesManager.Account.EtherMaxValue;
        //transform.FindChild("Ether").FindChild("EtherValue").GetComponent<UILabel>().text = ResourcesManager.Account.resourcesAmount[1].ToString();
        transform.Find("Ether").GetComponent<UISprite>().fillAmount = ether;


        if (PlayerManager.Player.PlayerRoles.Contains(PlayerRole.MONARCH)) {
            if(!plasmoOb.activeSelf)
                plasmoOb.SetActive(true);
            if (!steamOB.activeSelf)
                steamOB.SetActive(true);
            _plasmo.text = ResourcesManager.Account.resourcesAmount[2].ToString();
            _steam.text = ResourcesManager.Account.resourcesAmount[0].ToString();
        }
        else {
            if (plasmoOb.activeSelf)
                plasmoOb.SetActive(false);
            if (steamOB.activeSelf)
                steamOB.SetActive(false);
        }
	}
}
