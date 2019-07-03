using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileComponent : AbstractUnitComponent {
	//COMPONENTES
    //public bool IsMoving;
    public bool SmoothStop = false;
    public float RotateSpeed = 5;
    public Movement CurrentMovement = null;

    private NavMeshAgent navMesh;
    private float currentRotateSpeed;

    #region PROPERTIES
    public NavMeshAgent NavMesh { get { return navMesh; } }
    public bool IsMoving
    {
        get { return navMesh.hasPath; }
        set
        {
            if (value == false)
            {
                navMesh.Stop(true);
                navMesh.ResetPath();
                navMesh.Resume();
            }
        }
    }
    public Vector3 Velocity { get { return navMesh.velocity; } }
    public float Speed { get { return navMesh.speed; } }
    #endregion

    void Start () {
		if (baseUnit == null) enabled = false;
			baseUnit.UnitType = ObjectType.CHARACTER;
	    navMesh = baseUnit.GetComponent<NavMeshAgent>();
	    baseUnit.ThreatLevel = 2;
        currentRotateSpeed = RotateSpeed;
        navMesh.angularSpeed = currentRotateSpeed;
    }

	void Update () {
		if (!baseUnit.IsEnemy && PlayerManager.Player.PlayerRoles.Contains(baseUnit.UnitRole) && baseUnit.IsSelected)
			CheckUserInput();

	    if (RotateSpeed != currentRotateSpeed)
	    {
	        currentRotateSpeed = RotateSpeed;
	        navMesh.angularSpeed = currentRotateSpeed;
	    }

        //Verifica se alcançou o destino 
        if (CurrentMovement != null && navMesh.hasPath && navMesh.remainingDistance <= navMesh.stoppingDistance)
	    {
	        CurrentMovement.movementComplete = true;
	    }
	}

    private void Move(Vector3 destination)
    {
        if (navMesh.hasPath && SmoothStop == false)
        {
            navMesh.Stop(true);
            navMesh.ResetPath();
            navMesh.Resume();
        }
        else if (navMesh.hasPath)
        {
            navMesh.ResetPath();
        }
        ConsumeMovement();
        navMesh.SetDestination(destination);
        CurrentMovement = new Movement();
    }

    public Movement MoveTo(Vector3 destination)
    {
        navMesh.stoppingDistance = 0;
        Move(destination);
        return CurrentMovement;
    }

    public Movement MoveTo(BaseUnit targetUnit)
    {
        navMesh.stoppingDistance = 3;
        Move(targetUnit.transform.position);
        return CurrentMovement;
    }

    public Movement MoveTo(Vector3 destination, float stopRange)
    {
        if (CurrentDistance(destination) <= stopRange)
        {
            CurrentMovement.movementComplete = true;
            return CurrentMovement;
        }

        navMesh.stoppingDistance = stopRange;
        Move(destination);
        return CurrentMovement;
    }

    public Movement MoveTo(BaseUnit targetUnit, float stopRange)
    {
        if (CurrentDistance(targetUnit.transform.position) <= stopRange)
        {
            CurrentMovement.movementComplete = true;
            return CurrentMovement;
        }

        navMesh.stoppingDistance = stopRange;
        Move(targetUnit.transform.position);
        return CurrentMovement;
    }

    public void ConsumeMovement()
    {
        if (CurrentMovement != null)
        {
            CurrentMovement = null;
            navMesh.ResetPath();
        }
    }
	
	void CheckUserInput()
	{
	    var _desiredPosition = PlayerManager.Player.clickController.GetClickPosition(MouseButton.Right);
        if (_desiredPosition != Vector3.zero)
            MoveTo(_desiredPosition);
	}

    public override void GUIPriority() { }

    public override void UserInputPriority() { }

    public override void Reset() {}

    public float CurrentDistance(Vector3 target)
    {
        var _position = (target - baseUnit.transform.position).normalized;
        return Vector3.Distance(_position, baseUnit.transform.position);
    }

    public float CurrentDistance(BaseUnit target)
    {
        if (target != null)
        {
            var _direction = (target.transform.position - baseUnit.transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(baseUnit.transform.position, _direction, out hit, Mathf.Infinity))
            {
                var _hitBaseUnit = hit.transform.GetComponent<BaseUnit>();
                if (_hitBaseUnit != null && _hitBaseUnit == target)
                {
                    return CurrentDistance(hit.point);
                }
            }
            else
            {
                return Vector3.Distance(target.transform.position, baseUnit.transform.position);
            }
        }

        return 9999;
    }
}

//Classe responsável para servir como marcador de movimento, podendo verificar se um movimento foi executado
public class Movement
{
    public bool movementComplete = false;

    public Movement()
    {
        movementComplete = false;
    }
}

