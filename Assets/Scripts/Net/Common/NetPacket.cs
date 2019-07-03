//---------------------------------------------
//            Network
//---------------------------------------------

namespace Net
{
/// <summary>
/// Clients send requests to the server and receive responses back. Forwarded calls arrive as-is.
/// </summary>

public enum Packet
{
	/// <summary>
	/// Empty packet. Can be used to keep the connection alive.
	/// </summary>

	Empty,

	/// <summary>
	/// This packet indicates that an error has occurred.
	/// string: Description of the error.
	/// </summary>

	Error,

	/// <summary>
	/// This packet indicates that the connection should be severed.
	/// </summary>

	Disconnect,

	//===================================================================================

	/// <summary>
	/// This should be the very first packet sent by the client.
	/// int32: Protocol version.
	/// string: Player Name.
	/// </summary>

	RequestID,

	/// <summary>
	/// Envia para o servidor uma requisicao para os jogadores sincronizarem
	/// </summary>

    RequestAllSynchronize,

	/// <summary>
	/// Clients should send a 
    /// request periodically.
	/// </summary>

    RequestPing,
    
    /// <summary>
    /// Informa os outros jogadores que o turno acabou
    /// </summary>

    RequestSetTurnEnded,

    /// <summary>
    /// Clients should send a ping info to other players.
    /// </summary>

    RequestSetPing,

	/// <summary>
	/// Set the remote UDP port for unreliable packets.
	/// ushort: port.
	/// </summary>

	RequestSetUDP,

	/// <summary>
	/// Join the specified channel.
	/// int32: Channel ID
	/// </summary>

	RequestJoinChannel,

	/// <summary>
	/// Leave the channel the player is in.
	/// </summary>

	RequestLeaveChannel,

	/// <summary>
	/// Load the specified level.
	/// string: Level Name.
	/// </summary>

    RequestLoadLevel,

    /// <summary>
    /// Requisicao de troca do role do jogador
    /// int32: Player role.
    /// </summary>

    RequestSetRole,

    /// <summary>
    /// Requisicao de troca do house do jogador
    /// int32: Player house.
    /// </summary>

    RequestSetHouse,

    /// <summary>
    /// Requisicao de confirmacao de status pronto do jogador
    /// bool: Player ready status.
    /// </summary>

    RequestSetReady,	

    /// <summary>
    /// Requisicao de confirmacao de status sincronizado do jogador
    /// bool: Player ready status.
    /// </summary>

    RequestSetSynchronized,	
    
	/// <summary>
	/// Player name change.
	/// string: Player name.
	/// </summary>

    RequestSetName,	
    
    /// <summary>
    /// Player team change
    /// int32: Player team.
    /// </summary>

    RequestSetTeam,

	/// <summary>
	/// Improve latency of the established connection at the expense of network traffic.
	/// bool: Whether to improve it (enable NO_DELAY)
	/// </summary>

	RequestNoDelay,

	//===================================================================================

	/// <summary>
	/// Always the first packet to arrive from the server.
	/// If the protocol version didn't match the client, a disconnect may follow.
	/// int32: Protocol ID.
	/// int32: Player ID.
	/// </summary>

	ResponseID,

	/// <summary>
	/// Response to a ping request.
	/// </summary>

	ResponsePing,

	/// <summary>
	/// Set a UDP port used for communication.
	/// ushort: port. (0 means disabled)
	/// </summary>

	ResponseSetUDP,

	/// <summary>
	/// Inform everyone of this player leaving the channel.
	/// int32: Player ID.
	/// </summary>

	ResponsePlayerLeft,

	/// <summary>
	/// Inform the channel that a new player has joined.
	/// 
	/// Parameters:
	/// int32: Player ID,
	/// string: Player name.
	/// </summary>

	ResponsePlayerJoined,

