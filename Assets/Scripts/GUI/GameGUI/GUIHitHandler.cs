using UnityEngine;
using System.Collections;

public class GUIHitHandler : MonoBehaviour {

    public HUDText _mText = null;

	void Start () {
        _mText = transform.GetComponentInChildren<HUDText>();
	}

}
