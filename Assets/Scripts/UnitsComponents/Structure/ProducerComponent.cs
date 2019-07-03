using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProducerComponent : AbstractUnitComponent {
	//Public Attributes
	public List<Pool> Tier1UnitsPool;
	public List<Pool> Tier2UnitsPool;
	public List<Pool> Tier3UnitsPool;
	public Pool InitialUnitToTrain;					//Pool para o treinamento da unidade default da estrutura
	public bool InsideTraining = true;				//Flag que define se a unidade é treinada dentro ou fora da estrutura
	public bool RepeatMode = true;					//Flag que ativa/desativa o RepeatTrainMode
	public int MaxTrainQueueSize = 5;				//Variável que define o tamanho máximo da fila de treinamento de unidades
	public int TickPercent = 10;					//Variável que define a porcentagem para desconto dos recursos do jogador (Ex. TickPercent = 10 -> A cada 10% do progresso de treinamento, será descontado 1 tick de recurso do jogador)
    public int TrainingVelocity = 2;
    public bool ListHasBeenChanged = false;

	//Private Attributes
	public List<Pool> unitTrainQueue;				//Fila de unidades em treinamento
	private bool training = false;					//Flag que verifica que alguma unidade está sendo treinada no momento
	private bool canDebit = true;					//Flag que permite o débito da conta de recursos do jogador
	private float trainingTime;						//Variável que armazena o tempo total de treinamento da unidade atual
	private float trainingProgress = 0;				//Variável que armazena o progresso do treinamento da unidade atual
	private int lastUnitTrained;					//Variável que armazena o ID do POOL da ultima unidade treinada, para o autorepeat
	private int steamTick = 0;						//Variável que debitará o Steam em ticks da conta do jogador
	private int plasmoTick = 0;						//Variável que debitará o Plasmo em ticks da conta do jogador
	private int totalSteamDebitAmount = 0;			//Variável que armazena o débito de STEAM da unidade em treinamento
	private int totalPlasmoDebitAmount = 0;			//Variável que armazena o dévito de PLASMO da unidade em treinamento
    private Transform rallyPointPrefab;				//Prefab do RallyPoint
    private Transform spawnPointPrefab;				//Prefab do SpawnPoint

    //Bounds do mapa para dar move corretamente nas unidades que estão bloqueando o caminho (Ráfagan)
    private Vector3 _right, _left;

	#region PROPERTIES
	public Transform RallyPointPrefab { get { return rallyPointPrefab; } set { rallyPointPrefab = value; } }
    public Vector3 RallyPointPosition {
        get { return rallyPointPrefab.transform.position; } 
        set { rallyPointPrefab.transform.position = value; }
    }
    public Vector3 SpawnPointPosition {
        get { return spawnPointPrefab.transform.position; }
        private set { spawnPointPrefab.transform.position = value; }
    }
    public bool ActiveRallyPoint {
        get { return rallyPointPrefab.gameObject.activeInHierarchy; } 
        set { rallyPointPrefab.gameObject.SetActive(value); }
    }
    public bool IsTraining { get { return training; } }
    public float CurrentTrainingProgress { get { return trainingProgress; } }
    public List<Pool> TrainingQueue { get { return unitTrainQueue; } } 
	#endregion
	
    void Awake() {
        _right = GameObject.Find("RightBound").transform.position;
        _left = GameObject.Find("LeftBound").transform.position;
    }

	public void Start () {
		//if (baseUnit == null) enabled = false;
		rallyPointPrefab = transform.Find("RallyPoint");
		spawnPointPrefab = transform.Find("SpawnPoint");
	    rallyPointPrefab.transform.position = spawnPointPrefab.transform.position;
		rallyPointPrefab.gameObject.SetActive(false);
        spawnPointPrefab.gameObject.SetActive(false);
		unitTrainQueue = new List<Pool>();
		lastUnitTrained = InitialUnitToTrain.PoolUniqueID;
	}
	
	void Update () {
        var _structure = baseUnit.GetUnitComponent<StructureComponent>();
	    if (_structure != null && _structure.built)
	    {
	        if (PlayerManager.Player.CheckPlayerRole(baseUnit.UnitRole))
	        {
	            if (!baseUnit.IsSelected)
	            {
	                rallyPointPrefab.gameObject.SetActive(false);
	            }
	            else
	            {
	                rallyPointPrefab.gameObject.SetActive(true);
	                var clickPosition = PlayerManager.Player.clickController.GetClickPosition(MouseButton.Right);

	                if (clickPosition != Vector3.zero && baseUnit.IsSelected)
	                {
	                    rallyPointPrefab.transform.position = clickPosition;
	                }
	            }
	        }

	        if (unitTrainQueue.Count > 0 && unitTrainQueue.Count < 5 && !training)
	        {
	            StartCoroutine("Training");
	            spawnPointPrefab.gameObject.SetActive(true);
	        }

	        if (RepeatMode && unitTrainQueue.Count < 5)
	            TrainUnit(lastUnitTrained);
	    }
	}

	 public override void GUIPriority() {
        
	 }

	public void TrainUnit(int poolID) {
		if (unitTrainQueue.Count < MaxTrainQueueSize)
		{
		    ListHasBeenChanged = true;
			lastUnitTrained = poolID;
			var _unitPool = PoolsManager.Manager.GetPoolByID (poolID);
			
			unitTrainQueue.Add(_unitPool);
		}
	}
	
	public void CancelTraining(int index) {
		if (index == 0) {
			StopCoroutine("Training");
			
			//Retorna o valor debitado
            MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 0, totalSteamDebitAmount));
            MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 1, totalPlasmoDebitAmount));

            ListHasBeenChanged = true;
			ResetVariables ();
		}
			
		unitTrainQueue.RemoveAt(index);
	}
	
	private IEnumerator Training() {
		training = true;
		
		//Armazena o AttributesComponent para leitura de dados como Tempo e Custo de produção
		var _trainingUnitAttibutes = unitTrainQueue[0].Prototype.GetComponent<AttributesComponent>();
		
		//Define o tempo de produção
		trainingTime = _trainingUnitAttibutes.TrainingTime;
		
		//Define os ticks de recurso a serem debitados
		steamTick = CalculateResourceTick(_trainingUnitAttibutes.SteamCost);
		plasmoTick = CalculateResourceTick(_trainingUnitAttibutes.PlasmoCost);
		
		//Executa o laço de treinamento
		while (trainingProgress <= trainingTime) {
			//Caso o jogador tenha recurso suficiente
			if(	ResourcesManager.Account.HasSufficientResource(ResourceType.Steam, steamTick) &&
				ResourcesManager.Account.HasSufficientResource(ResourceType.Plasmo, plasmoTick)) {
				
				//Se estiver no momento de debitar
				if (CheckDescountTime () && canDebit)		
					StartCoroutine("DebitResources");
				
				//Adiciona o tempo de progresso do treinamento
				trainingProgress += Time.deltaTime * TrainingVelocity;
			}
			yield return null;
		}
		
		//Reseta as variáveis
		ResetVariables ();

        AvoidUnities();
		
		//Spawna a unidade
		MailMan.Post.NewMail(new Mail("Spawn", baseUnit.UniqueID, unitTrainQueue[0].PoolUniqueID, SpawnPointPosition, RallyPointPosition, PlayerManager.Player.PlayerTeam));
		
        //Ativa o flag
        ListHasBeenChanged = true;

		//Remove a unidade da fila de treinamento
		unitTrainQueue.RemoveAt(0);
	}


    private void AvoidUnities() {
        var hitColliders = Physics.OverlapSphere(SpawnPointPosition, 3);

        foreach (Collider t in hitColliders) {
            var bu = t.GetComponent<BaseUnit>();
            if (bu == null) continue;
            var type = bu.UnitType;
            if (type != ObjectType.CHARACTER) continue;
            Debug.Log(bu.name);

            var mobile = bu.GetUnitComponent<MobileComponent>();
            var distRight = baseUnit.transform.position - _right;
            var distLeft = baseUnit.transform.position - _left;
            var direction = distLeft.sqrMagnitude > distRight.sqrMagnitude ? -1 : 1;
            Debug.Log(distLeft.sqrMagnitude);
            Debug.Log(distRight.sqrMagnitude);
            Debug.Log(direction);
            mobile.Destiny = bu.transform.position + new Vector3(10 * direction, 0);

            var fsm = bu.GetUnitComponent<IA_HFSM_MobileUnitManager>();
            fsm.CurrentStateMachine.SendMessageToState(IA_Messages.MOVING);
        }
    }
	
	private IEnumerator DebitResources () {
		canDebit = false;
		
		//Adiciona o tick ao custo total da unidade para que caso o jogador cancele o treinamento, retorna o valor para o jogador
		totalSteamDebitAmount += steamTick;
		totalPlasmoDebitAmount += plasmoTick;
		
		//Debita o tick da conta do jogador
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 0, -steamTick));
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 1, -plasmoTick));
					
		yield return new WaitForSeconds(0.9f);
		
		canDebit = true;
	}

	private void ResetVariables ()
	{
		training = false;
		trainingProgress = 0;
		totalSteamDebitAmount = 0;
		totalPlasmoDebitAmount = 0;
		steamTick = 0;
		plasmoTick = 0;
        spawnPointPrefab.gameObject.SetActive(false);
	}
	
	public int CalculateProgress() {
	    var res = (int) ((trainingProgress*100)/trainingTime);
		return res < 100 ? res : 100;
	}
	
	private int CalculateResourceTick(int resourceCost) {
		return (int)Mathf.Floor(resourceCost / TickPercent);
	}
	
	private bool CheckDescountTime() {
		return CalculateProgress() % TickPercent == 0;
	}
	
	public override void UserInputPriority() {}
	
	public override void Reset() {}
}
