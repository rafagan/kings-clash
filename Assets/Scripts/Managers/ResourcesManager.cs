using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourcesManager : MonoBehaviour {
	public static ResourcesManager Account;
	
	public int TeamID;
	public float EtherMaxValue = 1000.0f;
	public List<ResourceType> resourcesType;
	public List<float> resourcesAmount;
	
	public void Awake () {
		if (Account != null)
			Destroy(Account.gameObject);
		Account = this;
	}
	
	public void Start () {
		TeamID = PlayerManager.Player.PlayerTeam;
	}
	
	public float ReadResource(ResourceType resource) {
		int _index = 0;
		
		foreach (ResourceType _resouce in resourcesType) {
			if (_resouce == resource)
				break;	
			_index++;
		}
		
		return resourcesAmount[_index];
	}

    public void SendMailAddResource(BaseUnit baseUnit, int teamID, ResourceType resource, int cost)
    {
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, teamID, (int)resource, cost));
    }

    public void AddResource(ResourceType resource, float cost) {
		int _index = 0;
		
		foreach (ResourceType _resouce in resourcesType) {
			if (_resouce == resource)
				break;	
			_index++;
		}
        if (resource == ResourceType.Ether && resourcesAmount[_index] + cost >= EtherMaxValue) {
            resourcesAmount[_index] = EtherMaxValue;
        }
        else {
            resourcesAmount[_index] += cost;
        }
	}
	
	public bool HasSufficientResource(ResourceType resource, float cost) {
		int _index = 0;
		
		foreach (ResourceType _resouce in resourcesType) {
			if (_resouce == resource) break;
			_index++;
		}
				
		if (resourcesAmount[_index] >= cost) return true;
		
		return false;
	}
	
	public void DebitResource(ResourceType resource, float cost) {
		int _index = 0;
		
		foreach (ResourceType _resouce in resourcesType) {
			if (_resouce == resource) break;
			_index++;
		}
		
		if (resourcesAmount[_index] >= cost) {
			resourcesAmount[_index] -= cost;
		}
	}
}