	/// <summary>
	/// Start of the channel joining process. Sent to the player who is joining the channel.
	/// 
	/// Parameters:
    /// int32: Channel ID,
    /// int32: Player ID,
    /// string: Player Name.
    /// int32: Team ID,
	/// int16: Number of players.
	/// 
	/// Then for each player:
	/// int32: Player ID,
    /// string: Player Name.
    /// int32: Team ID,
	/// </summary>

	ResponseJoiningChannel,

	/// <summary>
	/// Inform the player that they have successfully joined a channel.
	/// bool: Success or failure.
	/// string: Error string (if failed).
	/// </summary>

	ResponseJoinChannel,

	/// <summary>
	/// Inform the player that they have left the channel they were in.
	/// </summary>

	ResponseLeaveChannel,

    /// <summary>
    /// Troca o role do jogador especificado
    /// int32: Player ID,
    /// int32: Player Role.
    /// </summary>

    ResponseSetRole,

    /// <summary>
    /// Troca o house do jogador especificado
    /// int32: Player ID,
    /// int32: Player House.
    /// </summary>

    ResponseSetHouse,

    /// <summary>
    /// Troca o status ready do jogador especificado
    /// int32: Player ID,
    /// boolean: Player Ready.
    /// </summary>

    ResponseSetReady,	

    /// <summary>
    /// Troca o status sincronizado do jogador especificado
    /// int32: Player ID,
    /// boolean: Player isSynchronized.
    /// </summary>

    ResponseSetSynchronized,	

    /// <summary>
    /// Change the specified player's team
    /// int32: Player ID,
    /// int32: Player team.
    /// </summary>

    ResponseSetTeam,	
    
    /// <summary>
    /// Informa os outros jogadores que o turno do jogador acabou
    /// int32: Player ID.
    /// </summary>

    ResponseSetTurnEnded,

    /// <summary>
    /// Change the specified player's ping.
    /// int32: Player ID,
    /// int32: Player ping.
    /// </summary>

    ResponsePingPlayer,

	/// <summary>
	/// Change the specified player's name.
	/// int32: Player ID,
	/// string: Player name.
	/// </summary>

	ResponseRenamePlayer,
	/// <summary>
	/// Requisicao para os jogadores sincronizarem
	/// </summary>

    ResponseAllSynchronize,

	/// <summary>
	/// Load the specified level. Should happen before all buffered calls.
	/// string: Name of the level.
	/// </summary>

	ResponseLoadLevel,


	/// <summary>
	/// Loaded file response.
	/// string: Filename.
	/// int32: Number of bytes to follow.
	/// byte[]: Data.
	/// </summary>

	ResponseLoadFile,

	/// <summary>
	/// List open channels on the server.
	/// int32: number of channels to follow
	/// For each channel:
	/// int32: ID
	/// string: Level
	/// string: Custom data
	/// </summary>

	ResponseChannelList,

	//===================================================================================

	/// <summary>
	/// Echo the packet to everyone in the room. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToAll,

	/// <summary>
	/// Echo the packet to everyone in the room except the sender. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToOthers,

	/// <summary>
	/// Echo the packet to the room's host. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToHost,

	/// <summary>
	/// Echo the packet to the specified player.
	/// int32: Player ID
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToPlayer,

	//===================================================================================

	/// <summary>
	/// Add a new entry to the list of known servers. Used by the Lobby Server.
	/// ushort: Game ID.
	/// string: Server name.
	/// ushort: Number of connected players.
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	RequestAddServer,

	/// <summary>
	/// Remove an existing server list entry. Used by the Lobby Server.
	/// ushort: Game ID.
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	RequestRemoveServer,

	/// <summary>
	/// Request a list of all known servers for the specified game ID. Used by the Lobby Server.
	/// ushort: Game ID.
	/// </summary>

	RequestServerList,

	/// <summary>
	/// Response sent by the Lobby Server, listing servers.
	/// ushort: List size
	/// For each entry:
	/// string: Server name
	/// ushort: Player count
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	ResponseServerList,
}
}
