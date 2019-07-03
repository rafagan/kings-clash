using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitAnimator: AbstractUnitComponent {

	private Animator Anim {
		get { return baseUnit.Anim; }
	}

	public ParticleSystem[] Particles = new ParticleSystem[10];
	public AudioClip[] AudioClips = new AudioClip[10];
	//50% of idle0, 30% of idle1, 20% of idle2
	public int[] IdleWeights = new int[10] { 0, 0, 0, 0, 0, 1, 1, 1, 2, 2 };

	// Variáveis que podem ser acessadas externamente e definem o estado lógico do UnitAnimator
	public enum AttackStateType { PreAttack, Attack, Reload }
	public enum AnimStateType { Null, Idle, Walk, Attack, Carry, Dead, Absorb, Build, Heal, Repair }
	private AnimStateType _currentLogicState;
	public AnimStateType LogicState;
	public AttackStateType AttackState;
	//private AttackStateType _currentAttackState;
	public int ParticleState = 0;	// 0 = nothing, 1 = fire particle 1, 2 = fire particle 2, etc 
	public int AudioClipState = 0;
	public int WeaponCode = 0;		// eg.: 0 = hammer, 1 = pyron projector, 2 = pyron collector, etc
	public bool IsAttacking = false;

	// Os parâmetros do Animator são chamados por strings fixas, definidas aqui:
	// Os parâmetros são: attackState, animState, animIdx (índice da animação, quando há várias do tipo)
	private static string AnimStateTag = "animState";
	//private static string AttackStateTag = "attackState";
	private static string ChangeStateTag = "changeState";
	private static string animIdxTag = "animIdx";

	// Todas as variáveis de curvas do Animator começarão com "set", mas não precisam
	// ser reproduzidas aqui no script, só a string; ex: "setAttackState", "setParticle"
	public static string SetAttackStateTag = "setAttackState"; // 0 = nothing, 1 = preattack, 2 = attack, 3 = reload
	public static string SetParticleStateTag = "setParticleState";
	public bool ImmediateAttack;
	public bool DoAttackAction;		// Flag usada pelo AttackComponent
	public bool DebugAnimState = false;
	private int _currentIdleIdx = 0;
	//private bool idleAnimTimeout;	// Flag de verificação para mudança da animação idle
	static readonly int AttackStateHash = Animator.StringToHash("Base.Attack01");
	static readonly int NullStateHash = Animator.StringToHash("Base.Null");
	static readonly int IdleStateHash = Animator.StringToHash("Base.Idle_BlendTree");
	static readonly int WalkStateHash = Animator.StringToHash("Base.Walk");
	private int _currentWeaponIdBkp = 0;
	public int CurrentWeaponId = 0;
	public Transform[] WeaponHooks = new Transform[5]; // Max 5 weapons
	private AnimStateType _debugAnimatorState;

	public void Start() {
		_currentLogicState = LogicState = AnimStateType.Null;
		AttackState = 0;
		ParticleState = 0;
		AudioClipState = 0;
		//InvokeRepeating("IdleAnimCheck", 2f, 2f);
		SetWeaponVisibility();
	}

	public void Update() {
		CheckAnimationState();
		CheckParticleState();
		CheckSoundState();
	}

	/// <summary> Requires an AudioSource component </summary>
	private void CheckSoundState() {
		if (GetComponent<AudioSource>() == null || AudioClipState == 0) return;
		GetComponent<AudioSource>().PlayOneShot(AudioClips[ParticleState]);
	}

	void CheckParticleState() {
		if (ParticleState == 0) return;
		Particles[ParticleState].Play(true);
	}

	void CheckAnimationState() {
		if (Anim == null) return;
		IdleAnimCheck();
		// Só troca estados se algo mudou, para evitar repetição contínua de animações
		if (_currentLogicState == LogicState) return;
		DoAttackAction = Mathf.Approximately(Anim.GetFloat(SetAttackStateTag), 2f);

		_currentLogicState = LogicState;
		Anim.SetBool(ChangeStateTag, true);

		// Atualiza o Animator pra o estado atual. Transições e reprodução é trabalho do Animator.
		if (DebugAnimState) Debug.Log("Anim: " + Anim + " LogicState: " + LogicState);
		Anim.SetInteger(AnimStateTag, (int)LogicState); // Cast de enum pra int funciona! :)

		if (LogicState == AnimStateType.Attack)
			CheckAttackState();
	}

	public void IdleAnimCheck() {
		if (AnimatorState() != AnimStateType.Null || LogicState != AnimStateType.Idle) return;
		int idx = IdleWeights[Random.Range(0, 9)];
		if (DebugAnimState) Debug.Log("picked idle anim idx: " + idx);
		if (_currentIdleIdx == idx) return;		// won't re-start an animation currently playing
		Anim.SetFloat(animIdxTag, _currentIdleIdx);
		Anim.SetBool(ChangeStateTag, true);
		_currentIdleIdx = idx;
	}

	private void CheckAttackState() {
		Debug.Log("Got into checkattackstate");
		StartCoroutine(WaitForEndOfAnim(AttackStateHash, () => {LogicState = AnimStateType.Null;}));
		if (Mathf.FloorToInt(Anim.GetFloat(SetAttackStateTag)) == (int)AttackStateType.Attack) {
			// A arma (ability) é efetivamente disparada aqui.
			DoAttackAction = true;
		}
		// PS.: o Animator não precisa avisar que a animação de ataque acabou, o 'canFire' é definido 
		// pelo reload Time, que é contado a partir do início da animação de ataque mesmo.
	}

	void FixedUpdate() {
		IsAttacking = AnimatorState() == AnimStateType.Attack;

// 		if (LogicState == AnimStateType.Idle) {
// 			IsAttacking = false;
// 			NullState = true;
// 		}

		if (CurrentWeaponId == _currentWeaponIdBkp) return;
		SetWeaponVisibility();
		_currentWeaponIdBkp = CurrentWeaponId;
	}

	private void SetWeaponVisibility() {
		if (WeaponHooks == null) return;
		for (int i = 0; i < WeaponHooks.Length; i++) {
			if (WeaponHooks[i] == null) continue;
			WeaponHooks[i].localScale = (i == CurrentWeaponId) ? Vector3.one : Vector3.zero;
		}
	}

	public void SetAnimState(AnimStateType state) {
		Debug.Log("Change Anim to: " + state);
		if (Anim == null) return;
		LogicState = state;
		CheckAnimationState();
	}

	public void LateUpdate() {
		if (Anim == null) return;
		Anim.SetBool(ChangeStateTag, false);
	}

	public AnimStateType AnimatorState() {
		//public enum AnimStateType { Idle, Walk, Attack, Carry, Dead, Absorb, Build, Heal, Repair }
		var stateHash = Anim.GetCurrentAnimatorStateInfo(0).nameHash;
		if (stateHash == AttackStateHash) _debugAnimatorState = AnimStateType.Attack;
		if (stateHash == WalkStateHash) _debugAnimatorState = AnimStateType.Walk;
		if (stateHash == IdleStateHash) _debugAnimatorState = AnimStateType.Idle;
		//if (stateHash == AttackStateHash) return AnimStateType.Attack; -walk, etc, will go here (TODO)
		if (stateHash == NullStateHash) _debugAnimatorState = AnimStateType.Null;
		return (_debugAnimatorState);
	}

// 	public IEnumerator WaitForEndOfAnim() {
// 		
// 	}

	public IEnumerator WaitForEndOfAnim(int hash, /*string layerName,*/ System.Action callback) {
// 		int layer = 0;
// 		for (int i = 0; i < Anim.layerCount; i++)
// 			if (Anim.GetLayerName(i) == layerName)
// 				layer = i;

		while (Anim.GetCurrentAnimatorStateInfo(0).nameHash != hash) // 0=>layer (if not multi-layered)
			yield return 0;
		float currentTime = Mathf.Floor(Anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
		while (Anim.GetCurrentAnimatorStateInfo(0).nameHash == hash 
			&& Anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
			yield return 0;
		if (callback != null)
			callback();
	}

	// Caso o teste do AttackComponent não funcione, tente restaurar aqui para evitar múltiplos ataques </summary>
	// 	public void LateUpdate() {
	// 		DoAttackAction = false;
	// 	}

	#region implemented abstract members of AbstractUnitComponent

	public override void GUIPriority() { }
	public override void Reset() { }
	public override void UserInputPriority() { }

	#endregion
}
