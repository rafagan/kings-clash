using UnityEngine;

public class HUDHealthBars : MonoBehaviour {
    static public GameObject go;
    public GameObject HealthBarPrefab;
    static public GameObject prefab;

    void Awake() {
        go = gameObject;
        prefab = HealthBarPrefab;
    }
}
