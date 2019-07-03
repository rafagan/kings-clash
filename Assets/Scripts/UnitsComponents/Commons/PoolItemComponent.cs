using UnityEngine;

public class PoolItemComponent : AbstractUnitComponent {

	public Pool MyPoolManager;
	public int PoolItemID;
	
	private bool _inUse = false;
	
	public void Start () {
		baseUnit = transform.GetComponent<BaseUnit>();
		if (baseUnit == null) {
			Debug.Log("BaseUnit do PoolItem não encontrado!!!");
		}
	}
	
	#region PROPERTIES
	public bool InUse { get { return _inUse; } set { _inUse = value; gameObject.SetActive(_inUse); } }
	public BaseUnit OwnerUnit { get { return baseUnit; } }
	#endregion
	
	 public override void GUIPriority() {
	 
	 }
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
