using UnityEngine;
using System.Collections;

public class GUIBackButtonHandler : MonoBehaviour {

    public GameObject repositionTarget;

    void OnClick() {
        repositionTarget.transform.position = Vector3.zero;
        //NGUITools.SetActive(gameObject, false);
    }
}
