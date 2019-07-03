using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Ability : MonoBehaviour {
	public List<Nugget> Nuggets;
	public Transform Projectile;
    public Upgrade Prerequisite;
	[HideInInspector] public string abilityName;
    [HideInInspector] public string abilityDescription;
    public string AbilityIconName = "Star";
	public float abilityTime = 0.0f;
	public float attackRange = 1.5f;
	public float reloadTime = 0.0f;
	public int EtherCost = 100;
	public KeyCode HotKey = KeyCode.None;
    [HideInInspector] public bool abilityUnlocked = false;
    public bool needPoint = false;
	public bool isAttackAbility = false;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool inCooldown = false;
    [HideInInspector] public bool usingAbility = false;
	public bool usePrediction = false;
	public bool needTarget = true;
	public bool allowTargetAlly = false;
    [HideInInspector] public int AbilityIndex;
    [HideInInspector] public float currentTime = 0;

    public void Start()
    {
        if (Prerequisite == null)
        {
            abilityUnlocked = true;
        }
    }
	
	public bool IsHealingAbility { get { 
		foreach(Nugget _nugget in Nuggets) {
			if (_nugget.damageType == DmgType.HEAL) {
				return true;
			}
		}
		return false;
		} 
	}
  
  	public virtual void Use (BaseUnit owner, BaseUnit target) { }

	public virtual bool StartCooldown () {
		if (!inCooldown)
		{
            currentTime = reloadTime;
			StartCoroutine("StartCooldownTimer");
			return true;
		}
		return false;
	}
	
	
	public virtual bool StartAbility () {
		if (!usingAbility) {
			StartCoroutine("StartAbilityTimer");
			return true;
		}
		return false;
	}
	
	
	private IEnumerator StartAbilityTimer () {
		usingAbility = true;
		yield return new WaitForSeconds(abilityTime);
		usingAbility = false;
	}
	
	private IEnumerator StartCooldownTimer ()
	{
	    inCooldown = true;
	    StartCoroutine("StartClock");
        yield return new WaitForSeconds(reloadTime);
        inCooldown = false;
	}

    private IEnumerator StartClock()
    {
        while (currentTime >= 0)
        {
            currentTime -= Time.deltaTime;
            yield return null;
        }
        currentTime = reloadTime;
    }
}
