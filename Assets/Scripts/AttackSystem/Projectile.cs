using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	public AttackComponent OwnerAttacker;
	public Ability AbilityToUse;
	public float ProjectileSpeed;
	public float MaxDistanceLife;
	public bool StartMovement = false;
	
	private Vector3 initialPosition;

	void Start () {
		initialPosition = transform.position;
	}
	
	void Update () {
		CheckLifeDistance();
		if (StartMovement)
			Move();
	}
	
	void OnTriggerEnter (Collider col) {
		BaseUnit _target = col.gameObject.GetComponent<BaseUnit>();
		if (_target != null && _target != OwnerAttacker.baseUnit && OwnerAttacker.baseUnit.CheckIfIsMyEnemy(_target)) {
            Debug.Log("Destruindo");
			var distance = _target.transform.position - transform.position;
			Vector3 _direction = distance / distance.magnitude;
			OwnerAttacker.StartAttackCommand(_target, AbilityToUse, _direction);
			//Envia o mail de destruir o projétil
			Destroy(this.gameObject);
		}
	}
	
	private void Move() {
		transform.Translate(Vector3.forward * ProjectileSpeed * Time.deltaTime, Space.Self);
	}
	
	private void CheckLifeDistance() {
		//Envia o mail de destruir o projétil
		if (Vector3.Distance(initialPosition, transform.position) >= MaxDistanceLife)
			Destroy(this.gameObject);
	}
}
