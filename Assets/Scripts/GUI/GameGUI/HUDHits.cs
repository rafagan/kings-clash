using UnityEngine;

public class HUDHits : MonoBehaviour {
    static public GameObject go;
    public GameObject HitPrefab;
    static public GameObject prefab;

	void Awake () {
        go = gameObject;
        prefab = HitPrefab;
	}

}