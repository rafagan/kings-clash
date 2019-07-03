//---------------------------------------------
//            Network
//---------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;

namespace Net
{
/// <summary>
/// Client-side logic.
/// </summary>

public class GameClient
{
	public delegate void OnError (string message);
	public delegate void OnConnect (bool success, string message);
	public delegate void OnDisconnect ();
	public delegate void OnJoinChannel (bool success, string message);
	public delegate void OnLeftChannel ();
	public delegate void OnLoadLevel (string levelName);
	public delegate void OnPlayerJoined (Player p);
    public delegate void OnPlayerLeft(Player p);
    public delegate void OnRenamePlayer(Player p, string previous);
    public delegate void OnChangedPlayerName(Player p, int previous);
	public delegate void OnSetHost (bool hosting);
	public delegate void OnSetChannelData (string data);
	public delegate void OnForwardedPacket (BinaryReader reader);
	public delegate void OnPacket (Packet response, BinaryReader reader, IPEndPoint source);

	/// <summary>
	/// Error notification.
	/// </summary>

	public OnError onError;

	/// <summary>
	/// Connection attempt result indicating success or failure.
	/// </summary>

	public OnConnect onConnect;

	/// <summary>
	/// Notification sent after the connection terminates for any reason.
	/// </summary>

	public OnDisconnect onDisconnect;

	/// <summary>
	/// Notification sent when attempting to join a channel, indicating a success or failure.
	/// </summary>

	public OnJoinChannel onJoinChannel;

	/// <summary>
	/// Notification sent when leaving a channel.
	/// Also sent just before a disconnect (if inside a channel when it happens).
	/// </summary>

	public OnLeftChannel onLeftChannel;

	/// <summary>
	/// Notification sent when changing levels.
	/// </summary>

	public OnLoadLevel onLoadLevel;

	/// <summary>
	/// Notification sent when a new player joins the channel.
	/// </summary>

	public OnPlayerJoined onPlayerJoined;

	/// <summary>
	/// Notification sent when a player leaves the channel.
	/// </summary>

	public OnPlayerLeft onPlayerLeft;


	/// <summary>
	/// Notification of some player changing their name.
	/// </summary>

    public OnRenamePlayer onRenamePlayer;

    /// <summary>
    /// Notification of some player changing their team.
    /// </summary>

    public OnChangedPlayerName onChangedPlayerTeam;

	/// <summary>
	/// Notification sent when the channel's host changes.
	/// </summary>

	public OnSetHost onSetHost;

	/// <summary>
	/// Notification of the channel's custom data changing.
	/// </summary>

	public OnSetChannelData onSetChannelData;

	/// <summary>
	/// Notification of a client packet arriving.
	/// </summary>

	public OnForwardedPacket onForwardedPacket;

	/// <summary>
	/// List of players in the same channel as the client.
	/// </summary>

    public NetList<Player> players = new NetList<Player>();


	// Same list of players, but in a dictionary format for quick lookup
	Dictionary<int, Player> mDictionary = new Dictionary<int, Player>();

	// TCP connection is the primary method of communication with the server.
	TcpProtocol mTcp = new TcpProtocol();

	// UDP can be used for transmission of frequent packets, network broadcasts and NAT requests.
	// UDP is not available in the Unity web player because using UDP packets makes Unity request the
	// policy file every time the packet gets sent... which is obviously quite retarded.
    UdpProtocol mUdp = new UdpProtocol();

    // Tempo para sincronizar os jogadores
    long mConnectionEstablishedTime = 0;

    // Tempo atual do turno
    long mTurnTime = 0;
    long mTurnConfirmationTime = 0;

	// Current time, time when the last ping was sent out, and time when connection was started
	long mTime = 0;
	long mPingTime = 0;

	// Last ping, and whether we can ping again
	int mPing = 0;
	bool mCanPing = false;
	bool mIsInChannel = false;

	// Server's UDP address
	IPEndPoint mServerUdpEndPoint;

	// Temporary, not important
	static Buffer mBuffer;

	/// <summary>
	/// Whether the client is currently connected to the server.
	/// </summary>

