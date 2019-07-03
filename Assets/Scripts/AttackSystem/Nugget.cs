using UnityEngine;
using System.Collections;

public enum DmgType { SLASH, PIERCE, CAVALRY, KNOCKBACK, POISON, CRUSH, SIEGE, STRUCTURAL, HEAL }

public class Nugget : MonoBehaviour {
	public string nuggetName;
	public DmgType damageType;
	public float damageValue;
	
	public bool isDot = false;
	public float DotTotalTime = 0.0f;
	public int DotTicks = 0;
	
	public float KnockbackInnerForce = 0.0f;
	public float KnockbackInnerRadius = 0.0f;
	public float KnockbackOuterForce = 0.0f;
	public float KnockbackOuterRadius = 0.0f;
	
	public bool isAoe = false;
	public float AoeInnerDamagePercent = 0.0f;
	public float AoeInnerRadius = 0.0f;
	public float AoeOuterDamagePercent = 0.0f;
	public float AoeOuterRadius = 0.0f;
}
