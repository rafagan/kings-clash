using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class CreateNuggetWindow : EditorWindow {
	#region PRIVATE EDITOR VARIABLES
	//NUGGET PARAMETERS
	private DmgType damageNuggetType = DmgType.PIERCE;
	private bool isDot = false;
	private bool isAoe = false;
	private bool nameInUse = false;
	private string nuggetName = "Nugget Name";
	private float nuggetDamage = 0;
	//KNOCKBACK PARAMETERS
	private float kbInnerForce = 0.0f;
	private float kbInnerRadius = 0.0f;
	private float kbOuterForce = 0.0f;
	private float kbOuterRadius = 0.0f;
	//AOE PARAMETERS
	private float aoeInnerDamagePercent = 0.0f;
	private float aoeInnerRadius = 0.0f;
	private float aoeOuterDamagePercent = 0.0f;
	private float aoeOuterRadius = 0.0f;
	//DOT PARAMETERS
	private float nuggetDotTotalTime = 0.0f;
	private int nuggetDotTicks = 0;
	//GUI EDITOR PARAMETERS
	private Transform nuggetsContainer;
	private List<Transform> nuggetsList;
	private GUIStyle currentNameFieldColor = new GUIStyle(EditorStyles.textField);
	private GUIStyle inUseNameFieldColor = new GUIStyle(EditorStyles.textField);
	private GUIStyle normalNameFieldColor = new GUIStyle(EditorStyles.textField);
	#endregion
	
	[MenuItem("CARTS GAME/Create Nugget", false, 10)]
	static void Init () {
		CreateNuggetWindow nuggetWindow = EditorWindow.GetWindow<CreateNuggetWindow>("Create Nugget");
		
		nuggetWindow.maxSize = new Vector2(280, 400);
		nuggetWindow.minSize = new Vector2(280, 400);
	}
	
	void OnEnable () {
		SetTextFieldColor();
		nuggetsList = new List<Transform>();
		
		GameObject _nuggetsContainer = GameObject.Find("NUGGETS");
		if (_nuggetsContainer == null) {
			CreateNuggetsContainer();
		}
		else
			nuggetsContainer = _nuggetsContainer.transform;
		if (nuggetsContainer.childCount > 0) {
			foreach(Transform _nugget in nuggetsContainer)
				nuggetsList.Add(_nugget);
		}
	}
	
	void OnGUI () {
		EditorGUILayout.Space();
		
		#region NUGGET PARAMETERS
		GUILayout.Label("Create new Nugget", "BoldLabel");
		
		EditorGUILayout.BeginHorizontal(); {
			GUILayout.Label("Nugget Name: ");
			nuggetName = GUILayout.TextField(nuggetName, currentNameFieldColor, GUILayout.MaxWidth(150), GUILayout.Width(150));
			nameInUse = CheckInUseName(nuggetName);
		} EditorGUILayout.EndHorizontal();
		
		EditorGUIUtility.LookLikeControls(180, 100);
		damageNuggetType = (DmgType)EditorGUILayout.EnumPopup("Nugget Damage Type: ", damageNuggetType);
		nuggetDamage = EditorGUILayout.FloatField("Nugget Damage: ", nuggetDamage);
		nuggetDamage = nuggetDamage <= 0 ? 0 : nuggetDamage;
		#endregion
		
		EditorGUILayout.Space();
		
		//KNOCKBACK PARAMETERS
		GUI.enabled = (damageNuggetType == DmgType.KNOCKBACK);
		GUILayout.Label("Is Knockback");
		kbInnerForce = EditorGUILayout.FloatField("Inner Knockback Force: ", kbInnerForce);
		kbInnerForce = kbInnerForce <= 0 ? 0 : kbInnerForce;
		kbOuterForce = EditorGUILayout.FloatField("Outer Knockback Force: ", kbOuterForce);
		kbOuterForce = kbOuterForce <= 0 ? 0 : kbOuterForce;
		kbInnerRadius = EditorGUILayout.FloatField("Inner Knockback Radius: ", kbInnerRadius);
		kbInnerRadius = kbInnerRadius <= 0 ? 0 : kbInnerRadius;
		kbOuterRadius = EditorGUILayout.FloatField("Outer Knockback Radius: ", kbOuterRadius);
		kbOuterRadius = kbOuterRadius <= 0 ? 0 : kbOuterRadius;
		GUI.enabled = true;
		
		EditorGUILayout.Space();
		
		//DOT PARAMETERS
		isDot = EditorGUILayout.Toggle("Is Dot", isDot);
		GUI.enabled = (isDot);
		nuggetDotTotalTime = EditorGUILayout.FloatField("Total Dot Time (secs): ", nuggetDotTotalTime);
		nuggetDotTotalTime = nuggetDotTotalTime <= 0 ? 0 : nuggetDotTotalTime;
		nuggetDotTicks = EditorGUILayout.IntField("Dot Ticks: ", nuggetDotTicks);
		nuggetDotTicks = nuggetDotTicks <= 0 ? 0 : nuggetDotTicks;
		GUI.enabled = true;
		
		EditorGUILayout.Space();
		
		//AOE PARAMETERS
		isAoe = EditorGUILayout.Toggle("Is AOE", isAoe);
		GUI.enabled = (isAoe);
		aoeInnerDamagePercent = EditorGUILayout.FloatField("Inner AOE Damage (%): ", aoeInnerDamagePercent);
		aoeInnerDamagePercent = aoeInnerDamagePercent <= 0 ? 0 : aoeInnerDamagePercent;
		aoeOuterDamagePercent = EditorGUILayout.FloatField("Outer AOE Damage (%): ", aoeOuterDamagePercent);
		aoeOuterDamagePercent = aoeOuterDamagePercent <= 0 ? 0 : aoeOuterDamagePercent;
		aoeInnerRadius = EditorGUILayout.FloatField("Inner AOE Radius: ", aoeInnerRadius);
		aoeInnerRadius = aoeInnerRadius <= 0 ? 0 : aoeInnerRadius;
		aoeOuterRadius = EditorGUILayout.FloatField("Outer AOE Radius: ", aoeOuterRadius);
		aoeOuterRadius = aoeOuterRadius <= 0 ? 0 : aoeOuterRadius;
		GUI.enabled = true;	
		
		#region BUTTONS
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal(); {
			GUI.enabled = !nameInUse;
			if (GUILayout.Button("Create And Close")) {				
				CreateNugget();
				this.Close();
			}
			GUI.enabled = true;
			if (GUILayout.Button("Reset")) {
				ResetParameters();
			}
			if (GUILayout.Button("Close"))
				this.Close();
		} EditorGUILayout.EndHorizontal();
		#endregion
	}
	
	private void CreateNugget() {
		GameObject _newNuggetObject = new GameObject(nuggetName);
		_newNuggetObject.SetActive(false);
		_newNuggetObject.transform.parent = nuggetsContainer;
		
		Nugget _newNugget = _newNuggetObject.AddComponent<Nugget>();
		_newNugget.nuggetName = nuggetName;
		_newNugget.damageType = damageNuggetType;
		_newNugget.damageValue = nuggetDamage;
		
		//Knockback parameters
		if (damageNuggetType == DmgType.KNOCKBACK) {
			_newNugget.KnockbackInnerRadius = kbInnerRadius;
			_newNugget.KnockbackOuterRadius = kbOuterRadius;
			_newNugget.KnockbackInnerForce = kbInnerForce;
			_newNugget.KnockbackOuterForce = kbOuterForce;
		}
		
		//Dot parameters
		if (isDot) {
			_newNugget.isDot = isDot;
			_newNugget.DotTicks = nuggetDotTicks;
			_newNugget.DotTotalTime = nuggetDotTotalTime;
		}
		
		if (isAoe) {
			_newNugget.isAoe = isAoe;
			_newNugget.AoeInnerRadius = aoeInnerRadius;
			_newNugget.AoeOuterRadius = aoeOuterRadius;
			_newNugget.AoeInnerDamagePercent = aoeInnerDamagePercent;
			_newNugget.AoeOuterDamagePercent = aoeOuterDamagePercent;
		}
	}
	
	private void ResetParameters() {
		//NUGGET PARAMETERS
		damageNuggetType = DmgType.PIERCE;
		nuggetName = "Nugget Name";
		isDot = false;
		isAoe = false;
		nameInUse = false;
		nuggetDamage = 0;
		//KNOCKBACK PARAMETERS
		kbInnerForce = 0.0f;
		kbInnerRadius = 0.0f;
		kbOuterForce = 0.0f;
		kbOuterRadius = 0.0f;
		//AOE PARAMETERS
		aoeInnerDamagePercent = 0.0f;
		aoeInnerRadius = 0.0f;
		aoeOuterDamagePercent = 0.0f;
		aoeOuterRadius = 0.0f;
		//DOT PARAMETERS
		nuggetDotTotalTime = 0.0f;
		nuggetDotTicks = 0;
	}
	
	private bool CheckInUseName(string nuggetname) {
		if (nuggetsContainer != null) {
			if (nuggetsContainer.childCount > 0) {
				foreach(Transform _nugget in nuggetsContainer) {
					if (_nugget.name == nuggetname) {
						if (currentNameFieldColor != inUseNameFieldColor)
							currentNameFieldColor = inUseNameFieldColor;
						return true;
					}
				}
			}
			if (currentNameFieldColor != normalNameFieldColor)
				currentNameFieldColor = normalNameFieldColor;
		}
		return false;
	}
	
	private void SetTextFieldColor () {
		inUseNameFieldColor.normal.textColor = Color.red;
		inUseNameFieldColor.onNormal.textColor = Color.red;
		inUseNameFieldColor.focused.textColor = Color.red;
		inUseNameFieldColor.onFocused.textColor = Color.red;
		inUseNameFieldColor.active.textColor = Color.red;
		inUseNameFieldColor.onActive.textColor = Color.red;
		
		normalNameFieldColor.normal.textColor = Color.white;
		normalNameFieldColor.onNormal.textColor = Color.white;
		normalNameFieldColor.focused.textColor = Color.white;
		normalNameFieldColor.onFocused.textColor = Color.white;
		normalNameFieldColor.active.textColor = Color.white;
		normalNameFieldColor.onActive.textColor = Color.white;
	}
	
	void CreateNuggetsContainer ()
	{
		var _damageNuggets = new GameObject("NUGGETS");
		var _prototypesContainer = GameObject.Find("PROTOTYPES");
		if (_prototypesContainer == null)
			_prototypesContainer = new GameObject("PROTOTYPES");
		_damageNuggets.transform.parent = _prototypesContainer.transform;
	}
}
