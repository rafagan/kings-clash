using UnityEngine;
using System.Collections;

public class GUIEtherTooltip : MonoBehaviour {

    //NGUI OnTooltip Event
    void OnTooltip(bool show) {
        if (show) {
            var ether = "Ether: ";
            ether += (int)ResourcesManager.Account.resourcesAmount[1] + " of ";
            ether += ResourcesManager.Account.EtherMaxValue;
            UITooltip.ShowText(ether);
            return;
        }
        UITooltip.ShowText(null);
    }
}
