using UnityEngine;
using System.Collections;

public class AbilityFlyLand : Ability
{
    public float LosPercentUp = 35;
	private bool _canMove = true;
    private float _normalLos;
    private float _oldRadius;
    private float _oldHeight;
    private AttributesComponent _ownerUnitAttributes;

	public void Awake () {
		abilityName = "Flies high";
	    abilityDescription = "Flies high, and increases by " + LosPercentUp + " the field of view while flying.";
	}

    new void Start() {
        base.Start();
    }
	
	public override void Use(BaseUnit owner, BaseUnit target) {
		if (owner != null)
		{
		    if (Mathf.Approximately(_normalLos, 0f)) {
                _ownerUnitAttributes = owner.GetUnitComponent<AttributesComponent>();
                _normalLos = _ownerUnitAttributes.FieldOfView;
		        _oldRadius = owner.GetUnitComponent<MobileComponent>().NavMesh.radius;
                _oldHeight = owner.GetUnitComponent<MobileComponent>().NavMesh.height;
		    }
			FlyOrLand(owner);
            StartCooldown();
		}
	}
	
	public void FlyOrLand(BaseUnit target) {
		var unitAttributes = target.GetUnitComponent<AttributesComponent>();
		var navMesh = target.GetUnitComponent<MobileComponent>().NavMesh;
		
		if(_canMove && unitAttributes.IsFlying == false) {
		    _ownerUnitAttributes.FieldOfView = _normalLos + (_normalLos*LosPercentUp/100);
			_canMove = false;
			StartCoroutine(GoFly(navMesh, unitAttributes));
		}
		else if(_canMove && unitAttributes.IsFlying) {
		    _ownerUnitAttributes.FieldOfView = _normalLos;
			_canMove = false;
			StartCoroutine(GoLand(navMesh, unitAttributes));
		}
	}

	IEnumerator GoFly(UnityEngine.AI.NavMeshAgent navMesh, AttributesComponent unitAttributes) {	
    	while (navMesh.baseOffset <= unitAttributes.FlightHeight) {
			navMesh.baseOffset+=0.1f;
				
			yield return null;
		}
		navMesh.baseOffset = unitAttributes.FlightHeight;
	    navMesh.radius = 0;
	    navMesh.height = 0;
		unitAttributes.IsFlying = true;
		_canMove = true;
	}


	IEnumerator GoLand(UnityEngine.AI.NavMeshAgent navMesh, AttributesComponent unitAttributes)
	{
		while (navMesh.baseOffset > 0) {
			navMesh.baseOffset -= 0.1f;
			yield return null;
		}
        navMesh.radius = _oldRadius;
	    navMesh.height = _oldHeight;
		unitAttributes.IsFlying = false;
		_canMove = true;
	}
}
