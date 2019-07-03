//---------------------------------------------
//            Network
//---------------------------------------------

using System;

namespace Net
{
/// <summary>
/// Attribute used to identify remotely called functions.
/// </summary>

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RFC : Attribute
{
	public byte Id = 0;

	public RFC () { }
	public RFC (byte rid) { Id = rid; }
}
}