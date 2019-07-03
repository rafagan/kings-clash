using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(ArmorComponent))]
public class ArmorEditor : Editor {
	public SerializedProperty DamageTypeDefense;
	public SerializedProperty DamageTypeValue;
	public ArmorComponent _armor;
	
	private Texture deleteTexture;

	void OnEnable () {
		deleteTexture = Resources.Load("Textures/Editor/deleteIcon") as Texture;
		_armor = target as ArmorComponent;
		
		DamageTypeDefense = serializedObject.FindProperty("DamageTypeDefense");
		DamageTypeValue = serializedObject.FindProperty("DefAmount");
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		
		EditorGUILayout.Space();
		
		#region DAMAGE TYPE LAYOUT
		GUILayout.BeginHorizontal(); {
			
			GUILayout.BeginVertical(); {
			} GUILayout.EndVertical();
			
			GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.ExpandWidth(false)); {
				
				GUILayout.BeginHorizontal(); {
					if (GUILayout.Button("Add Defense Type", GUILayout.MaxWidth(150), GUILayout.Width(150))) {
						DamageTypeDefense.arraySize++;
						_armor.DamageTypeDefense.Add(DmgType.PIERCE);
						DamageTypeValue.arraySize++;
						_armor.DefAmount.Add(0.0f);
					}
					if (GUILayout.Button("Clear All Defenses", GUILayout.MaxWidth(150), GUILayout.Width(150))) {
						DamageTypeDefense.ClearArray();
						DamageTypeValue.ClearArray();
						
						_armor.DamageTypeDefense.Clear();
						_armor.DefAmount.Clear();
					}
				} GUILayout.EndHorizontal();
				
				ShowDamageTypesField();
				
			}GUILayout.EndVertical();
			
			GUILayout.BeginVertical(); {
			}GUILayout.EndVertical();
			
		}GUILayout.EndHorizontal();
		#endregion
		
		EditorGUILayout.Space();
		
		serializedObject.ApplyModifiedProperties();
	}
	
	private void ShowDamageTypesField() {
		if (_armor.DamageTypeDefense.Count > 0) {
			EditorGUILayout.Space();
			GUILayout.Label("Damage Taken in %:", "BoldLabel");
			
			for (int i = 0; i < _armor.DamageTypeDefense.Count; i++ ) {
				GUILayout.BeginHorizontal(); {
					EditorGUIUtility.LookLikeControls(50, 80);
					
					SerializedProperty _damageTypeProperty = DamageTypeDefense.GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(_damageTypeProperty, new GUIContent("Damage Type: "), GUILayout.Width(130));
					
					GUILayout.Space(20);
					
					SerializedProperty _damageValue = DamageTypeValue.GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(_damageValue, new GUIContent("Percent: "), GUILayout.Width(130));
					
					if (GUILayout.Button(deleteTexture, "label", GUILayout.MaxWidth(18), GUILayout.MaxHeight(18))) {
						_armor.DamageTypeDefense.RemoveAt(i);
						_armor.DefAmount.RemoveAt(i);	
						
						DamageTypeDefense.DeleteArrayElementAtIndex(i);
						DamageTypeValue.DeleteArrayElementAtIndex(i);
					}
				} GUILayout.EndHorizontal();
			}
		}
	}
}
