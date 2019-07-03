using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityLibraryComponent : AbstractUnitComponent {
	public List<Ability> UnitAbilities;					//Lista de habilidades da unidade
	public Ability SelectedAbility;						//Habilidade selecionada pelo usuário
	private AttackComponent ownerAttackComponent;		//Componente de ataque da unidade para ser usada caso a habilidade seja um ataque
	private BaseUnit clickedUnit;						//Armazena a unidade selecionada como target da habildiade

	public void Start () {
		ownerAttackComponent = baseUnit.GetUnitComponent<AttackComponent>();

	    if (UnitAbilities.Count > 0)
	    {
	        for (int i = 0; i < UnitAbilities.Count; i++)
	        {
	            UnitAbilities[i].AbilityIndex = i;
	        }
	    }

        //Ativa o fly ability do crow
	    if (baseUnit.UnitClass == Unit.Crow)
	    {
	        baseUnit.GetUnitComponent<AttributesComponent>().IsFlying = false;
            SelectAbility(1);
	    }
	}
	
	public void Update () {
		//Se a unidade for deselecionada, reseta a habilidade selecionada e a unidade alvo
		if (!baseUnit.IsSelected && SelectedAbility != null) {
			SelectedAbility = null;
			clickedUnit = null;
		} else if (SelectedAbility != null && SelectedAbility.inCooldown == false) {
            if ((SelectedAbility.needTarget && clickedUnit != null) || SelectedAbility.needTarget == false) {
				PrepareToUseAbility();
				SelectedAbility = null;
			}
		}
	}

    public Ability GetAbilityInLibrary(string name)
    {
        if (UnitAbilities.Count > 0)
        {
            foreach (Ability _unitAbility in UnitAbilities)
            {
                if (_unitAbility.abilityName == name)
                {
                    return _unitAbility;
                }
            }
        }
        return null;
    }

	public void SelectAbility (int abilityIndex) {
		if (UnitAbilities.Count > 0 && abilityIndex < UnitAbilities.Count) {
            SelectedAbility = UnitAbilities[abilityIndex];
			if (SelectedAbility.isAttackAbility){
				var _attackComponent = baseUnit.GetUnitComponent<AttackComponent>();
				if (_attackComponent != null) {
					_attackComponent.AdjustRangeAndCooldown(SelectedAbility);
				    _attackComponent.SelectedAbility = SelectedAbility;
				}
			}
		} else 
			SelectedAbility = null;
	}
	
	public void SelectAbility (Ability ability) {
		if (UnitAbilities.Count > 0 && UnitAbilities.Contains(ability)) {
			SelectAbility(UnitAbilities.IndexOf(ability));
		}
	}
	
	public void SelectAbility (KeyCode hotKey) {
		if (UnitAbilities.Count > 0) {
			foreach (Ability _ability in UnitAbilities) {
				if (_ability.HotKey == hotKey) {
					SelectAbility(_ability);
					return;
				}
			}
		}
	}
	
	public bool SelectAbilityWithTarget (Ability ability, BaseUnit target) {
		SelectAbility(ability);
		if (SelectedAbility == null) return false;
		clickedUnit = target;
		if (clickedUnit == null) return false;
		return true;
	}

    public T GetAbility<T>() {
        if (UnitAbilities != null && UnitAbilities.Count > 0) {
            foreach (Ability ability in UnitAbilities) {
                if (ability.GetType() == typeof(T)) return (T)Convert.ChangeType(ability, typeof(T));
            }
        }

        return default(T);
    }
	
	//Método que fará o envio do MAIL do uso de abilidade
	public void PrepareToUseAbility () {
        Debug.Log("Preparando");
		if (SelectedAbility != null && SelectedAbility.inCooldown == false) {
			int _targetID = clickedUnit != null ? clickedUnit.UniqueID : -1;
	        //Se a habilidade não for uma habilidade de ataque, envia o MAIL do comando de usar a habilidade
			if (SelectedAbility.isAttackAbility == false) {
				//Envia o mail do uso de habilidade
                Debug.Log("Enviando mail");
                MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, (int)ResourceType.Ether, -(int)SelectedAbility.EtherCost));
	        	MailMan.Post.NewMail(new Mail("UseAbility", baseUnit.UniqueID, _targetID, UnitAbilities.IndexOf(SelectedAbility)));
			}
			//Caso contrario, se a habilidade for de ataque, envia a habilidade para ser usada pelo AttackComponent
			else if (SelectedAbility.isAttackAbility && ownerAttackComponent != null) {
				//Seta a abilidade selecionada do AttackComponent
				ownerAttackComponent.SelectedAbility = SelectedAbility;
                MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, (int)ResourceType.Ether, -(int)SelectedAbility.EtherCost));
				MailMan.Post.NewMail(new Mail("Attack", baseUnit.UniqueID, _targetID));
			}
		}
	}

    //Método chamado pelo mail
    public void UseSelectedAbility(BaseUnit targetUnit) {
		if (SelectedAbility != null && !SelectedAbility.inCooldown) {
			var _targetUnit = targetUnit ?? baseUnit;
			SelectedAbility.Use(baseUnit, _targetUnit);
		}

		SelectedAbility = null;
	}

	#region implemented abstract members of AbstractUnitComponent

    public override void GUIPriority() {
	}

	public override void UserInputPriority()
	{
	    var _structure = baseUnit.GetUnitComponent<StructureComponent>();
	    if (_structure != null && _structure.built)
	    {
	        //Verifica se a hotkey da habilidade foi ativada pelo jogador
	        if (UnitAbilities.Count > 0)
	        {
	            KeyCode _hotKeyPressed = PlayerManager.Player.clickController.GetKeyCodePressed();
	            if (_hotKeyPressed != KeyCode.None)
	            {
	                foreach (Ability _ability in UnitAbilities)
	                {
	                    if (_ability.HotKey == _hotKeyPressed)
	                    {
	                        InterfaceManager.Manager.SetSelectedAbility(UnitAbilities.IndexOf(_ability));
	                        break;
	                    }
	                }
	            }
	        }

	        //Captura a unidade clicada
	        if (SelectedAbility != null && SelectedAbility.needTarget)
	        {
	            BaseUnit _clickedUnit = null;
	            //Verifica se pode clicar em uma unidade aliada
	            if (SelectedAbility.allowTargetAlly)
	                _clickedUnit = PlayerManager.Player.clickController.CheckClickOnAlly();
	            else if (_clickedUnit == null)
	                _clickedUnit = PlayerManager.Player.clickController.CheckClickOnEnemy();

	            clickedUnit = _clickedUnit;
	        }
	    }
	}

    public override void Reset() {
    }

    #endregion
}
