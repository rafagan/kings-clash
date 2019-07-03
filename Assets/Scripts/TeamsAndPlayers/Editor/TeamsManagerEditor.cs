using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TeamsManager))]
public class TeamsManagerEditor : Editor {
	public TeamsManager teamsManager;
	public SerializedProperty desiredNumberOfTeamsInGame;
	public SerializedProperty teamsInGame;

	public void OnEnable() {
		teamsManager = target as TeamsManager;
		
		teamsManager.idsManager = teamsManager.transform.GetComponent<UniqueIDsManager>();
		if (teamsManager.idsManager == null)
			teamsManager.idsManager = teamsManager.gameObject.AddComponent<UniqueIDsManager>();
		
		desiredNumberOfTeamsInGame = serializedObject.FindProperty("TeamsInGameAmout");
		teamsInGame = serializedObject.FindProperty("TeamsInCurrentGame");
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		
		#region ROW 1
		GUILayout.BeginHorizontal(); {
			#region COLUMN 1
			GUILayout.BeginVertical(); {
				
			}  GUILayout.EndVertical();
			#endregion
			
			EditorGUILayout.Space();
			
			#region COLUMN 2
			GUILayout.BeginVertical(GUILayout.Width(300)); {
				GUILayout.BeginVertical(); {
					EditorGUILayout.PropertyField(desiredNumberOfTeamsInGame);
				}GUILayout.EndVertical();	
				
				if(GUILayout.Button("Create Teams"))
					teamsManager.CreateTeams();
				
				if(GUILayout.Button("Delete All Teams"))
					teamsManager.RemoveAllTeams();
					
				if(GUILayout.Button("Add New Player"))
					teamsManager.AddNewPlayerToGame();
					
			}GUILayout.EndVertical();
			#endregion
			
			EditorGUILayout.Space();
			
			#region COLUMN 3
			GUILayout.BeginVertical(); {
				
			}GUILayout.EndVertical();
			#endregion
		}GUILayout.EndHorizontal();
		#endregion
		
		serializedObject.ApplyModifiedProperties();
	}
}
