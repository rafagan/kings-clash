//---------------------------------------------
//            Network
//---------------------------------------------

namespace Net
{
/// <summary>
/// Helper enum -- the entries should be in the same order as in the Packet enum.
/// </summary>

public enum Target
{
	/// <summary>
	/// Echo the packet to everyone in the room.
	/// </summary>

	All,

	/// <summary>
	/// Echo the packet to everyone in the room except the sender.
	/// </summary>

	Others,

	/// <summary>
	/// Echo the packet to the room's host.
	/// </summary>

	Host,
}
}