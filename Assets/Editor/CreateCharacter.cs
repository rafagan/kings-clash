using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateCharacter : EditorWindow {
	private string unitName = "";
	private float initialLife;
	private float speed;
	private float resEtherCost;
	private float resSteamCost;
	private float resPlasmoCost;
	private int exp;
	private int ether;
	private bool isAttacker = false;
	private bool isConstructor = false;
	private PlayerRole unitRole;
	private PoolsManager poolManager;

	[MenuItem("CARTS GAME/Create Character", false, 9)]
	static void Init() {
		CreateCharacter _charWindow = EditorWindow.GetWindow<CreateCharacter>("Create a Character");
		
		_charWindow.maxSize = new Vector2(280, 270);
		_charWindow.minSize = new Vector2(280, 270);
	}
	
	void OnEnable() {
		poolManager = GameObject.Find("POOLMANAGERS").GetComponent<PoolsManager>();
	}

	void OnGUI() {
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(150, 80);
		
		unitName = EditorGUILayout.TextField("Character Name: ", unitName); 

		EditorGUILayout.Space();
		GUILayout.Label("Character Attributes: ", "BoldLabel");
		
		unitRole = (PlayerRole)EditorGUILayout.EnumPopup("Character Role: ", unitRole); 
				
		initialLife = EditorGUILayout.FloatField("Initial Life: ", initialLife);
		
		speed = EditorGUILayout.FloatField("Speed: ", speed);
		
		exp = EditorGUILayout.IntField("Experience amount: ", exp);
		
		ether = EditorGUILayout.IntField("Ether amount: ", ether);
		
		GUILayout.Label("Resources Costs: ", "BoldLabel");
		EditorGUILayout.BeginHorizontal(); {
			EditorGUIUtility.LookLikeControls(45, 15);
			resSteamCost = EditorGUILayout.FloatField("Steam: ", resSteamCost);
			resEtherCost = EditorGUILayout.FloatField("Ether: ", resEtherCost);
			resPlasmoCost = EditorGUILayout.FloatField("Plasmo: ", resPlasmoCost);
		} EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		EditorGUIUtility.LookLikeControls(150, 80);
		isAttacker = EditorGUILayout.Toggle("Can Attack?", isAttacker);
		isConstructor = EditorGUILayout.Toggle("Can Build Strucutes?", isConstructor);
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal(); {
			if (GUILayout.Button("Create And Close")) {				
				var _character = CreateNewCharacter();

				GameObject _charactersContainer = GameObject.Find("PROTOTYPES/CHARACTER").gameObject;
				if (_charactersContainer == null) {
					var _prototypesContainer = new GameObject("PROTOTYPES");
					_charactersContainer = new GameObject("UNITS");
					_charactersContainer.transform.parent = _prototypesContainer.transform;
				}
			
				_character.transform.parent = _charactersContainer.transform;
				_character.SetActive(false);

				this.Close();
			}
			if (GUILayout.Button("Reset")) {
				unitName = "";
				initialLife = 0;		
				speed = 0;
				exp = 0;
				ether = 0;
				isAttacker = false;
				isConstructor = false;
			}
			if (GUILayout.Button("Close"))
				this.Close();
		} EditorGUILayout.EndHorizontal();
	}

	private GameObject CreateNewCharacter() {
		GameObject _character = new GameObject(unitName);
		_character.layer = 8;
		//CRIAÇÃO DA UNIDADE E SEUS COMPONENTES
		
		_character.AddComponent("BaseUnit");
		var _baseUnit = _character.GetComponent<BaseUnit>();	
		_baseUnit.UnitRole = unitRole;
		
		//Attributes Component
		_character.AddComponent("Attributes");
		var _att = _character.GetComponent<AttributesComponent>();	
		_att.UnitName = unitName;
		_att.InitialLife = initialLife;		
		_att.Speed = speed;
		_att.Experience = exp;
		_att.Ether = ether;
		_att.UnitType = ObjectType.CHARACTER;
		
		//Mobile Component
		_character.AddComponent("MobileUnit");
		
		//Armor Component
		_character.AddComponent("Armor");

		//Another Components
		if (isConstructor)
			_character.AddComponent("Constructor");
		if (isAttacker)
			_character.AddComponent("Attacker");

		//ADICIONA O MESH DA UNIDADE
		GameObject _unitMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		_unitMesh.name = "UnitMesh";
		var _unitMeshCollider = _unitMesh.GetComponent<CapsuleCollider>();
		DestroyImmediate(_unitMeshCollider);
		_unitMesh.transform.parent = _character.transform;
		_unitMesh.transform.localPosition = new Vector3(0, 1.0f, 0);
		
		//Capsule Collider
		_character.AddComponent<CapsuleCollider>();
		var _collider = _character.GetComponent<CapsuleCollider>();
		_collider.radius = 0.5f;
		_collider.height = 2;
		
		//Attributes Pool
		_character.AddComponent("PoolItem");
		
		//Adiciona o Projector
		GameObject _projectorPrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/Projector")) as GameObject;
		_projectorPrefab.transform.parent = _character.transform;
		_projectorPrefab.transform.localPosition = new Vector3(0, 8.0f, 0);
		_projectorPrefab.SetActive(false);
		
		//CRIAÇÃO DO POOL DA UNIDADE
		CreateUnitPool(_character);
		
		return _character;
	}

	private GameObject CreateUnitPool(GameObject character) {		
		#region CRIA O POOL DO ALLYTEAM
		GameObject _unitPool = new GameObject("c" + unitName + "Pool");
		_unitPool.AddComponent("Pool");
		var _pool = _unitPool.GetComponent<Pool>();
		_pool.CapMaxObjects = true;
		_pool.MaxObjects = 50;
		_pool.Prototype = character;
		poolManager.AddNewPool(_unitPool.transform);
		#endregion

		return _unitPool;
	}
}
