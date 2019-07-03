using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackComponent : AbstractUnitComponent {
	//Public Attributes
	public Ability AutoAttackAbility;
	public float AngleLookAtThreshold = 15;
	public float RotationSpeed = 8;
	
    private float currentReloadTime = 3.0f;
	private float currentAttackRange = 3.0f;
	private Ability selectedAbility;
	private Transform projectilesContainers;
	private UnitAnimator unitAnimator;
	private bool canAttack = true;
    private Movement currentMovement = null;

    #region NOVA IMPLEMENTACAO
    private MobileComponent mobileComponent;
    private bool gettingRange = false;
    #endregion

	
    //Informação do local de ataque ao inimigo
    public Vector3 AttackPoint = Vector3.zero;
    public BaseUnit EnemyBeingAttacked = null;
	
	#region Properties
	public float AttackRange { get { return currentAttackRange; } set { currentAttackRange = value; } }
	public float ReloadTime { get { return currentReloadTime; } set { currentReloadTime = value; } }
	public Ability SelectedAbility { set { selectedAbility = value; } }
	#endregion
	
	void Awake () {
		projectilesContainers = GameObject.Find("PROJECTILES").transform;
		if (projectilesContainers == null) {
			Debug.Log("ERRO: Container PROJECTILES não encontrado, desativando...");
			gameObject.SetActive(false);
		}
	}
	
	void Start () {
		unitAnimator = baseUnit.GetUnitComponent<UnitAnimator>();
	    mobileComponent = baseUnit.GetUnitComponent<MobileComponent>();
	}
	
	void Update () {
		if (selectedAbility != null && baseUnit.IsSelected == false && !baseUnit.IsEnemy)
			selectedAbility = null;
	}
	
	public void Attack (BaseUnit unitTarget) {
		if (unitTarget != null && unitTarget.IsDead == false) {
			//Seleciona a abilidade de acordo com a selectedAbility
			Ability _abilityToUse = selectedAbility;

		    if (_abilityToUse == null)
		    {
		        _abilityToUse = AutoAttackAbility;
		    }

		    if (_abilityToUse != null && _abilityToUse.inCooldown == false) {
				_abilityToUse.StartCooldown();
                StartAttack(_abilityToUse, unitTarget);
			}
		}
	}

    private void StartAttack(Ability ability, BaseUnit unitTarget)
    {
        //Se o atacante possui MobileComponent e o alvo não está no range, move até o alvo em busca do range
        if (mobileComponent != null && CurrentDistance(unitTarget) > ability.attackRange)
        {
            StartCoroutine(MoveToGetRange(ability, unitTarget));
        }
        else if (mobileComponent == null && CurrentDistance(unitTarget) <= ability.attackRange)
        {
            StartCoroutine(ExecuteAttack(ability, unitTarget));
        }
    }

    private IEnumerator ExecuteAttack(Ability ability, BaseUnit unitTarget)
    {
        //Aguarda o disparo do ataque pelo UnitAnimator
        if (unitAnimator.ImmediateAttack)
        {
            while (!unitAnimator.DoAttackAction)
                yield return null;
        }

        //Reseta o flag de computar ataque do unitAnimator
        unitAnimator.DoAttackAction = false;

        //Envia o mail para descontar o ether do resources
        GameManager.Manager.Resources.DebitResource(ResourceType.Ether, ability.EtherCost);

        //Verifica se a ability possui um nugget de heal, para permitir selecionar como target, um aliado
        bool _isHealingAbility = ability.IsHealingAbility;

        // Se a abilidade não for do tipo Heal, verifica se o alvo não é um recurso e é inimigo
        // Caso a abilidade for do tipo Heal, permite a seleção de um aliado
        if (unitTarget != null &&
            (!_isHealingAbility && !unitTarget.IsResource && baseUnit.CheckIfIsMyEnemy(unitTarget)) ||
            (_isHealingAbility && unitTarget != null && !unitTarget.IsResource &&
             !baseUnit.CheckIfIsMyEnemy(unitTarget)))
        {
            //Verifica se a abilidade possui um projétil, caso possua, dispara, se não, ataca direto
            if (ability.Projectile != null)
            {
                StartShootProjectileCommand(unitTarget, ability);
            }
            else
            {
                StartAttackCommand(unitTarget, ability);
            }
        }
    }

    private IEnumerator MoveToGetRange(Ability ability, BaseUnit unitTarget)
    {
        //Se não estiver no range adequado
        if (CurrentDistance(unitTarget) > ability.attackRange)
        {
            //Dispara o comando de mover e aguarda alcançar o range da habilidade
            currentMovement = mobileComponent.MoveTo(unitTarget, ability.attackRange);

            if (currentMovement != null)
            {
                //Aguarda alcançar o range
                while (currentMovement.movementComplete == false)
                {
                    //Se o movimento foi cancelado
                    if (currentMovement == null || currentMovement != mobileComponent.CurrentMovement)
                    {
                        Debug.Log("Quebrando");
                        currentMovement = null;
                        yield break;
                    }

                    yield return null;
                }
            }
        }

        mobileComponent.ConsumeMovement();
        currentMovement = null;
        //Ao atingir o range, ataca
        StartCoroutine(ExecuteAttack(ability, unitTarget));
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
		Transform newProjectile = Instantiate(ability.Projectile, baseUnit.transform.position, baseUnit.transform.rotation) as Transform;
		newProjectile.parent = projectilesContainers;
		Projectile _projectile = newProjectile.GetComponent<Projectile>();
		_projectile.AbilityToUse = ability;
		_projectile.OwnerAttacker = this;
		
		//Inicio o movimento do projétil
		_projectile.StartMovement = true;
	}
	
	private Vector3 CalculateProjectileDirection(BaseUnit target, Ability ability)
    {
    	Vector3 _targetPosition = target.transform.position;
    	Vector3 _inititalProjectilePosition = baseUnit.transform.position;
	    Vector3 _targetVelocity = target.transform.GetComponent<NavMeshAgent>().velocity;
    	var _projectileSpeed = ability.Projectile.GetComponent<Projectile>().ProjectileSpeed;
    
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

        //Após executar o ataque, reseta as propriedades
        selectedAbility = null;
        canAttack = true;
        StopAllCoroutines();
	}
	
	public void StartAttackCommand (BaseUnit unitTarget, Ability ability, Vector3 direction) {
		unitTarget.GetUnitComponent<ArmorComponent>().SetDamage(baseUnit, ability, direction);

        //Após executar o ataque, reseta as propriedades
        selectedAbility = null;
        canAttack = true;
        StopAllCoroutines();
	}
	
	public override void GUIPriority() { }

	public override void UserInputPriority () { 
		if (baseUnit.IsSelected && !baseUnit.IsEnemy){
			BaseUnit _unitTarget = PlayerManager.Player.clickController.CheckClickOnEnemy();
			if (_unitTarget != null) {
				Attack(_unitTarget);
			}
		}
	}
	
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
