using UnityEngine;
using UnityEditor;
using System;

public static class NetEditorTools
{
	[MenuItem("Component/Net/Normalizar IDs")]
	static public void NormalizeIDs ()
	{
		Undo.RegisterSceneUndo("Normalizar NetObject IDs");
		NetObject[] objs = UnityEngine.Object.FindObjectsOfType(typeof(NetObject)) as NetObject[];
		Array.Sort(objs, delegate(NetObject o1, NetObject o2) { return o1.uid.CompareTo(o2.uid); });
		
		for (int i = 0; i < objs.Length; ++i)
		{
			objs[i].uid = (uint)(1 + i);
			EditorUtility.SetDirty(objs[i]);
		}
	}
}
