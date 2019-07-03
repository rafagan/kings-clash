using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class CreateAbilityWindow : EditorWindow {
	#region PRIVATE EDITOR VARIABLES
	private List<Transform> nuggetsList;
	private List<Nugget> nuggets;
	private List<string> nuggetNames;
	private Nugget selectedNugget;
	//GUI EDITOR PARAMETERS
	private Vector2 scrollPos;
	private Texture deleteTexture;
	//NEW ABILITY PARAMETERS
	private bool isRanged = false;
	private List<Nugget> nuggetsToUse;
	private int index = 0;
	private string abilityName = "Name";
	private float abilityRange = 0;
	private float abilityReload = 0;
	#endregion

	[MenuItem("CARTS GAME/Create Ability", false, 11)]
	static void Init() {
		CreateAbilityWindow abilityWindow = EditorWindow.GetWindow<CreateAbilityWindow>("Create Ability");
		
		abilityWindow.maxSize = new Vector2(280, 390);
		abilityWindow.minSize = new Vector2(280, 390);
	}
	
	void OnEnable() {
		deleteTexture = Resources.Load("Textures/Editor/deleteIcon") as Texture;
		nuggetNames = new List<string>();
		nuggetsList = new List<Transform>();
		nuggetsToUse = new List<Nugget>();
		nuggets = new List<Nugget>();
		
		var _nuggetsContainer = GameObject.Find("PROTOTYPES/NUGGETS").transform;
		if (_nuggetsContainer != null) {
			foreach(Transform _nugget in _nuggetsContainer) {
				nuggetsList.Add(_nugget);
				nuggets.Add(_nugget.GetComponent<Nugget>());
			}
		}
		else {
			CreateNuggetsContainer();
		}
	}
	
	void OnGUI() {
		#region ROW 1
		GUILayout.BeginVertical(GUILayout.Width(115)); {
			
			EditorGUILayout.Space();
			GUILayout.Label("Ability Settings", "BoldLabel");
			GUILayout.BeginHorizontal(); {
				
				GUILayout.Label("Ability Name:", GUILayout.Width(120));
				abilityName = GUILayout.TextField(abilityName, GUILayout.Width(140));
			
			} GUILayout.EndHorizontal();
			
			EditorGUIUtility.LookLikeControls(124, 102);
			isRanged = EditorGUILayout.Toggle("Is Ranged: ", isRanged);
			
			GUI.enabled = (isRanged);
			GUILayout.BeginHorizontal(); {
				GUILayout.Label("Ability Range:", GUILayout.Width(120));
				abilityRange = EditorGUILayout.FloatField(abilityRange, GUILayout.Width(140));
				if (!isRanged)
					abilityRange = 1.5f;
			} GUILayout.EndHorizontal();
			GUI.enabled = true;
			
			GUILayout.BeginHorizontal(); {
				GUILayout.Label("Reload Time:", GUILayout.Width(120));
				abilityReload = EditorGUILayout.FloatField(abilityReload, GUILayout.Width(140));
			} GUILayout.EndHorizontal();
			
			
		} GUILayout.EndVertical();
		#endregion
		
		#region ROW 2
		GUILayout.BeginHorizontal(); {
			#region COLUMN 1
			GUILayout.BeginVertical(GUILayout.Width(115)); {
				#region AVALIABLE NUGGETS LIST
				CheckNuggetsContainer();
				
				string[] _currentNames = new string[nuggetNames.Count];
				for (int i = 0; i < nuggetNames.Count; i++)
					_currentNames[i] = nuggetNames[i];
				
				GUILayout.Label("Select Nugget: ", "BoldLabel");
				index = EditorGUILayout.Popup(index, _currentNames, GUILayout.Width(115));
				
				#endregion
				EditorGUILayout.Space();
				if (_currentNames.Length > 0) {
					GUI.enabled = !CheckInUseNugget(_currentNames[index]);
					if (GUILayout.Button("Use Nugget", GUILayout.Width(115), GUILayout.Height(26)))
						AddNuggetToUse(_currentNames[index]);
					GUI.enabled = true;
				}
				
				EditorGUILayout.Space();
				if (GUILayout.Button("Create Nugget", GUILayout.Width(115), GUILayout.Height(26))) {
					CreateNuggetWindow _window = EditorWindow.GetWindow<CreateNuggetWindow>();
					_window.maxSize = new Vector2(280, 400);
					_window.minSize = new Vector2(280, 400);
				}
			} GUILayout.EndVertical();
			#endregion
			#region COLUMN 2
			GUILayout.BeginVertical(GUILayout.Width(40)); {
				//COLUMN 2
				
			}GUILayout.EndVertical();
			#endregion
			#region COLUMN 3
			GUILayout.BeginVertical(GUILayout.Width(115)); {
				//COLUMN 3
				
			}GUILayout.EndVertical();
			#endregion
		}GUILayout.EndHorizontal();
		#endregion
		
		#region ROW 3
		GUILayout.Label("Selected Nuggets: ", "BoldLabel");
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(275), GUILayout.Height(100));
			if (nuggetsToUse.Count > 0) {
				var _count = 0;
				foreach (Nugget _nugget in nuggetsToUse) {
					GUILayout.BeginHorizontal(GUILayout.Width(150)); {
						GUILayout.Label(_nugget.nuggetName, GUILayout.Width(220));
						if (GUILayout.Button(deleteTexture, "label", GUILayout.Width(18), GUILayout.Height(18))) {
							nuggetsToUse.RemoveAt(_count);
							break;
						}
					} GUILayout.EndHorizontal();
					_count++;
				}
			}
		EditorGUILayout.EndScrollView();
		#endregion
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		#region ROW 4 BUTTONS
		EditorGUILayout.BeginHorizontal(); {
			if (GUILayout.Button("Create And Close")) {				
				CreateAbility();
				this.Close();
			}
			if (GUILayout.Button("Reset")) {
				ResetParameters();
			}
			if (GUILayout.Button("Close"))
				this.Close();
		} EditorGUILayout.EndHorizontal();
		#endregion BUTTONS
	}
	
	private void CheckNuggetsContainer() {
		GameObject _nuggetsContainer = GameObject.Find("PROTOTYPES/NUGGETS").gameObject;
		if (_nuggetsContainer != null) {
			if (nuggetsList.Count != _nuggetsContainer.transform.childCount) {
				nuggetsList.Clear();
				nuggetNames.Clear();
				nuggets.Clear();
				foreach(Transform _nugget in _nuggetsContainer.transform) {
					nuggetsList.Add(_nugget);
					nuggets.Add(_nugget.GetComponent<Nugget>());
				}
			}
		}
		
		if (nuggetsList.Count > 0) {
			foreach(Transform nugget in nuggetsList) {
				nuggetNames.Add(nugget.name);
			}
		}
	}
	
	private bool CheckInUseNugget(string nuggetName) {
		if (nuggetsToUse.Count > 0) {
			foreach (Nugget _nugget in nuggetsToUse) {
				if (_nugget.transform.name == nuggetName)
					return true;
			}
		}
		return false;
	}
	
	private void AddNuggetToUse(string nuggetName) {
		if (nuggets.Count > 0) {
			foreach (Nugget _nugget in nuggets) {
				if (_nugget.transform.name == nuggetName)
					nuggetsToUse.Add(_nugget);
			}
		}
	}
	
	private void CreateAbility() {
		var _newAbility = new GameObject(abilityName);
		_newAbility.transform.parent = GetAbilityContainer();
		_newAbility.SetActive(false);
		
		Ability _newAbilityComponent = _newAbility.AddComponent<Ability>();
		_newAbilityComponent.Nuggets = nuggetsToUse;
		_newAbilityComponent.attackRange = abilityRange;
		_newAbilityComponent.reloadTime = abilityReload;
	}
	
	private void ResetParameters() {
		isRanged = false;
		abilityName = "Name";
		abilityRange = 0;
		abilityReload = 0;
		nuggetsToUse.Clear();
	}

	private void CreateNuggetsContainer ()
	{
		var _damageNuggets = new GameObject("NUGGETS");
		var _prototypesContainer = GameObject.Find("PROTOTYPES");
		if (_prototypesContainer == null)
			_prototypesContainer = new GameObject("PROTOTYPES");
		_damageNuggets.transform.parent = _prototypesContainer.transform;
	}
	
	private Transform GetAbilityContainer () {
		var _prototypesContainer = GameObject.Find("PROTOTYPES");
		if (_prototypesContainer == null)
			_prototypesContainer = new GameObject("PROTOTYPES");
		var _abilityContainer = _prototypesContainer.transform.SearchChild("ABILITIES").gameObject;
		if (_abilityContainer == null) {
			_abilityContainer = new GameObject("ABILITIES");
			_abilityContainer.transform.parent = _prototypesContainer.transform;
		}
		return _abilityContainer.transform;
	}
}
