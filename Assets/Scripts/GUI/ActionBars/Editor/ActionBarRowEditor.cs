using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ActionBarRow))]
public class ActionBarRowEditor : Editor
{

	enum RowArrangement{Horizontal, Vertical};
	ActionBarRow BarRow;
    public override void OnInspectorGUI()  
    { 
		BarRow = target as ActionBarRow;
		BarRow.ActionBarButtonPrefab = (GameObject) EditorGUILayout.ObjectField("ActionBar Button", BarRow.ActionBarButtonPrefab, typeof(GameObject), false);
		BarRow.ButtonSize = EditorGUILayout.Vector2Field("Button Size", BarRow.ButtonSize);
		BarRow.Columns = EditorGUILayout.IntField("Columns", BarRow.Columns);
		BarRow.ColumnPadding = EditorGUILayout.IntField("Column Padding", BarRow.ColumnPadding);
		BarRow.Rows = EditorGUILayout.IntField("Rows", BarRow.Rows);
		BarRow.RowPadding = EditorGUILayout.IntField("Row Padding", BarRow.RowPadding);
		BarRow.Arrangement = (ActionBarRow.RowArrangement) EditorGUILayout.EnumPopup("Arrangement",BarRow.Arrangement);

		if (GUI.changed)
		{
			EditorUtility.SetDirty(BarRow);
		}
	}
	
}
