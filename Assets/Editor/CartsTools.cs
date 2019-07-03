using UnityEngine;
using System.Collections;
using UnityEditor;

public class CartsTools : EditorWindow {	
	private Texture separatorTexture = Resources.Load("Textures/Editor/separatorTexture") as Texture;
	private Texture newUnitIconTexture = Resources.Load("Textures/Editor/newUnitIcon") as Texture;
	private Texture newNuggetIconTexture = Resources.Load("Textures/Editor/newNuggetIcon") as Texture;
	private Texture newAbilityIconTexture = Resources.Load("Textures/Editor/newAbilityIcon") as Texture;

	
	[MenuItem("CARTS GAME/Tools", false, 1)]
	static void Init() {
		CartsTools toolsWindow = EditorWindow.GetWindow<CartsTools>("Tools");
		
		toolsWindow.maxSize = new Vector2(80, 2000);
		toolsWindow.minSize = new Vector2(80, 100);
	}
	
	void OnGUI() {
		#region WINDOW LAYOUT
		GUILayout.BeginHorizontal(); {
			GUILayout.BeginVertical(); {
			} GUILayout.EndVertical();
			
			GUILayout.BeginVertical(GUILayout.Width(50), GUILayout.ExpandWidth(false)); {
				EditorGUILayout.Space();
				
				if (GUILayout.Button(new GUIContent(newUnitIconTexture, "Create New Unit"), GUILayout.Width(50), GUILayout.Height(50))) {
					CreateCharacter _charWindow = EditorWindow.GetWindow<CreateCharacter>();
					
					_charWindow.maxSize = new Vector2(280, 270);
					_charWindow.minSize = new Vector2(280, 270);
				}
				if (GUILayout.Button(new GUIContent(newNuggetIconTexture, "Create New Nugget"), GUILayout.Width(50), GUILayout.Height(50))) {
					CreateNuggetWindow _window = EditorWindow.GetWindow<CreateNuggetWindow>();
					_window.maxSize = new Vector2(280, 400);
					_window.minSize = new Vector2(280, 400);
				}
				if (GUILayout.Button(new GUIContent(newAbilityIconTexture, "Create New Ability"), GUILayout.Width(50), GUILayout.Height(50))) {
					CreateAbilityWindow abilityWindow = EditorWindow.GetWindow<CreateAbilityWindow>("Create Ability");
					abilityWindow.maxSize = new Vector2(280, 390);
					abilityWindow.minSize = new Vector2(280, 390);	
				}
				
			}GUILayout.EndVertical();
			
			GUILayout.BeginVertical(); {
			}GUILayout.EndVertical();
			
		}GUILayout.EndHorizontal();
		#endregion
	}
	
	private void LineSeparator() {
		GUILayout.Space(3);
		GUILayout.Box(separatorTexture, GUILayout.ExpandWidth(false), GUILayout.Width(50), GUILayout.Height(1.5f));
		GUILayout.Space(3);
	}
}
