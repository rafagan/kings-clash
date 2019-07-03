using UnityEngine;
using System.Collections;

public class ExtractorComponent : AbstractUnitComponent {
	public float ExtractionTime = 5.0f;
	public int ExtractionAmount = 10;
	
	private bool canExtract = true;
	private SteamComponent steamResouce;
	
	public bool Accopled { get { return steamResouce != null; } }

	void Start () {

	}
	
	void Update () {
		if (steamResouce != null && canExtract && baseUnit.GetUnitComponent<StructureComponent>().built)
			StartCoroutine("Extract");
	}
	
	private IEnumerator Extract () {
		canExtract = false;
		
		yield return new WaitForSeconds(ExtractionTime);
	    var _amount = steamResouce.Gather(ExtractionAmount);
        MailMan.Post.NewMail(new Mail("AddResource", baseUnit.UniqueID, baseUnit.TeamID, 0, (int)_amount));

		canExtract = true;
	}

    public void AttachGeyser(SteamComponent geyser)
    {
        if (geyser)
        {
            steamResouce = geyser;
            steamResouce.Occupied = true;
        }
    }

    public void RemoveGeyser()
    {
        if (steamResouce)
        {
            steamResouce.Occupied = false;
            steamResouce = null;
        }
    }
	
	 public override void GUIPriority(){}
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
