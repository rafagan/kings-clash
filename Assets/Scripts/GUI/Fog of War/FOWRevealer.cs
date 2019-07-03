using UnityEngine;
using System.Collections;

/// <summary>
/// Fog of War system needs 3 components in order to work:
/// - Fog of War system that will create a height map of your scene and perform all the updates.
/// - Fog of War Image Effect on the camera that will be displaying the fog of war.
/// - Fog of War Revealer on one or more game objects in the world (this class).
/// </summary>

[AddComponentMenu("Fog of War/Revealer")]
public class FOWRevealer : AbstractUnitComponent
{
	//public BaseUnit OwnerBaseUnit;
	/// <summary>
	/// Radius of the area being revealed. Everything below X is always revealed. Everything up to Y may or may not be revealed.
	/// </summary>

	private Vector2 range = new Vector2(2f, 5f);
	
	public bool isActive = false;
	
	public float defaultUnitRange = 5.0f;
	/// <summary>
	/// What kind of line of sight checks will be performed.
	/// - "None" means no line of sight checks, and the entire area covered by radius.y will be revealed.
	/// - "OnlyOnce" means the line of sight check will be executed only once, and the result will be cached.
	/// - "EveryUpdate" means the line of sight check will be performed every update. Good for moving objects.
	/// </summary>

	public FOWSystem.LOSChecks lineOfSightCheck = FOWSystem.LOSChecks.None;

	/// <summary>
	/// Whether the revealer is actually active or not. If you wanted additional checks such as "is the unit dead?",
	/// then simply derive from this class and change the "isActive" value accordingly.
	/// </summary>

	public bool heightLosMultiplier = false;
	
	[Range(1,40)]
	public int multiplierPercentage = 10;

    

	private FOWSystem.Revealer mRevealer;
	
	void Start()
	{
// 		if (baseUnit == null)
// 			baseUnit = OwnerBaseUnit;
		StartCoroutine("CheckIsEnemy");
	}
	
	IEnumerator CheckIsEnemy() {
		yield return new WaitForSeconds(0.5f);
		
		if (!baseUnit.IsEnemy)
            this.isActive = true; 
		else
			this.isActive = false;
	}

	void Awake ()
	{
		mRevealer = FOWSystem.CreateRevealer();
		
	}

	void OnDisable ()
	{
		mRevealer.isActive = false;
	}

	void OnDestroy ()
	{
		FOWSystem.DeleteRevealer(mRevealer);
		mRevealer = null;
	}
	
	void LateUpdate () {
		defaultUnitRange = baseUnit.GetUnitComponent<AttributesComponent>().FieldOfView;
        if (isActive) {
            if (lineOfSightCheck != FOWSystem.LOSChecks.OnlyOnce)
                mRevealer.cachedBuffer = null;

            mRevealer.pos = transform.parent.position;
            mRevealer.inner = range.x;
            mRevealer.outer = RangeValue();
            mRevealer.los = lineOfSightCheck;
            mRevealer.isActive = true;
            
        } else {
            mRevealer.isActive = false;
            mRevealer.cachedBuffer = null;
        }
	}

    private float RangeValue()
    {
        float r;

        if (heightLosMultiplier && transform.parent.position.y > 1)	
		{
			int intPart = (int)transform.parent.position.y;
		 	r = defaultUnitRange + (defaultUnitRange * ((float)intPart * multiplierPercentage/100));
		}
		else
			r = defaultUnitRange;
		
        return r;
    }

	void OnDrawGizmosSelected ()
	{
		if (lineOfSightCheck != FOWSystem.LOSChecks.None && range.x > 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, range.x);
		}
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(transform.position, defaultUnitRange);
	}

	/// <summary>
	/// Want to force-rebuild the cached buffer? Just call this function.
	/// </summary>

	public void Rebuild () { mRevealer.cachedBuffer = null; }

	#region implemented abstract members of AbstractUnitComponent
	public override void GUIPriority()
	{
	}
	public override void UserInputPriority()
	{
	}
	public override void Reset() {}
	#endregion
}