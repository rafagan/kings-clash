//---------------------------------------------
//            Network
//---------------------------------------------

using UnityEngine;

/// <summary>
/// If your MonoBehaviour will need to use a NetObject, deriving from this class will make it easier.
/// </summary>

[RequireComponent(typeof(NetObject))]
public abstract class NetBehaviour : MonoBehaviour
{
	NetObject _mNetO;

	public NetObject Package
	{
		get { return _mNetO ?? (_mNetO = GetComponent<NetObject>()); }
	}
}