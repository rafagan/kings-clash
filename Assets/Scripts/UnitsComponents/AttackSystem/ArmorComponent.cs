using Net;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmorComponent : AbstractUnitComponent {
	public List<DmgType> DamageTypeDefense;
	public List<float> DefAmount;
    public bool Buffed = false;

    [HideInInspector] public List<float> UnbuffedDefAmount;
	private Vector3 _currentDirection;
    private AttributesComponent _attributes;
    private GUIUnitComponent _gui;
    private IA_HFSM_MobileUnitManager _iaManager;

	void Start () {
		if (baseUnit == null) enabled = false;
        _attributes = baseUnit.GetUnitComponent<AttributesComponent>();
        _gui = baseUnit.GetUnitComponent<GUIUnitComponent>();
	    _iaManager = baseUnit.GetUnitComponent<IA_HFSM_MobileUnitManager>();

        UnbuffedDefAmount = new List<float>();
	    if (DefAmount != null && DefAmount.Count > 0)
	    {
            for (int i = 0; i < DefAmount.Count; i++)
            {
                UnbuffedDefAmount.Add(DefAmount[i]);
            }
	    }
	}
	
	void Update () {
		CheckLife();
	}
	
	public void SetDamage (BaseUnit attacker, Ability ability, Vector3 direction) {
		_currentDirection = direction;
		
		SetDamage(attacker, ability);
	}
	
	public void SetDamage (BaseUnit attacker, Ability ability) {
		var abilityNuggets = ability.Nuggets;
		if (abilityNuggets == null) return;
		
		//Verifica se a ability possui um nugget de heal
		bool isHealingAbility = ability.IsHealingAbility;
		
		//Caso seja uma unidade inimiga, dê o dano
		if (baseUnit.CheckIfIsMyEnemy(attacker)) {
			var totalDamage = 0.0f;
			foreach (Nugget nugget in abilityNuggets) {
				if (abilityNuggets != null) {
                    totalDamage += ProcessDefense(nugget);
				}
			}

            if (_gui != null) {
                _gui.addTextEffect(totalDamage * -1, Color.red, 2.0f);
            }
			_attributes.DamageUnit(totalDamage);
		} 
		//Caso seja uma unidade aliada, e a abilidade seja de cura, efetue a cura
		else if (!baseUnit.CheckIfIsMyEnemy(attacker) && isHealingAbility) {
			var totalHealing = 0.0f;
			
			foreach (Nugget nugget in abilityNuggets) {
				if (abilityNuggets != null) {
					totalHealing += nugget.damageValue;
				}
			}
			Debug.Log(totalHealing);
			_attributes.HealUnit(totalHealing);
		}
	}
	
	private float ProcessDefense (Nugget nugget) {
		if (nugget == null) return 0;			
			
		var _damage = nugget.damageValue;
		var _index = 0;
		
		if(nugget.damageType == DmgType.KNOCKBACK)
		{
			StartCoroutine(StartKnockbackEffect(_currentDirection));
			return 0;
		}
		
		if (DamageTypeDefense.Contains(nugget.damageType)) {
			_index = DamageTypeDefense.IndexOf(nugget.damageType);
			var _defense = DefAmount[_index] / 100;
			_damage = _defense	* nugget.damageValue;
			
			if (nugget.isDot) {
				var _tickTime = nugget.DotTotalTime / nugget.DotTicks;
				StartCoroutine(ProcessDot(_damage, nugget.DotTicks, _tickTime));
			}
			else
				return _damage;
		}
		
		return 0;
	}
	
	private IEnumerator ProcessDot(float dotDamage, float dotTicks, float tickTime) {
		int _tick = 1;
		do {
			_attributes.DamageUnit(dotDamage);
			yield return new WaitForSeconds(tickTime);
			_tick++;
		} while (_tick <= dotTicks);
	}
	
	private void CheckLife() {
	    if (!(_attributes.CurrentLife <= 0))
	        return;
        if(_iaManager != null && _iaManager.enabled)
            _iaManager.CurrentStateMachine.SendMessageToState(IA_Messages.DYING);
	    StopAllCoroutines();
	    _attributes.MaxLife = 0;
	    PlayerManager.Player.
	                  PlayerResources.SendMailAddResource(baseUnit, baseUnit.TeamID == 0 ? 1 : 0,
	                                                      ResourceType.Ether, (int) _attributes.Ether);

	    Graveyard.KillUnit(baseUnit);

	    if (baseUnit.UnitClass == Unit.Castle)
	        GameManager.Manager.SetGameOver(baseUnit.TeamID);
	}
	
	IEnumerator StartKnockbackEffect(Vector3 direction)
	{
		baseUnit.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().Stop(true);
		baseUnit.IsGrounded = false;
		baseUnit.GetComponent<Rigidbody>().isKinematic = false;	
		baseUnit.GetComponent<Rigidbody>().useGravity = true;
		baseUnit.GetComponent<Rigidbody>().AddForce(new Vector3(direction.x*100, 300, direction.z*100), ForceMode.Force);	
		
		yield return new WaitForSeconds(0.5f);
		StartCoroutine("FinishKnockbackEffect");
	}
	
	IEnumerator FinishKnockbackEffect() {
		while (baseUnit.IsGrounded == false)
			yield return null;
		
		if (baseUnit.GetComponent<Rigidbody>().isKinematic == false)
		{
		    baseUnit.GetComponent<Rigidbody>().useGravity = false;
			baseUnit.GetComponent<Rigidbody>().velocity = Vector3.zero;
			baseUnit.GetComponent<Rigidbody>().useGravity = false;	
		}
		baseUnit.GetComponent<Rigidbody>().isKinematic = true;	
		baseUnit.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().Resume();
		baseUnit.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();
		
	}
	
	 public override void GUIPriority() {
	 
	 }
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
