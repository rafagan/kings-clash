using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public enum ObjectType { none, CHARACTER, STRUCTURE, RESOURCE, CRUDERESOURCE }
public enum DeathState { Alive, InDeathAnim, DeathAnimFinished, InBurrowAnim, BurrowAnimFinished }

public class BaseUnit : MonoBehaviour {
	//Public Attributes
	public string UnitIconName;
	public PlayerRole UnitRole;
	public Unit UnitClass;
	public GameObject ProjectorPrefab;
	public int Tier;
	public int TeamID;
    public Transform DamageTarget;
    public Transform Shooter;
	[HideInInspector][SerializeField]
	private Animator _animatorBackupField;
	[HideInInspector]public Animator Anim {
		get
		{
			if (_animatorBackupField == null && MeshGO != null)
					_animatorBackupField = MeshGO.GetComponent<Animator>();
			return _animatorBackupField;
		}
	}
    public int ThreatLevel = 1;
	
	//Toggles
	public bool IsResource = false;
	public bool IsGrounded = false;
	
	//Materials Attributes
	public Material SelfProjectorMaterial;
	public Material EnemyProjectorMaterial;
	public Material AllyProjectorMaterial;
	
	//Unit Components
	private List<AbstractUnitComponent> unitComponents;
	
	//Private Attributes
	private Projector projector;
	private bool isSelected = false;
	private float groundDistance = 0;
	public float maxGroundDistance = 1f;
	
	//GUI Attributes
	private GUIStyle currentBoxColor;
	private GUIStyle allyBoxColor;
	private GUIStyle enemyBoxColor;
	private GUIStyle selfBoxColor;
	
	#region Atributos relacionados à morte da unidade
	public bool IsDead { 
		get { 
			return (CurrentDeathState != DeathState.Alive); 
		} set { 
			if (value == true) CurrentDeathState = DeathState.InDeathAnim; else CurrentDeathState = DeathState.Alive; 
			} 
		}
	public DeathState CurrentDeathState = DeathState.Alive;
	public float DeathAnimationDuration = 5.0f;
	public float BurySpeed = 5.0f;
	public float BuryTime = 5.0f;
	#endregion
	
	#region PROPERTIES
	public ObjectType UnitType { get { return GetUnitComponent<AttributesComponent>().UnitType; } set { GetUnitComponent<AttributesComponent>().UnitType = value; } }
	public bool IsSelected { get { if (this.IsEnemy) return false; else return isSelected; } set { if (ProjectorPrefab != null) { if (value) ProjectorPrefab.SetActive(true); else ProjectorPrefab.SetActive(false); } isSelected = value; } }
	public int UniqueID { get { return GetUnitComponent<PoolItemComponent>().PoolItemID; } set { GetUnitComponent<PoolItemComponent>().PoolItemID = value; } }
	public string UnitName { get { return GetUnitComponent<AttributesComponent>().UnitName; } }
	public GameObject MeshGO;
	public int UnitTeam { get { return TeamID; } set { TeamID = value;} }
	public List<AbstractUnitComponent> GetAllUnitComponents { get {return unitComponents;} }
    public bool IsEnemy {
        get {
            if (PlayerManager.Player != null && PlayerManager.Player.Spectator)
                return false;
            if (PlayerManager.Player != null)
            {
                return (TeamID != PlayerManager.Player.PlayerTeam);
            }
               
            return false;
        }
    }

	#endregion
	
	void Awake() {
		unitComponents = new List<AbstractUnitComponent>();
		
		//Busca os componentes no pai
		foreach (AbstractUnitComponent _unitComponent in transform.GetComponents<AbstractUnitComponent>()) {
			if (_unitComponent != null) unitComponents.Add(_unitComponent);
		}
		//Busca os componentes nos filhos
		for (int i = 0; i < transform.childCount; i++) {
			foreach (AbstractUnitComponent _unitComponent in transform.GetChild(i).GetComponents<AbstractUnitComponent>()) {
				if (_unitComponent != null) unitComponents.Add(_unitComponent);
			}
		}
		//Busca os componentes no gameObject Components
		Transform _componentsContainer = transform.Find("Components");
		if (_componentsContainer != null && _componentsContainer.transform.childCount > 0) {
			for (int i = 0; i < _componentsContainer.transform.childCount; i++) {
				foreach (AbstractUnitComponent _unitComponent in _componentsContainer.transform.GetChild(i).GetComponents<AbstractUnitComponent>()) {
					if (_unitComponent != null) unitComponents.Add(_unitComponent);
				}
			}
		}
		
		//Atribui o baseUnit para todos os componentes encontrados;
		foreach (AbstractUnitComponent _component in unitComponents) {
			_component.baseUnit = this;
		}
		
		//Atribui o baseUnit para todos os COMMANDS da unidade;
	    if (!IsResource) {
            foreach (AbstractCommand _command in transform.Find("Commands").GetComponents<AbstractCommand>())
                _command.OwnerUnit = this;
	    }

		if (currentBoxColor == null) currentBoxColor = UnityEngine.GUIStyle.none;
	}

