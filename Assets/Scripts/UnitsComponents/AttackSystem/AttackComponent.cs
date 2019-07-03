using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackComponent : AbstractUnitComponent {
	//Public Attributes
	public Ability AutoAttackAbility;
	public float AngleLookAtThreshold = 15;
	public float RotationSpeed = 8;
    public bool CanAttackFlyingUnities;
	
    private float _currentReloadTime = 3.0f;
	private float _currentAttackRange = 3.0f;
	private Ability _selectedAbility;
	private Transform _projectilesContainers;
	private UnitAnimator _unitAnimator;
	//private bool canAttack = true;
	
    //Informação do local de ataque ao inimigo
    public Vector3 AttackPoint = Vector3.zero;
    public BaseUnit EnemyBeingAttacked = null;
	
	#region Properties
	public float AttackRange { get { return _currentAttackRange; } set { _currentAttackRange = value; } }
	public float ReloadTime { get { return _currentReloadTime; } set { _currentReloadTime = value; } }
	public Ability SelectedAbility { set { _selectedAbility = value; } }
	#endregion
	
	void Awake () {
		_projectilesContainers = GameObject.Find("PROJECTILES").transform;
		if (_projectilesContainers == null) {
			Debug.Log("ERRO: Container PROJECTILES não encontrado, desativando...");
			gameObject.SetActive(false);
		}
	}
	
	void Start () {
		_unitAnimator = baseUnit.GetUnitComponent<UnitAnimator>();
	    if (AutoAttackAbility != null)
	    {
            AdjustRangeAndCooldown(AutoAttackAbility);
	    }
	}
	
	void Update () {
		//Se o jogador deselecionou a unidade, reseta a abilidade selecionada
		if (_selectedAbility != null && baseUnit.IsSelected == false && !baseUnit.IsEnemy)
			_selectedAbility = null;

        if (baseUnit.IsSelected && !baseUnit.IsEnemy && AutoAttackAbility.inCooldown == false)
        {
            BaseUnit _unitTarget = PlayerManager.Player.clickController.CheckClickOnEnemy();
            if (_unitTarget != null)
            {
                MailMan.Post.NewMail(new Mail("Attack", baseUnit.UniqueID, _unitTarget.UniqueID));
            }
        }
	}

 	public void AdjustRangeAndCooldown(Ability ability) {
		//Ajusta o range e o reloadTime da unidade para a abilidade selecionada
		if (ability != null) {
			_currentAttackRange = ability.attackRange;
			_currentReloadTime = ability.reloadTime;
		}
	}
		
	public bool IsInRangeOfAbility (BaseUnit target) {
	    if (target == null) return false;
	    return IsInRangeOfAbility(target.transform.position);
	}

    public bool IsInRangeOfAbility(Vector3 target) {
        var distance = Vector3.Distance(baseUnit.transform.position, target);
        var isInRange = distance <= _currentAttackRange;
        return isInRange;
    }

    public bool IsInFrontOfMe(BaseUnit target) {
        if (target == null) return false;

        RaycastHit hit;
        var direction = (target.transform.position - baseUnit.transform.position).normalized;
        var ray = Physics.Raycast(baseUnit.transform.position, direction, out hit, AttackRange);

        var res = ray && hit.transform && hit.transform.gameObject == target.gameObject;

        return res;
    }
	
	public void PrepareAttack (BaseUnit unitTarget) {
	    if (unitTarget == null)
	        return;

		if (unitTarget.IsDead == false) {
			//Seleciona a abilidade de acordo com a selectedAbility
			Ability _abilityToUse = _selectedAbility;
		    if (_abilityToUse == null)
		    {
                AdjustRangeAndCooldown(_abilityToUse);
		        _abilityToUse = AutoAttackAbility;
		    }

		    if (_abilityToUse != null) {
				if (_abilityToUse.inCooldown == false) {
					AdjustRangeAndCooldown(_abilityToUse);
					StartCoroutine(LookAtTarget(_abilityToUse, unitTarget));
				}
			}
		}
	}
	
	public IEnumerator DoAttack (Ability ability, BaseUnit unitTarget) {
		//Se ataque definido pela animação, aguarda até o unitAnimator dar o ok
        if (_unitAnimator != null && _unitAnimator.ImmediateAttack)
        {
			while (!_unitAnimator.DoAttackAction) {
				Debug.Log("Aguardando ordem do Anim");

				yield return null;
			}

            //Reseta o flag de computar ataque do unitAnimator
            _unitAnimator.DoAttackAction = false;
		}
		
		//Envia o mail para descontar o ether do resources
		GameManager.Manager.Resources.DebitResource(ResourceType.Ether, ability.EtherCost);
		
		//Verifica se a ability possui um nugget de heal, para permitir selecionar como target, um aliado
		bool _isHealingAbility = ability.IsHealingAbility;
		
		// Se a abilidade não for do tipo Heal, verifica se o alvo não é um recurso e é inimigo
		// Caso a abilidade for do tipo Heal, permite a seleção de um aliado
		if (unitTarget != null && (!_isHealingAbility && !unitTarget.IsResource && baseUnit.CheckIfIsMyEnemy(unitTarget)) ||
			(_isHealingAbility && unitTarget != null && !unitTarget.IsResource && !baseUnit.CheckIfIsMyEnemy(unitTarget))) {
			//Verifica se a abilidade possui um projétil, caso possua, dispara, se não, ataca direto
			if (ability.Projectile != null)
			{
			    AutoAttackAbility.StartCooldown();
				StartShootProjectileCommand(unitTarget, ability);
			} else {
                AutoAttackAbility.StartCooldown();
				StartAttackCommand(unitTarget, ability);
			}
		}
		
		//Após executar o ataque, reseta as propriedades
		_selectedAbility = null;
		//canAttack = true;
		StopAllCoroutines();
	}
	
	private void StartShootProjectileCommand (BaseUnit unitTarget, Ability ability) {
		//Se deve ser usado o prediction:
		if (ability.usePrediction) {
			//Rotaciona a unidade em direção do tiro
			Vector3 _direction = this.CalculateProjectileDirection(unitTarget, ability);
			
			Quaternion newRotation = Quaternion.LookRotation(_direction);
			
			baseUnit.transform.rotation = newRotation;
		}
		
		baseUnit.transform.LookAt(unitTarget.transform.position);
	
		//Instância o projétil
        Transform newProjectile = Instantiate(ability.Projectile, baseUnit.Shooter.transform.position, baseUnit.transform.rotation) as Transform;
		newProjectile.parent = _projectilesContainers;
		Projectile _projectile = newProjectile.GetComponent<Projectile>();
		_projectile.AbilityToUse = ability;
		_projectile.OwnerAttacker = this;
		
		//Inicio o movimento do projétil
		_projectile.StartMovement = true;
	}
	
	private Vector3 CalculateProjectileDirection(BaseUnit target, Ability ability)
    {
    	Vector3 _targetPosition = target.DamageTarget.position;
    	Vector3 _inititalProjectilePosition = baseUnit.transform.position;
    	Vector3 _targetVelocity = target.transform.GetComponent<Rigidbody>().velocity;
    	var _projectileSpeed = ability.Projectile.GetComponent<Projectile>().ProjectileSpeed;
    
        // make sure it's all in the horizontal plane:
       // _targetPosition.y = _inititalProjectilePosition.y = _targetVelocity.y = 0.0f;

        // create a normalized vector that is perpendicular to the vector pointing from the muzzle to the target's current position (a localized x-axis):
        Vector3 perpendicularVector = Vector3.Cross(_targetPosition - _inititalProjectilePosition, -Vector3.up).normalized;

        // project the target's velocity vector onto that localized x-axis:
        Vector3 projectedTargetVelocity = Vector3.Project(_targetVelocity, perpendicularVector);

        // calculate the angle that the projectile velocity should make with the localized x-axis using the consine:
        float angle = Mathf.Acos(projectedTargetVelocity.magnitude / _projectileSpeed) / Mathf.PI * 180;

        if (Vector3.Angle(perpendicularVector, _targetVelocity) > 90.0f)
        {
            angle = 180.0f - angle;
        }

        // rotate the x-axis so that is points in the desired velocity direction of the projectile:
        Vector3 returnValue = Quaternion.AngleAxis(angle, -Vector3.up) * perpendicularVector;

        // give the projectile the correct speed:
        returnValue *= _projectileSpeed;

        return returnValue;
    }
	
	public void StartAttackCommand (BaseUnit unitTarget, Ability ability) {
		unitTarget.GetUnitComponent<ArmorComponent>().SetDamage(baseUnit, ability);
	}
	
	public void StartAttackCommand (BaseUnit unitTarget, Ability ability, Vector3 direction) {
		unitTarget.GetUnitComponent<ArmorComponent>().SetDamage(baseUnit, ability, direction);
	}
	
	public override void GUIPriority() { }
	public override void UserInputPriority () {
        
	}
	
	private IEnumerator LookAtTarget(Ability ability, BaseUnit target) {
	    if (target != null && target.GetUnitComponent<AttributesComponent>().UnitType != ObjectType.STRUCTURE)
	    {
	        while (CheckTargetIsInVision(target) == false)
	        {
	            var _lookPos = (target.transform.position - baseUnit.transform.position).normalized;
	            _lookPos = new Vector3(_lookPos.x, 0, _lookPos.z);
	            var _rotation = Quaternion.LookRotation(_lookPos);
	            baseUnit.transform.rotation = Quaternion.Slerp(baseUnit.transform.rotation, _rotation,
	                RotationSpeed*Time.deltaTime);

	            yield return null;
	        }
	    }

	    StartCoroutine(DoAttack(ability, target));
	}
	
	private bool CheckTargetIsInVision(BaseUnit target) {
		var _targetDir = (target.transform.position - baseUnit.transform.position);
		var _angle = Vector3.Angle(baseUnit.transform.forward, _targetDir);

		return (_angle <= AngleLookAtThreshold);
	}
	
	public override void Reset() {}
}
