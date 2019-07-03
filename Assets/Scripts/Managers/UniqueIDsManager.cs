using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UniqueIDsManager : MonoBehaviour {
	public List<int> UniquesIDs;

	public int GetNewUniqueID() {
		if (UniquesIDs == null || UniquesIDs.Count == 0) {
			UniquesIDs = new List<int>();
			UniquesIDs.Add(1);
			return 1;
		}
		
		for (int i = 1; i < int.MaxValue; i++) {
			if (!UniquesIDs.Contains(i)) {
				UniquesIDs.Add(i);
				return i;
			}
		}
		
		return 0;
	}
	
	public void RemoveUniqueID(int ID) {
		if (UniquesIDs != null && UniquesIDs.Count > 0 && UniquesIDs.Contains(ID))
			UniquesIDs.Remove(ID);
	}
	
	public void ClearAllIDs() {
		if (UniquesIDs != null && UniquesIDs.Count > 0)
			UniquesIDs.Clear();
	}
}
