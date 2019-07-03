using UnityEngine;
using System.Collections;

public class LumberComponent : AbstractPrimaryResource {
    void Awake() {
        gameObject.transform.parent.GetComponentInChildren<CollectorComponent>().MyLumber = this;
        gameObject.SetActive(false);
    }

    void Start() {
        if (baseUnit == null)
            baseUnit = transform.GetComponent<BaseUnit>();

        ResourceName = ResourceType.Steam;
        ResourcesLeft = 100;
    }

    public void Stock() {
        var value = ResourcesLeft;
        Debug.Log(value);
        PlayerManager.Player.PlayerResources.AddResource(ResourceType.Steam, value);
        gameObject.SetActive(false);
    }

    void OnEnable() {
        GetComponent<MeshRenderer>().enabled = true;
    }

    void OnDisable() {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
