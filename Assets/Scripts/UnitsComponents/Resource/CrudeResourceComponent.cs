using UnityEngine;
using System.Collections;

public enum CrudeResourceType { CrudePlasmo, CrudeTree }

public class CrudeResourceComponent : AbstractUnitComponent {
	public CrudeResourceType ResourceType = CrudeResourceType.CrudePlasmo;
	
	public int TicksToGather = 5;
	public int NodesLeft = 10;
	public int RespawnTimeInSecs = 60;
	public float NodeSpawnRange = 2;
	public Pool NodePool;
	
	private int ticksLeft;
	private Vector3 nodeSpawnPosition;
	//CRUDE PLASMO PROPERTIES
	private float nodeSpawnAngle;
	//CRUDE TREE PROPERTIES
	private Transform normalTreeMesh;
	private Transform cutDownTreeMesh;
	
	public BaseUnit ResourceBaseUnit { get { return baseUnit; } }
				
	void Start () {
		if (baseUnit == null) enabled = false;
		if (NodePool == null) enabled = false;
		
		ticksLeft = TicksToGather;
		
		if (transform.GetComponent<Collider>().enabled == false)
			transform.GetComponent<Collider>().enabled = true;
			
		if (ResourceType == CrudeResourceType.CrudeTree) {
			NodesLeft = 1;
            nodeSpawnPosition = transform.Find("SpawnPoint").position;
            normalTreeMesh = transform.Find("Mesh01");
            normalTreeMesh.gameObject.SetActive(true);
            cutDownTreeMesh = transform.Find("Mesh02");
            cutDownTreeMesh.gameObject.SetActive(false);
		}
		
		transform.GetComponent<Collider>().enabled = true;
	}
	
	public void Gather () {
		ticksLeft--;
		if (ticksLeft <= 0 && NodesLeft > 0) {
			ticksLeft = TicksToGather;
			SpawnNode();
		}
	}
	
	private void SpawnNode () {
		NodesLeft--;
		Vector3 _spawnPosition = Vector3.zero;
		
		if (ResourceType == CrudeResourceType.CrudePlasmo)
			_spawnPosition = GetNodeSpawnPosition();
		else {
			_spawnPosition = nodeSpawnPosition;
			normalTreeMesh.gameObject.SetActive(false);
			cutDownTreeMesh.gameObject.SetActive(true);
			transform.GetComponent<Collider>().enabled = false;
		}
		MailMan.Post.NewMail(new Mail("Spawn", baseUnit.UniqueID, NodePool.PoolUniqueID, _spawnPosition, 99));

        if (NodesLeft <= 0) {
            if (ResourceType == CrudeResourceType.CrudePlasmo)
                Despawn();
            else {
                //				transform.collider.enabled = false;
                //				StartCoroutine("Respawn");
                Despawn();
            }
        }
	}
	
	private void Despawn () {
		MailMan.Post.NewMail(new Mail("Despawn", baseUnit.UniqueID, baseUnit.UniqueID, baseUnit.GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID));
	}
	
	private IEnumerator Respawn () {
		yield return new WaitForSeconds(RespawnTimeInSecs);
		
		MailMan.Post.NewMail(new Mail("Spawn", baseUnit.UniqueID, baseUnit.GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID, transform.position, 99));
		StopAllCoroutines();
		Despawn();	
	}
	
	private Vector3 GetNodeSpawnPosition () {
		nodeSpawnAngle = Random.Range(0, 360);
		Vector3 _nodeSpawnPoint = (transform.forward - transform.position).normalized * NodeSpawnRange;
		Vector3 _helper = Vector3.Cross(_nodeSpawnPoint, Vector3.right);
		
		_nodeSpawnPoint = transform.position - (Quaternion.AngleAxis(nodeSpawnAngle, _helper) * _nodeSpawnPoint);
		
		return _nodeSpawnPoint;
	}
	
	 public override void GUIPriority(){}
	 
	 public override void UserInputPriority() {
	
	}
	public override void Reset() {}
}
