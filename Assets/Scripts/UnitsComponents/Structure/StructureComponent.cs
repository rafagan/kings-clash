using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructureComponent : AbstractUnitComponent {

    public int TickPercent = 10;					//Variável que define a porcentagem para desconto dos recursos do jogador (Ex. TickPercent = 10 -> A cada 10% do progresso de treinamento, será descontado 1 tick de recurso do jogador)
    //Private Attributes
	public bool built = false;					    //Flag que verifica se a estrutura foi construida
    public List<BaseUnit> BuildingUnits;				//Fila de unidades em assist
    private BaseUnit monarchAssist;
    private bool _canDebit = true;					//Flag que permite o débito da conta de recursos do jogador
    private int _steamTick = 0;						//Variável que debitará o Steam em ticks da conta do jogador
    private int _plasmoTick = 0;						//Variável que debitará o Plasmo em ticks da conta do jogador
    private int totalSteamDebitAmount = 0;			//Variável que armazena o débito de STEAM da estrutura em construcao
    private int totalPlasmoDebitAmount = 0;			//Variável que armazena o dévito de PLASMO da estrutura em construcao
    private Transform _mesh;
    private AttributesComponent attributes;

	#region PROPERTIES
	public BaseUnit BaseUnit { get { return baseUnit; } }
	#endregion

	void Start() {
		if (baseUnit == null) enabled = false;
		baseUnit.UnitType = ObjectType.STRUCTURE;
        PlayerManager.Player.CanSelect = true;


        BuildingUnits = new List<BaseUnit>();
        _mesh = transform.Find("Mesh");
        if (_mesh != null)
            _mesh.localScale = new Vector3(1.0f, 0.01f, 1.0f);
        
        attributes = baseUnit.GetComponent<AttributesComponent>();
        if (attributes != null)
            attributes.CurrentLife = 1.0f;
        baseUnit.ThreatLevel = 1;
	}


    void Update() {

        // (Testes) Faz a logica de construcao no mesh  
        if (_mesh != null)
            _mesh.localScale = new Vector3(1.0f, CalculateProgress() / 100.0f, 1.0f);

        if (CalculateProgress() == 100) {
            StopBuilding();
            built = true;
        }

        if (baseUnit.IsDead) {
            built = false;
        }

        if ((BuildingUnits.Count > 0 || monarchAssist != null) && CalculateProgress() < 100) {
            //Define os ticks de recurso a serem debitados
            _steamTick = CalculateResourceTick(attributes.SteamCost);
            _plasmoTick = CalculateResourceTick(attributes.PlasmoCost);

            //Caso o jogador tenha recurso suficiente
            if (ResourcesManager.Account.HasSufficientResource(ResourceType.Steam, _steamTick) &&
                ResourcesManager.Account.HasSufficientResource(ResourceType.Plasmo, _plasmoTick)) {

                //Se estiver no momento de debitar
                if (CheckDescountTime() && _canDebit)
                    StartCoroutine("DebitResources");

                // Adiciona vida conforme as conversoes de tempo e progresso
                var secondsFactor = 1.0f + CalculateBonusTime();
                var convertFactor = attributes.MaxLife / attributes.TrainingTime;
                attributes.CurrentLife += Time.deltaTime * (secondsFactor * convertFactor);
                if (attributes.CurrentLife > attributes.MaxLife)
                    attributes.CurrentLife = attributes.MaxLife;
            } else {
                // Se acabou os recursos para de construir
                StopBuilding();
            }

        }

    }

    private void StopBuilding() {
        BuildingUnits.Clear();
        monarchAssist = null;
    }

    public void AddBuildUnit(BaseUnit unit) {
        if(unit.UnitClass.Equals(Unit.Monarch)) {
            monarchAssist = unit;
        }
		else if (unit.UnitClass.Equals(Unit.Blacksmith) && !BuildingUnits.Contains(unit)) {
            BuildingUnits.Add(unit);
        }
    }

    public void RemoveBuildUnit(BaseUnit unit) {
        if (unit.UnitClass.Equals(Unit.Monarch)) {
            monarchAssist = null;
        }
        else if (unit.UnitClass.Equals(Unit.Blacksmith)) {
            BuildingUnits.Remove(unit);
        }
    }

    private IEnumerator DebitResources() {
        _canDebit = false;

        //Adiciona o tick ao custo total da unidade para que caso o jogador cancele o treinamento, retorna o valor para o jogador
        totalSteamDebitAmount += _steamTick;
        totalPlasmoDebitAmount += _plasmoTick;

        //Debita o tick da conta do jogador
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 0, -_steamTick));
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 1, -_plasmoTick));

        yield return new WaitForSeconds(0.9f);

        _canDebit = true;
    }


    private float CalculateBonusTime() {
        var bonusTime = 0.0f;

        if(BuildingUnits.Count > 1) {
            bonusTime += (BuildingUnits.Count-1) <= 5 ? (BuildingUnits.Count-1) * 10.0f : 50.0f;
        }

        if (monarchAssist != null)
            bonusTime += 20.0f;

        return bonusTime <= 50.0f ? (bonusTime / 100.0f) : (50.0f / 100.0f);
    }

    private int CalculateProgress() {
        var res = (int)((attributes.CurrentLife) * 100) / (int)(attributes.MaxLife);
        return res < 100 ? res : 100;
    }
    

    private int CalculateResourceTick(int resourceCost) {
        return (int)Mathf.Floor(resourceCost / TickPercent);
    }

    private bool CheckDescountTime() {
        return CalculateProgress() % TickPercent == 0;
    }

	 public override void GUIPriority() {

	}
	
	public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