	public bool isConnected { get { return mTcp.isConnected; } }

	/// <summary>
	/// Whether we are currently trying to establish a new connection.
	/// </summary>

	public bool isTryingToConnect { get { return mTcp.isTryingToConnect; } }
  
	/// <summary>
	/// Whether this player is hosting the game.
	/// </summary>

    public bool isHosting { get { return NetServerInstance.isActive; } }

	/// <summary>
	/// Whether this player is hosting the game.
	/// </summary>

    public bool inGame { get { return Application.loadedLevelName.Equals(NetManager.gameScene); } }
    
	/// <summary>
	/// Whether the client is currently in a channel.
	/// </summary>

	public bool isInChannel { get { return mIsInChannel; } }

	/// <summary>
	/// Port used to listen for incoming UDP packets. Set via StartUDP().
	/// </summary>

	public int listeningPort
	{
		get
		{
			return mUdp.listeningPort;
		}
	}

	/// <summary>
	/// Enable or disable the Nagle's buffering algorithm (aka NO_DELAY flag).
	/// Enabling this flag will improve latency at the cost of increased bandwidth.
	/// http://en.wikipedia.org/wiki/Nagle's_algorithm
	/// </summary>

	public bool noDelay
	{
		get
		{
			return mTcp.noDelay;
		}
		set
		{
			if (mTcp.noDelay != value)
			{
				mTcp.noDelay = value;
				
				// Notify the server as well so that the server does the same
				BeginSend(Packet.RequestNoDelay).Write(value);
				EndSend();
			}
		}
	}

	/// <summary>
	/// Current ping to the server.
	/// </summary>

	public int ping { get { return isConnected ? mPing : 0; } }

    /// <summary>
    /// Se todos os jogadores estao sincronizados
    /// </summary>

    public bool allSynchronized
    {
        get
        {
            if (!mTcp.isSynchronized) return false;
            foreach (var p in players)
                if (!p.isSynchronized)
                    return false;
            return true;
        }
    }

    /// <summary>
    /// Se todos os jogadores finalizaram o turno
	/// </summary>

    public bool allTurnsEnded
	{
		get
        {
            if (!mTcp.turnEnded) return false;
            foreach (var p in players)
            {
                if (!p.turnEnded && p.turn != mTcp.turn && p.isSynchronized)
                    return false;
            }
			return true;
		}
	}

	/// <summary>
	/// Whether we can communicate with the server via UDP.
	/// </summary>

	public bool canUseUDP
	{
		get
		{
			return mUdp.isActive && mServerUdpEndPoint != null;
		}
	}

	/// <summary>
	/// Return the local player.
	/// </summary>

	public Player player { get { return mTcp; } }

	/// <summary>
	/// The player's unique identifier.
	/// </summary>

    public int playerID { get { return mTcp.id; } }

    /// <summary>
    /// Turno atual do jogador.
    /// </summary>

    public int playerTurn { get { return mTcp.turn; } }

    /// <summary>
    /// Se o turno atual do jogador finalizou.
    /// </summary>

    public bool playerTurnEnded { get { return mTcp.turnEnded; } }

    /// <summary>
    /// Se o jogador esta pronto para iniciar o jogo
    /// </summary>

    public bool playerSynchronized {
        get {
            return mTcp.isSynchronized;
        }
        set {
            if (mTcp.isSynchronized != value) {
                if (isConnected) {
                    var writer = BeginSend(Packet.RequestSetSynchronized);
                    writer.Write(value);
                    EndSend();
                }
                else mTcp.isSynchronized = value;
            }
        }
    }

    /// <summary>
    /// Se o jogador esta pronto para iniciar o jogo
    /// </summary>

    public bool playerReady
    {
        get
        {
            return mTcp.isReady;
        }
        set
        {
            if (mTcp.isReady != value)
            {
                if (isConnected)
                {
                    var writer = BeginSend(Packet.RequestSetReady);
                    writer.Write(value);
                    EndSend();
                }
                else mTcp.isReady = value;
            }
        }
    }

    /// <summary>
    /// Role atual do jogador.
    /// </summary>

