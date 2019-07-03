using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileComponent : AbstractUnitComponent {
	//COMPONENTES
	private UnityEngine.AI.NavMeshAgent _navMesh;
	public bool IsMoving;
    public Vector3 Destiny = Vector3.zero;
	public UnityEngine.AI.NavMeshAgent NavMesh { get { return _navMesh; } }


	void Start () {
		if (baseUnit == null) enabled = false;
			baseUnit.UnitType = ObjectType.CHARACTER;
	    _navMesh = baseUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
	    baseUnit.ThreatLevel = 2;
	}

	void Update () {
		if (!baseUnit.IsEnemy && PlayerManager.Player.PlayerRoles.Contains(baseUnit.UnitRole) && baseUnit.IsSelected)
		{
			CheckUserInput();
		}
	}

    public void MoveTo(Vector3 position) {
        _navMesh.SetDestination(position);
        Destiny = position;
        IsMoving = true;
    }
    public void MoveToDestiny() {
        if (Destiny == Vector3.zero)
            return;
        _navMesh.SetDestination(Destiny);
        IsMoving = true;
    }
    public void StopMoving() {
        _navMesh.ResetPath();
        Destiny = Vector3.zero;
        IsMoving = false;
    }
	

	void CheckUserInput() {
	    if (!baseUnit.IsSelected)
	        return;
	    var desiredPosition = PlayerManager.Player.clickController.GetClickPosition (MouseButton.Right);
	    if (desiredPosition != Vector3.zero && !IsAPatrolCommand()) {
	        MailMan.Post.NewMail(new Mail("Move", baseUnit.UniqueID, desiredPosition));
	    }
	}
	
	private bool IsAPatrolCommand() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	}
	
    public bool ReachedDestination() {
        var dist = _navMesh.remainingDistance;
        var reached = (!Mathf.Approximately(dist, Mathf.Infinity)) && _navMesh.remainingDistance <= Mathf.Epsilon &&
                  (_navMesh.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid || _navMesh.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete);
		return reached;
    }

    public override void GUIPriority() {}

    public override void UserInputPriority() {}

    public void MoveToAttackRange(BaseUnit target) {
        StartCoroutine(MoveToAttackRangeCoroutine(target.transform.position));
    }

    private IEnumerator MoveToAttackRangeCoroutine(Vector3 target) {
        var attack = baseUnit.GetUnitComponent<AttackComponent>();
        if (attack == null) yield break;

        var myNavMesh = baseUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
        myNavMesh.SetDestination(target);

        while (!ReachedDestination() || !attack.IsInRangeOfAbility(target)) {
            yield return new WaitForEndOfFrame();
        }
        myNavMesh.ResetPath();
    }

    public bool IsInRangeOfFOV(Vector3 target) {
        var distance = Vector3.Distance(baseUnit.transform.position, target);
        var isInRange = distance <= baseUnit.GetUnitComponent<AttributesComponent>().FieldOfView;
        return isInRange;
    }

    public bool IsInRangeOfFOV(BaseUnit target) {
        return IsInRangeOfFOV(target.transform.position);
    }


    
    public override void Reset() {}
}

