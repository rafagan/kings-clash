using UnityEngine;

public class AttributesComponent : AbstractUnitComponent {
	//Public Attributes
	public string UnitName;
	public bool IsAirUnit = false;
	public int FlightHeight = 0;
    public float InitialLife = 100;
    public float MaxLife = 100;
	public float Speed;
	public float Experience;
	public float Ether;
	[HideInInspector] public bool IsFlying = false;
    public bool IsInvisible = false;
    public bool CanAttack = false;
    public bool Invisible = false;
	public ObjectType UnitType;
	
	public int SteamCost = 0;
	public int PlasmoCost = 0;
	public float TrainingTime = 15.0f;
    public float MyfieldOfView = 6.0f;
    public float MyHeightOfView = 6.0f;

    public float FieldOfView {
        get { return MyfieldOfView; }
        set {
            MyfieldOfView = value;
            var range1 = baseUnit.GetUnitComponent<AttackerRangeView>();
            if(range1 != null) {
                range1.Collider.radius = value;
            }
            var range2 = baseUnit.GetUnitComponent<CollectorRangeView>();
            if (range2 != null) {
                range2.Collider.radius = value;
            }
            var range3 = baseUnit.GetUnitComponent<BuilderRangeView>();
            if (range3 != null) {
                range3.Collider.radius = value;
            }
        }
    }

    public float HeightOfView {
        get { return MyHeightOfView; }
        set {
            MyHeightOfView = value;
            var range1 = baseUnit.GetUnitComponent<AttackerRangeView>();
            if (range1 != null) {
                range1.Collider.height = value;
            }
            var range2 = baseUnit.GetUnitComponent<CollectorRangeView>();
            if (range2 != null) {
                range2.Collider.height = value;
            }
            var range3 = baseUnit.GetUnitComponent<BuilderRangeView>();
            if (range3 != null) {
                range3.Collider.height = value;
            }
        }
    }
	
	public float EtherAmountToVeteran = 300;
	[HideInInspector] public bool isVeteran = false;
	
	//Private Attributes
	private float _currentLife;

	
	#region PROPERTIES
    public float CurrentLife { get { return _currentLife; } set { _currentLife = value; } }
	
	#endregion
	
	void Awake() {
		if (string.IsNullOrEmpty(UnitName))
			UnitName = "NO NAME";
	}
	
	void Start() {
	    if(baseUnit.GetUnitComponent<AttackComponent>() != null)
	        CanAttack = true;
	        
	    LoadAttributes();
	}
	
	void Update() {
	    if(!isVeteran) CheckUnitLevel();
	}
	
	public void HealUnit (float healAmount) {
		if (_currentLife + healAmount >= InitialLife)
			_currentLife = MaxLife;
		else
			_currentLife += healAmount;
	}
	
	public void DamageUnit (float damageAmount) {
		_currentLife -= damageAmount;
	}
	
	 public override void GUIPriority() {
	        
	 }
	 
	 public override void UserInputPriority() {
	
	}
	
	public void LoadAttributes() {
		_currentLife = InitialLife;
	    IsFlying = false;
	}
	
	private void CheckUnitLevel(){	
		if(this.Ether >= EtherAmountToVeteran) isVeteran = true;
	}
	
	public override void Reset() {}
}
