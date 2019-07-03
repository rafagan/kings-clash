using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graveyard : MonoBehaviour {
	public static Graveyard Instance;
	
	private List<BaseUnit> unitsToKill;
	private List<BaseUnit> unitsToReset;
	
	void Awake () {
		if (Instance != null)
			Destroy(Instance.gameObject);
		Instance = this;
	}

	void Start () {
		unitsToKill = new List<BaseUnit>();
		unitsToReset = new List<BaseUnit>();
	}
	
	void Update () {
		CheckKillList();
		ResetUnits();
	}
	
	public static void KillUnit(BaseUnit unit) {
		if(unit != null && !unit.IsDead) {
            GameManager.Manager.unitsManager.RemoveUnitInScene(unit);

			unit.CurrentDeathState = DeathState.InDeathAnim;
		    var _unitAnimator = unit.GetUnitComponent<UnitAnimator>();
            if (_unitAnimator != null)
                unit.GetUnitComponent<UnitAnimator>().SetAnimState(UnitAnimator.AnimStateType.Dead);

			//Desabilita o navmesh
			var _navMeshAgent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
			if (_navMeshAgent != null)
				_navMeshAgent.enabled = false;
			
			//Desabilita todos os AbstractUnitsComponents
			var _components = unit.GetAllUnitComponents;
			if (_components != null && _components.Count > 0){
				foreach (AbstractUnitComponent _component in _components) {
					if (_component as UnitAnimator != null) continue;
					_component.enabled = false;
				}
			}
			
			//Inicia a animação de morte e dispara o contador
			Graveyard.Instance.StartCoroutine(Graveyard.Instance.StartDeathAnimation(unit));
			
			//Envia para a lista de unidades a serem mortas
			Graveyard.Instance.unitsToKill.Add(unit);
		}
	}
	
	private void CheckKillList() {
		if (unitsToKill.Count <= 0) return;
		
		//Percorre a lista de unidades a serem mortas
		for (int i = unitsToKill.Count - 1; i >= 0; i--) {
			//Verifica se a unidade já completou a animação "morrendo"
			if (unitsToKill[i].CurrentDeathState == DeathState.DeathAnimFinished) {
				//Se completou, remove a unidade da KillList e inicia a co-rotina de "afundar o corpo"
				BaseUnit _unitToBury = unitsToKill[i];
				_unitToBury.CurrentDeathState = DeathState.InBurrowAnim;
				StartCoroutine(BuryUnit(_unitToBury));
				unitsToKill.RemoveAt(i);
			}
		}
	}
	
	private IEnumerator BuryUnit(BaseUnit unit) {
		//Ativa o contador de tempo de "enterrar" a unidade
		unit.StartBuryUnit();
	
		//Inicia o laço de animação de "enterrar" a unidade
		while (unit.CurrentDeathState != DeathState.BurrowAnimFinished) {
			unit.transform.Translate(Vector3.down * unit.BurySpeed * Time.deltaTime);
			
			yield return null;
		}
		
		//Desativa a baseunit
		unit.gameObject.SetActive(false);
		
		//Envia a unidade para a ResetUnits
		unitsToReset.Add(unit);
	}
	
	private void ResetUnits() {
		if (unitsToReset.Count <= 0) return;
		
		//Percorre a lista de unidades, resetando-as
		for (int i = unitsToReset.Count - 1; i >= 0; i--) {
			BaseUnit _unit = unitsToReset[i];
			//Reseta a baseUnit;
			_unit.Reset();
			
			//Remove da lista
			unitsToReset.RemoveAt(i);
			
			//Envia a mensagem de despawn
			MailMan.Post.NewMail(new Mail("Despawn", _unit.UniqueID, _unit.UniqueID, _unit.GetUnitComponent<PoolItemComponent>().MyPoolManager.PoolUniqueID));
		}
	}
	
	private IEnumerator StartDeathAnimation(BaseUnit unit) {
		//Inicia a animação de morte
	
		yield return new WaitForSeconds(unit.DeathAnimationDuration);
		
		//Troca o estado para animação de morte finalizada
		unit.CurrentDeathState = DeathState.DeathAnimFinished;
	}
}
