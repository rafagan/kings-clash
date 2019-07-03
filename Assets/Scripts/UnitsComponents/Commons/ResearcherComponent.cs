using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResearcherComponent : AbstractUnitComponent {
	public List<int> UpgradesToResearchID;
	
	void Start () {
		if (baseUnit == null) enabled = false;
	}
	
	 public override void GUIPriority() {
		if (baseUnit.IsSelected && !baseUnit.IsEnemy && UpgradesToResearchID.Count > 0) {
			if (PlayerManager.Player.CheckPlayerRole(baseUnit.UnitRole)) {
				GUI.Label(new Rect(900.0f, 2.0f, 200.0f, 35.0f), "UPGRADES:");
	
				var _initialXpos = 900.0f;
				var _initialYpos = 25.0f;
				var _buttonOffset = 2.0f;
				var _buttonWidth = 120.0f;
				var _buttonHeight = 35.0f;
	
				foreach (int _upgradeID in UpgradesToResearchID) {
					Rect _buttonRect = new Rect(_initialXpos, _initialYpos, _buttonWidth, _buttonHeight);
					Upgrade _upgrade = UpgradesManager.Manager.GetUpgradeByID(_upgradeID);
					
					if(GUI.Button(_buttonRect, _upgrade.gameObject.name) && !_upgrade.Upgraded && _upgrade.CanResearch)
						_upgrade.Research();
						
					_initialYpos += _buttonOffset + _buttonHeight;
				}
			}
		}
	}
	
	public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
