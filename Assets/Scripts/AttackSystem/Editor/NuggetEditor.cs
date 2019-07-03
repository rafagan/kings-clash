//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//
//[CanEditMultipleObjects, CustomEditor(typeof(Nugget))]
//public class NuggetEditor : Editor {
//	public SerializedProperty DamageAlignment;
//	public SerializedProperty TypeOfDamage;
//	public SerializedProperty DamageSchool;
//	public SerializedProperty OnlyAlliedToggle;
//	public SerializedProperty DotTotalTime;
//	public SerializedProperty DotTicks;
//	public SerializedProperty DamageValue;
//	public Nugget _nugget;
//	
//	private bool AllyToggle;
//
//	void OnEnable () {
//		_nugget = target as Nugget;
//		
//		DamageAlignment = serializedObject.FindProperty("Aligment");
//		TypeOfDamage = serializedObject.FindProperty("Type");
//		DamageSchool = serializedObject.FindProperty("School");
//		OnlyAlliedToggle = serializedObject.FindProperty("OnlyAlliedUnits");
//		DotTotalTime = serializedObject.FindProperty("DotTotalTime");
//		DotTicks = serializedObject.FindProperty("DotTicks");
//		DamageValue = serializedObject.FindProperty("Damage");
//	}
//	
//	public override void OnInspectorGUI() {
//		serializedObject.Update();
//		
//		EditorGUILayout.Space();
//		
//		#region LAYOUT
//		GUILayout.BeginHorizontal(); {
//			
//			GUILayout.BeginVertical(); {
//			} GUILayout.EndVertical();
//			
//			GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.ExpandWidth(false)); {
//				
//				//COLOCAR COMPONENTES AQUI
//				EditorGUILayout.Space();
//				GUILayout.Label("Nugget Settings:", "BoldLabel");
//				EditorGUIUtility.LookLikeControls(180, 90);
//				EditorGUILayout.PropertyField(DamageAlignment, new GUIContent("Nugget Aligment: "), GUILayout.Width(270));
//				EditorGUILayout.PropertyField(DamageSchool, new GUIContent("Damage School Element: "), GUILayout.Width(270));
//				EditorGUILayout.PropertyField(TypeOfDamage, new GUIContent("Damage Type: "), GUILayout.Width(270));
//				
//				#region DAMAGE AND DOT ATTRIBUTES
//				if (_nugget.Type == DamageType.DOT) {
//					EditorGUILayout.Space();
//					GUILayout.Label("Dot Settings:", "BoldLabel");
//					_nugget.Damage = EditorGUILayout.FloatField("Total Dot Damage: ", _nugget.Damage);
//					_nugget.DotTotalTime = EditorGUILayout.FloatField("Total Dot Time in Secs: ", _nugget.DotTotalTime);
//					_nugget.DotTicks = EditorGUILayout.IntField("Total Dot Ticks: ", _nugget.DotTicks);
//				} else {
//					_nugget.Damage = EditorGUILayout.FloatField("Nugget Damage: ", _nugget.Damage);
//				}
//				#endregion
//				EditorGUILayout.Space();
//				
//				GUILayout.BeginHorizontal(); {
//					_nugget.OnlyAlliedUnits = EditorGUILayout.Toggle(_nugget.OnlyAlliedUnits, GUILayout.Width(15));
//					GUILayout.Label("Use this only on Self/Ally");
//				} GUILayout.EndHorizontal();
//				
//			}GUILayout.EndVertical();
//			
//			GUILayout.BeginVertical(); {
//			}GUILayout.EndVertical();
//			
//		}GUILayout.EndHorizontal();
//		#endregion
//		
//		serializedObject.ApplyModifiedProperties();
//	}
//}
