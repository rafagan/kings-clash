using UnityEngine;

public class SteamComponent : AbstractPrimaryResource {
	//private Transform _particle;
	
	public bool Occupied {
		get { return occupied; }
		set {
			occupied = value;
			GetComponent<Collider>().enabled = !occupied;
		}
	}
	
// 	void Awake () {
// 		_particle = transform.SearchChild("Particle System");
// 		if (_particle == null) Debug.LogWarning("Steam Particle System not found");
// 	}

	void Start () {
		ResourceName = ResourceType.Steam;
		if (Mathf.Approximately(ResourcesLeft, 0f))
			ResourcesLeft = 1000f;
	}
}