	void Start() {		
		//Ajusta os projectors
		if (!IsResource) {		
			ProjectorPrefab = transform.SearchChild("Projector").gameObject;
			if (projector == null)
				projector = ProjectorPrefab.GetComponent<Projector>();
			
			SetupMaterials();
			SetupGUI();
			AdjustColorsAndProjector();
		}
		
		GameManager.Manager.unitsManager.AddUnitInScene(this);
		
	}
	
	void Update () {
		if (!IsResource) 
			AdjustColorsAndProjector();
		
		CheckGroundDistance();
	}

    void OnEnable() {
        IsDead = false;
    }

    void OnDisable() {
        IsDead = true;
    }
	
	public T GetUnitComponent<T> () {
		if (unitComponents != null && unitComponents.Count > 0) {
			foreach (AbstractUnitComponent _unitComponent in unitComponents) {
				if (_unitComponent.GetType() == typeof(T)) return (T)Convert.ChangeType(_unitComponent, typeof(T));
			}
		}
		
		return default(T);
	}

    public bool IsInFOV(BaseUnit target){
        if (target != null)
            return (Vector3.Distance(transform.position, target.transform.position) <= GetUnitComponent<AttributesComponent>().FieldOfView);

        return false;
    }
    public bool IsInFOV(Vector3 destiny) {
        return (Vector3.Distance(transform.position, destiny) <= GetUnitComponent<AttributesComponent>().FieldOfView);
    }
	
	public bool ContainsComponent (string componentName) {
		foreach (AbstractUnitComponent _unitComponent in unitComponents) {
			if (componentName == _unitComponent.name) return true;
		}
		
		return false;
	}
	
	void SetupGUI ()
	{
		currentBoxColor = new GUIStyle();
		allyBoxColor = new GUIStyle();
		allyBoxColor.normal.textColor = Color.blue;;
		allyBoxColor.alignment = TextAnchor.MiddleCenter;
		enemyBoxColor = new GUIStyle();
		enemyBoxColor.normal.textColor = Color.red;;
		enemyBoxColor.alignment = TextAnchor.MiddleCenter;
		selfBoxColor = new GUIStyle();
		selfBoxColor.normal.textColor = Color.green;;
		selfBoxColor.alignment = TextAnchor.MiddleCenter;
	}
	
	private void SetupMaterials () {
		SelfProjectorMaterial = Resources.Load("Materials/Projector/ProjectorSelfUnitSelected") as Material;
		AllyProjectorMaterial = Resources.Load("Materials/Projector/ProjectorAllyUnitSelected") as Material;
		EnemyProjectorMaterial = Resources.Load("Materials/Projector/ProjectorEnemyUnitSelected") as Material;
	}

	private void AdjustColorsAndProjector ()
	{		
		//Ajusta as cores de acordo com o player role
		if (IsEnemy || UnitRole == PlayerRole.NPC) {
			currentBoxColor = enemyBoxColor;
			projector.material = EnemyProjectorMaterial;	
		}
		else if (PlayerManager.Player.CheckPlayerRole(UnitRole)) {
			currentBoxColor = selfBoxColor;
			projector.material = SelfProjectorMaterial;
		}
		else if (!PlayerManager.Player.CheckPlayerRole(UnitRole)) {
			currentBoxColor = allyBoxColor;
			projector.material = AllyProjectorMaterial;		
		}
	}
	
	void CheckGroundDistance() {
		var _raycast = new Ray(transform.position, Vector3.down);
		var _hit = new RaycastHit();

		if (Physics.Raycast(_raycast, out _hit, 500f)) {
			if (_hit.transform.gameObject.layer == 10) {
				groundDistance = Vector3.Distance(transform.position, _hit.point);
			}
		}	
		
		IsGrounded = groundDistance <= maxGroundDistance;
	}
	
	public bool CheckIfIsMyEnemy(BaseUnit target) {
		return target.TeamID != this.TeamID;
	}
	
	public void Reset() {
		//Marca como alive
		CurrentDeathState = DeathState.Alive;
		
		//Percorre todas as AbstractUnitComponents resetando-as
		var _components = GetAllUnitComponents;
		if (_components != null && _components.Count > 0){
			foreach (AbstractUnitComponent _component in _components) {
				//Ativa todos os componentes
				_component.enabled = true;
				_component.Reset();
			}
		}
	}
	
	public void StartBuryUnit() {
		StartCoroutine("Bury");
	}
	
	private IEnumerator Bury() {
		yield return new WaitForSeconds(BuryTime);
		
		CurrentDeathState = DeathState.BurrowAnimFinished;
	}
}
