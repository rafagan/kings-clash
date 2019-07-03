//---------------------------------------------
//            Network
//---------------------------------------------

using System.Net;

namespace Net
{
/// <summary>
/// Simple datagram container -- contains a data buffer and the address of where it came from (or where it's going).
/// </summary>

public struct Datagram
{
	public Buffer Buffer;
	public IPEndPoint Ip;
}
}