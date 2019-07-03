//---------------------------------------------
//            Network
//---------------------------------------------

using System;
using System.IO;

namespace Net
{
/// <summary>
/// A channel contains one or more players.
/// All information broadcast by players is visible by others in the same channel.
/// </summary>

public class Channel
{


	public class RFC
	{
		// Object ID (24 bytes), RFC ID (8 bytes)
		public uint id;
		public string funcName;
		public Buffer buffer;
	}

	public string level = "";
	public NetList<TcpPlayer> players = new NetList<TcpPlayer>();
	public uint objectCounter = 0xFFFFFF;


	/// <summary>
	/// Reset the channel to its initial state.
	/// </summary>

	public void Reset ()
	{
		objectCounter = 0xFFFFFF;
	}

	/// <summary>
	/// Remove the specified player from the channel.
	/// </summary>

	public void RemovePlayer (TcpPlayer p)
	{

		//if (p == host) host = null;
	    players.Remove(p);
	}

 
}
}
