using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Upgrade : MonoBehaviour {
	public List<Upgrade> Requirements;
	public List<Pool> PoolUnits;
	public string UpgradeDescription;
	public float ResearchTimeInSecs;
	public int SteamCost;
	public int PlasmoCost;
	public int UpgradeID = 0;
	public bool Upgraded = false;
	public bool GlobalUpgrade = false;
	
	public bool CanResearch { get { return CheckPrerequisites(); } }
	
	public void Research() {
			ResourcesManager.Account.DebitResource(ResourceType.Steam, SteamCost);
			ResourcesManager.Account.DebitResource(ResourceType.Plasmo, PlasmoCost);
			StartCoroutine("StartResearch");
	}
	
	private IEnumerator StartResearch() {
		yield return new WaitForSeconds(ResearchTimeInSecs);
		
		if(GlobalUpgrade)
		{
			foreach(Pool pool in PoolUnits)
			{
				UpgradesManager.Manager.AddUpgradeToEntirePool(this, pool);
			}
		}
		Upgraded = true;
	}
	
	private bool CheckPrerequisites() {
		var _trueCount = 0;
		if (Requirements.Count > 0) {
			foreach (Upgrade _upgrade in Requirements) {
				if (_upgrade.Upgraded) 
					_trueCount++;
			}
			
			if 	(_trueCount != Requirements.Count) 
				return false;
		}
		
		return true;
	}
	
	private bool CheckResourceCost() {
		if (!ResourcesManager.Account.HasSufficientResource(ResourceType.Steam, SteamCost) &&
			!ResourcesManager.Account.HasSufficientResource(ResourceType.Plasmo, PlasmoCost))
			return false;
		
		return true;
	}
}