    public int playerRole
    {
        get
        {
            return mTcp.role;
        }
        set
        {
            if (mTcp.role != value)
            {
                if (isConnected)
                {
                    var writer = BeginSend(Packet.RequestSetRole);
                    writer.Write(value);
                    EndSend();
                }
                else mTcp.role = value;
            }
        }
    }

    /// <summary>
    /// House atual do jogador
    /// </summary>

    public int playerHouse
    {
        get
        {
            return mTcp.house;
        }
        set
        {
            if (mTcp.house != value)
            {
                if (isConnected)
                {
                    var writer = BeginSend(Packet.RequestSetHouse);
                    writer.Write(value);
                    EndSend();
                }
                else mTcp.house = value;
            }
        }
    }

	/// <summary>
	/// Name of this player.
	/// </summary>

	public string playerName
	{
		get
		{
			return mTcp.name;
		}
		set
		{
			if (mTcp.name != value)
			{
				if (isConnected)
				{
					var writer = BeginSend(Packet.RequestSetName);
					writer.Write(value);
					EndSend();
				}
				else mTcp.name = value;
			}
		}
	}
    
    /// <summary>
    /// Team of this player.
    /// </summary>

    public int playerTeamID
    {
        get
        {
            return mTcp.teamId;
        }
        set
        {
            if (mTcp.teamId != value)
            {
                if (isConnected)
                {
                    var writer = BeginSend(Packet.RequestSetTeam);
                    writer.Write(value);
                    EndSend();
                }
                else mTcp.teamId = value;
            }
        }
    }

	/// <summary>
	/// Retrieve a player by their ID.
	/// </summary>

	public Player GetPlayer (int id)
	{
		if (id == mTcp.id) return mTcp;

		if (isConnected)
		{
			Player player = null;
			mDictionary.TryGetValue(id, out player);
			return player;
		}
		return null;
	}

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	public BinaryWriter BeginSend (Packet type)
	{
		mBuffer = Buffer.Create();
		return mBuffer.BeginPacket(type);
	}

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	public BinaryWriter BeginSend (byte packetID)
	{
		mBuffer = Buffer.Create();
		return mBuffer.BeginPacket(packetID);
	}

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	public void EndSend ()
	{
		mBuffer.EndPacket();
		mTcp.SendTcpPacket(mBuffer);
		mBuffer.Recycle();
		mBuffer = null;
	}
    

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	public void EndSend (bool reliable)
	{
		mBuffer.EndPacket();
		if (reliable || mServerUdpEndPoint == null || !mUdp.isActive)
		{
			mTcp.SendTcpPacket(mBuffer);
			mBuffer.Recycle();
		}
		else
		{
			mBuffer.EndPacket();
			mUdp.Send(mBuffer, mServerUdpEndPoint);
		}
		mBuffer = null;
	}

	/// <summary>
	/// Broadcast the outgoing buffer to the entire LAN via UDP.
	/// </summary>

	public void EndSend (int port)
	{
		mBuffer.EndPacket();
		mUdp.Broadcast(mBuffer, port);
		mBuffer = null;
	}

	/// <summary>
	/// Send this packet to a remote UDP listener.
	/// </summary>

	public void EndSend (IPEndPoint target)
	{
		mBuffer.EndPacket();
		mUdp.Send(mBuffer, target);
		mBuffer = null;
	}

	/// <summary>
	/// Try to establish a connection with the specified address.
	/// </summary>

	public void Connect (IPEndPoint externalIP, IPEndPoint internalIP)
	{
		Disconnect();
		mTcp.Connect(externalIP, internalIP);
	}	
  

	/// <summary>
	/// Disconnect from the server.
	/// </summary>

	public void Disconnect () {
        mTcp.Disconnect(); 
    }

	/// <summary>
	/// Start listening to incoming UDP packets on the specified port.
	/// </summary>

