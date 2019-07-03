using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PreviewStructure : MonoBehaviour {
	public Pool PoolToSpawn;
	public BaseUnit LastBaseUnitOrder;
    public float ThresholdTerrainColision = 1;
    public bool AllowGeyser = false;
    private bool _blocked = false;
    private bool _blockedByMesh = false;
    private bool _blockedByRay = false;
    private bool _connectedToGeyser = false;
    private Vector3 _geyserPosition = Vector3.zero;
    private int _geyserID;
    private MeshRenderer mesh;
    private Ray raycast;
    private RaycastHit hit;

    void Start() {
        mesh = transform.Find("Mesh").GetComponent<MeshRenderer>();
    }

	void Update () {

	    if (AllowGeyser == false)
	    {
            _blocked = (CheckUnevenTerrainColision() || _blockedByMesh || _blockedByRay);
	    }
        else
        {
            _blocked = (_blockedByMesh || _blockedByRay);
        }

        if (mesh != null) {
            foreach (var material in mesh.materials)
            {
                material.SetColor("_Color", _blocked ? Color.red : Color.green);
            }
        }

		if(gameObject.activeInHierarchy)
			PlayerManager.Player.CanSelect = false;

       
        transform.position = GetMouseWorldPos();

		if (Input.GetMouseButtonDown(0) && _blocked == false) {
		    if (AllowGeyser && _connectedToGeyser)
		    {
                MailMan.Post.NewMail(new Mail("Spawn", LastBaseUnitOrder.UniqueID, PoolToSpawn.PoolUniqueID, _geyserPosition, PlayerManager.Player.PlayerTeam, _geyserID));
		    }
            else
                MailMan.Post.NewMail(new Mail("Spawn", LastBaseUnitOrder.UniqueID, PoolToSpawn.PoolUniqueID, GetMouseWorldPos(), PlayerManager.Player.PlayerTeam, -1));
			gameObject.SetActive(false);
			GameManager.Manager.unitsManager.ClearSelectedList();
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			gameObject.SetActive(false);
			PlayerManager.Player.CanSelect = true;
		}
	}

    bool CheckUnevenTerrainColision() {
        var vertices = new Vector3[4];
        var center = GetComponent<Collider>().bounds.center;
        var extents = GetComponent<Collider>().bounds.extents;
        vertices[0] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
        vertices[1] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
        vertices[2] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
        vertices[3] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
        var diffs = 0.0f;
        foreach (var vec in vertices) {
            var hit = new RaycastHit();
            if (Physics.Raycast(vec, Vector3.down, out hit)) {
                diffs += hit.distance;
            }
        }
        if ((diffs /= 4.0f)*1000.0f > ThresholdTerrainColision)
            return true;
        return false;
    }
	
	public void Spawn(BaseUnit baseUnit) {
		LastBaseUnitOrder = baseUnit;
		gameObject.SetActive(true);
	}
	
	private RaycastHit MousePosScreen2World() {
        var _newraycast = Camera.main.ScreenPointToRay(Input.mousePosition);

	    if (!_newraycast.Equals(raycast))
	    {
	        raycast = _newraycast;
            hit = new RaycastHit();

	        if (Physics.Raycast(raycast, out hit, Mathf.Infinity))
	        {
	            var _attributes = hit.transform.gameObject.GetComponent<AttributesComponent>();
	            var _geyser = hit.transform.gameObject.GetComponent<SteamComponent>();
	            if (AllowGeyser == false)
	            {
	                if (_attributes != null &&
	                    (_attributes.UnitType == ObjectType.STRUCTURE || _attributes.UnitType == ObjectType.CHARACTER ||
	                     _attributes.UnitType == ObjectType.RESOURCE))
	                {
	                    _blockedByRay = true;
	                }
	                else
	                {
	                    _blockedByRay = false;
	                }
	            }
	            else
	            {
	                if (_attributes != null &&
	                    (_attributes.UnitType == ObjectType.STRUCTURE || _attributes.UnitType == ObjectType.CHARACTER))
	                {
	                    _connectedToGeyser = false;
	                    _blockedByRay = true;
	                    _geyserID = -1;
	                }
	                else if (_attributes != null &&
	                         (_attributes.UnitType == ObjectType.RESOURCE && _attributes.UnitName == "Geyser") &&
	                         (_geyser != null && _geyser.Occupied == false))
	                {
	                    _geyserPosition = hit.transform.position;
	                    _geyserID = _geyser.baseUnit.UniqueID;
	                    _connectedToGeyser = true;
	                    _blockedByRay = false;
	                }
	                else
	                {
	                    _connectedToGeyser = false;
	                    _blockedByRay = false;
	                    _geyserID = -1;
	                }
	            }
	            return hit;
	        }
	    }
        return hit;
    }

	private Vector3 GetMouseWorldPos()  {
        Vector3 hit = MousePosScreen2World().point;
	    if (AllowGeyser && _connectedToGeyser == true)
	        return _geyserPosition;

        return new Vector3(hit.x,hit.y,hit.z);
    }

    void OnCollisionStay(Collision col) {
        var attributes = col.gameObject.GetComponent<AttributesComponent>();
        if (attributes != null) {
            if (_connectedToGeyser == false)
            {
                if (attributes.UnitType == ObjectType.STRUCTURE || attributes.UnitType == ObjectType.CHARACTER || attributes.UnitType == ObjectType.RESOURCE)
                    _blockedByMesh = true;
            }
            else
            {
                _blockedByMesh = false;
                if (attributes.UnitType == ObjectType.STRUCTURE || attributes.UnitType == ObjectType.CHARACTER)
                    _blockedByMesh = true;
            }
            
        }
    }

     void OnCollisionExit (Collision col) {
         _blockedByMesh = false;
    }
}