	public bool StartUDP (int udpPort)
	{
		if (mUdp.Start(udpPort))
		{
			if (isConnected)
			{
				BeginSend(Packet.RequestSetUDP).Write((ushort)udpPort);
				EndSend();
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Stop listening to incoming broadcasts.
	/// </summary>

	public void StopUDP ()
	{
		if (mUdp.isActive)
		{
			if (isConnected)
			{
				BeginSend(Packet.RequestSetUDP).Write((ushort)0);
				EndSend();
			}
			mUdp.Stop();
		}
	}

	/// <summary>
	/// Join the specified channel.
	/// </summary>
	/// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
	/// <param name="levelName">Level that will be loaded first.</param>

	private void JoinChannel (string levelName)
	{
		if (isConnected)
		{
			var writer = BeginSend(Packet.RequestJoinChannel);
			writer.Write(string.IsNullOrEmpty(levelName) ? "" : levelName);
			EndSend();
		}
	}


	/// <summary>
	/// Troca a cena atual.
	/// </summary>

	public void LoadLevel (string levelName)
	{

		if (isConnected && isInChannel && isHosting)
		{
			BeginSend(Packet.RequestLoadLevel).Write(levelName);
			EndSend();
		}
	}


	/// <summary>
	/// Processa todos os pacotes que chegam do servidor.
	/// </summary>

	public void ProcessPackets ()
	{
		mTime = DateTime.Now.Ticks / 10000;


        // Envia requests de ping de vez em quando, para avisar o servidor que ainda esta conectado
		if (mTcp.isConnected && mCanPing && mPingTime + 4000 < mTime)
		{
			mCanPing = false;
			mPingTime = mTime;
			BeginSend(Packet.RequestPing);
            EndSend();
            // Informa o ultimo ping para os outros jogadores
            var writer = BeginSend(Packet.RequestSetPing);
            writer.Write(mPing);
            EndSend();
		}

        // Logica dos turnos
        if(mTcp.isSynchronized) {

            if (!mTcp.turnEnded && ((mTime - mTurnTime) > NetManager.timeEachTurn)) {
                // Finaliza o turno atual e informa os outros jogadores
                mTcp.turnEnded = true;
                // Guarda o tempo atual para procedimento de resincronizacao
                mTurnConfirmationTime = mTime;
                // Avisa os outros jogadores qual o turno finalizado
                var writer = BeginSend(Packet.RequestSetTurnEnded);
                writer.Write(mTcp.turn);
                EndSend();
                // Após passar por aqui aguarda confirmacao de finalizacao do mesmo turno dos outros jogadores
            }

            
            if (mTcp.turnEnded && ((mTime - mTurnConfirmationTime) > 2000)) { // 2000 ms espera para reenviar confirmacao de finalizacao do turno
                mTurnConfirmationTime = mTime;
                foreach (var p in players) {
                    // Se estiver atrasado em relacao a qualquer outro jogador entao adianta para sincronizar
                    if (p.turn > mTcp.turn) {
                        mTcp.turn = p.turn;
                        // Avisa os outros jogadores que esta sincronizado com o novo turno
                        var writer = BeginSend(Packet.RequestSetTurnEnded);
                        writer.Write(mTcp.turn);
                        EndSend();
                    }
                }
            }

            // Se todos os jogadores finalizarem o turno atual, executa os comandos gravados e avança para o proximo
            if (allTurnsEnded) {
                // Reseta o status de todos os jogadores
                foreach (var p in players) {
                    p.turnEnded = false;
                }
                mTcp.turnEnded = false;
                // Processa os mails gravados
                MailMan.ProcessRecordedMails();
                // Avanca o turno
                mTcp.turn++;
                // Guarda o tempo atual
                mTurnTime = mTime;
            }

        } else {
            // Se o tempo de sincronizacao acabou envia uma requisicao de sincronizacao para todos os clients
            if (inGame && isHosting) {
                if ((mConnectionEstablishedTime + (double)(NetManager.syncronizeTime * 1000.0f)) <= mTime) {
                    BeginSend(Packet.RequestAllSynchronize);
                    EndSend();
                }
            }
        }


		Buffer buffer = null;
		var keepGoing = true;

		IPEndPoint ip = null;

		while (keepGoing && mUdp.ReceivePacket(out buffer, out ip))
		{
			keepGoing = ProcessPacket(buffer, ip);
			buffer.Recycle();
		}
		while (keepGoing && mTcp.ReceivePacket(out buffer))
		{
			keepGoing = ProcessPacket(buffer, null);
			buffer.Recycle();
		}
	}

	/// <summary>
	/// Process a single incoming packet. Returns whether we should keep processing packets or not.
	/// </summary>

	bool ProcessPacket (Buffer buffer, IPEndPoint ip)
	{
		var reader = buffer.BeginReading();
		if (buffer.size == 0) return true;

		int packetID = reader.ReadByte();
		var response = (Packet)packetID;

		// Verification step must be passed first
		if (mTcp.stage == TcpProtocol.Stage.Verifying)
		{
			if (mTcp.VerifyResponseID(response, reader))
			{
				if (mUdp.isActive)
				{
					// If we have a UDP listener active, tell the server
					BeginSend(Packet.RequestSetUDP).Write(mUdp.isActive ? (ushort)mUdp.listeningPort : (ushort)0);
					EndSend();
				}
                mCanPing = true;
                JoinChannel(NetManager.menuScene);
				if (onConnect != null) onConnect(true, null);
			}
			else if (onConnect != null)
			{
				onConnect(false, "Protocol version mismatch!");
			}
			return true;
		}

		switch (response)
		{
			case Packet.Empty: break;
			case Packet.ForwardToAll:
			case Packet.ForwardToOthers:
			{
				if (onForwardedPacket != null) onForwardedPacket(reader);
				break;
			}
			case Packet.ForwardToPlayer:
			{
				// Skip the player ID
				reader.ReadInt32();
				if (onForwardedPacket != null) onForwardedPacket(reader);
				break;
			}
			case Packet.ResponsePing:
			{
				mPing = (int)(mTime - mPingTime);
				mCanPing = true;
				break;
			}
			case Packet.ResponseSetUDP:
			{
				// The server has a new port for UDP traffic
				var port = reader.ReadUInt16();

				if (port != 0)
				{
					var ipa = new IPAddress(mTcp.tcpEndPoint.Address.GetAddressBytes());
					mServerUdpEndPoint = new IPEndPoint(ipa, port);
                    // Envia um pacote vazio para o servidor, para abrir o canal de comunicacao
					if (mUdp.isActive) mUdp.SendEmptyPacket(mServerUdpEndPoint);
				}
				else mServerUdpEndPoint = null;
				break;
			}
			case Packet.ResponseJoiningChannel:
			{
				mIsInChannel = true;
				mDictionary.Clear();
				players.Clear();

                player.id = reader.ReadInt32();
                player.name = reader.ReadString();
                player.teamId = reader.ReadInt32();
                player.role = reader.ReadInt32();

				int count = reader.ReadInt16();

				for (var i = 0; i < count; ++i)
				{
					var p = new Player {
					    id = reader.ReadInt32(),
					    name = reader.ReadString(),
					    teamId = reader.ReadInt32(),
					    role = reader.ReadInt32(),
					    house = reader.ReadInt32(),
					    isReady = reader.ReadBoolean()
					};
				    mDictionary.Add(p.id, p);
                    players.Add(p);
				}
				break;
			}
			case Packet.ResponseLoadLevel: {
                var levelName = reader.ReadString();

                if (levelName.Equals(NetManager.gameScene)) {
                    mConnectionEstablishedTime = mTime;
                }
				// Purposely return after loading a level, ensuring that all future callbacks happen after loading
				if (onLoadLevel != null) onLoadLevel(levelName);
				return false;
			}
			case Packet.ResponsePlayerLeft:
			{
				var p = GetPlayer(reader.ReadInt32());
				if (p != null) mDictionary.Remove(p.id);
				players.Remove(p);

                if (p.teamId == playerTeamID) {
                    if(p.role == 2) { // se o jogador que deixou é archmage
                        if (playerRole == 2) // se eu sou warleader
                            playerRole = 1; // entao eu viro apenas warleader/archmage
                    }
                    if (p.role == 1) { // se o jogador que deixou é warleader/archmage
                        if (playerRole == 4) // se eu sou monarch
                            playerRole = 0; // entao eu viro apenas monarch/warleader/archmage
                    }
                }
				if (onPlayerLeft != null) onPlayerLeft(p);
				break;
			}
			case Packet.ResponsePlayerJoined:
			{
				var p = new Player {
				    id = reader.ReadInt32(),
				    name = reader.ReadString(),
				    teamId = reader.ReadInt32(),
				    house = reader.ReadInt32(),
				    role = reader.ReadInt32()
				};
			    mDictionary.Add(p.id, p);
				players.Add(p);
                if(p.teamId == playerTeamID) {

                    switch (playerRole) {
                        case 0: // se eu sou all
                            playerRole = 4; // entao eu viro apenas monarch
                            break;
                        case 1: // se eu sou arch/war
                            playerRole = 5; // entao eu viro apenas warleader
                            break;
                    }
                }

				if (onPlayerJoined != null) onPlayerJoined(p);
				break;
			}
			case Packet.ResponseJoinChannel:
			{
				mIsInChannel = reader.ReadBoolean();
				if (onJoinChannel != null) onJoinChannel(mIsInChannel, mIsInChannel ? null : reader.ReadString());
				break;
			}
			case Packet.ResponseLeaveChannel:
			{
				mIsInChannel = false;
				mDictionary.Clear();
				players.Clear();
				if (onLeftChannel != null) onLeftChannel();
				break;
            }
            case Packet.ResponseSetRole:
            {
                var p = GetPlayer(reader.ReadInt32());
                if (p != null) p.role = reader.ReadInt32();
                break;
            }
            case Packet.ResponseSetHouse:
            {
                var p = GetPlayer(reader.ReadInt32());
                if (p != null) p.house = reader.ReadInt32();
                break;
            }
            case Packet.ResponseSetReady:
            {
                var p = GetPlayer(reader.ReadInt32());
                if (p != null) p.isReady = reader.ReadBoolean();
                break;
            }
            case Packet.ResponseSetSynchronized:
            {
                var p = GetPlayer(reader.ReadInt32());
                if (p != null) p.isSynchronized = reader.ReadBoolean();
                break;
            }
            case Packet.ResponseSetTeam:
            {
                var p = GetPlayer(reader.ReadInt32());
                var oldTeam = p.teamId;
                if (p != null) p.teamId = reader.ReadInt32();
                if (onChangedPlayerTeam != null) onChangedPlayerTeam(p, oldTeam);
                break;
            }
            case Packet.ResponseSetTurnEnded:
            {
                var p = GetPlayer(reader.ReadInt32());
                if (p != null) {
                    p.turn = reader.ReadInt32();
                    p.turnEnded = true;
                }
                break;
            }
            case Packet.ResponsePingPlayer:
			{
				var p = GetPlayer(reader.ReadInt32());
				if (p != null) p.ping = reader.ReadInt32();
				break;
			}
			case Packet.ResponseRenamePlayer:
			{
				var p = GetPlayer(reader.ReadInt32());
				var oldName = p.name;
				if (p != null) p.name = reader.ReadString();
				if (onRenamePlayer != null) onRenamePlayer(p, oldName);
				break;
			}

            case Packet.ResponseAllSynchronize:
            {
                
                // Finalizacao da sincronizacao, aonde luiz terá que pegar as informacoes para iniciar os team managers
                PlayerManager.Player.PlayerTeam = NetManager.player.teamId;

                if (MailMan.Post != null)
                    MailMan.Post.Reset();
                mTurnTime = mTime;

                BeginSend(Packet.RequestSetSynchronized);
                EndSend();
				break;
			}
			case Packet.Error:
			{
				if (mTcp.stage != TcpProtocol.Stage.Connected && onConnect != null)
				{
					onConnect(false, reader.ReadString());
				}
				else if (onError != null)
				{
					onError(reader.ReadString());
				}
				break;
			}
			case Packet.Disconnect:
			{
				if (isInChannel && onLeftChannel != null) onLeftChannel();
				players.Clear();
				mDictionary.Clear();
				mTcp.Close(false);
				if (onDisconnect != null) onDisconnect();
				break;
			}
		}
		return true;
	}

}
}
