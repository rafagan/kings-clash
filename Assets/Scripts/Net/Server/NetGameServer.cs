//---------------------------------------------
//             Network
//---------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using UnityEngine;

namespace Net
{
    /// <summary>
    /// Game server logic. Handles new connections, RFCs, and pretty much everything else. Example usage:
    /// GameServer gs = new GameServer();
    /// gs.Start(5127);
    /// </summary>

    public class GameServer
    {
        /// <summary>
        /// You will want to make this a unique value.
        /// </summary>

        public const ushort gameID = 1;

        public delegate void OnCustomPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request, bool reliable);
        public delegate void OnPlayerAction(Player p);
        public delegate void OnShutdown();

        /// <summary>
        /// Any packet not already handled by the server will go to this function for processing.
        /// </summary>

        public OnCustomPacket onCustomPacket;

        /// <summary>
        /// Notification triggered when a player connects and authenticates successfully.
        /// </summary>

        public OnPlayerAction onPlayerConnect;

        /// <summary>
        /// Notification triggered when a player disconnects.
        /// </summary>

        public OnPlayerAction onPlayerDisconnect;

        /// <summary>
        /// Notification triggered when the server shuts down.
        /// </summary>

        public OnShutdown onShutdown;

        /// <summary>
        /// Give your server a name.
        /// </summary>

        public string name = "Game Server";       
        
        /// <summary>
        /// Give your server a password.
        /// </summary>

        public string password = "";

        /// <summary>
        /// The server player host name.
        /// </summary>

        public string host = "";       

        /// <summary>
        /// Give your server a player limit.
        /// </summary>

        public int playerLimit = 65535;

        /// <summary>
        /// Give your server a join in game option.
        /// </summary>
        public bool joinInGame = true;


        /// <summary>
        /// Se o servidor está pronto para iniciar os turnos
        /// </summary>
        public bool readyToStartTurns = false;

        /// <summary>
        /// Lobby server link, if one is desired.
        /// You can use this to automatically inform a remote lobby server of any changes to this server.
        /// </summary>

        public LobbyServerLink lobbyLink;

        /// <summary>
        /// List of players in a consecutive order for each looping.
        /// </summary>

        NetList<TcpPlayer> mPlayers = new NetList<TcpPlayer>();

        /// <summary>
        /// Dictionary list of players for easy access by ID.
        /// </summary>

        Dictionary<int, TcpPlayer> mDictionaryID = new Dictionary<int, TcpPlayer>();

        /// <summary>
        /// Dictionary list of players for easy access by IPEndPoint.
        /// </summary>

        Dictionary<IPEndPoint, TcpPlayer> mDictionaryEP = new Dictionary<IPEndPoint, TcpPlayer>();

        /// <summary>
        /// Active Channel
        /// </summary>
        /// 
        Channel mChannel = new Channel();

        Buffer mBuffer;
        TcpListener mListener;
        Thread mThread;
        int mListenerPort = 0;
        long mTime = 0;
        UdpProtocol mUdp = new UdpProtocol();
        bool mAllowUdp = false;

        /// <summary>
        /// Whether the server is currently actively serving players.
        /// </summary>

        public bool isActive { get { return mThread != null; } }

        /// <summary>
        /// Whether the server is listening for incoming connections.
        /// </summary>

        public bool isListening { get { return (mListener != null); } }

        /// <summary>
        /// Whether the server is listening for incoming connections.
        /// </summary>

        public bool isPassProtected { get { return (!string.IsNullOrEmpty(password)); } }

        /// <summary>
        /// Port used for listening to incoming connections. Set when the server is started.
        /// </summary>

        public int tcpPort { get { return (mListener != null) ? mListenerPort : 0; } }

        /// <summary>
        /// Listening port for UDP packets.
        /// </summary>

        public int udpPort { get { return mUdp.listeningPort; } }

        /// <summary>
        /// How many players are currently connected to the server.
        /// </summary>

        public int playerCount { get { return isActive ? mPlayers.size : 0; } }

        /// <summary>
        /// Start listening to incoming connections on the specified port.
        /// </summary>

        public bool Start(int tcpPort) { return Start(tcpPort, 0); }

        /// <summary>
        /// Start listening to incoming connections on the specified port.
        /// </summary>

        public bool Start(int tcpPort, int udpPort)
        {
            Stop();

            try
            {
                mListenerPort = tcpPort;
                mListener = new TcpListener(IPAddress.Any, tcpPort);
                mListener.Start(50);
                //mListener.BeginAcceptSocket(OnAccept, null);
            }
            catch (System.Exception ex)
            {
                Error(ex.Message);
                return false;
            }

		    Debug.Log("Game server started on port " + tcpPort);
            if (!mUdp.Start(udpPort))
            {
                Error("Unable to listen to UDP port " + udpPort);
                Stop();
                return false;
            }

            mAllowUdp = (udpPort > 0);

            if (lobbyLink != null)
            {
                lobbyLink.Start();
                lobbyLink.SendUpdate(this);
            }

            mThread = new Thread(ThreadFunction);
            mThread.Start();
            return true;
        }

        /// <summary>
        /// Accept socket callback.
        /// </summary>

        //void OnAccept (IAsyncResult result) { AddPlayer(mListener.EndAcceptSocket(result)); }

        /// <summary>
        /// Stop listening to incoming connections and disconnect all players.
        /// </summary>

        public void Stop()
        {
            if (lobbyLink != null) lobbyLink.Stop();

            mAllowUdp = false;

            // Stop the worker thread
            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }

            // Stop listening
            if (mListener != null)
            {
                mListener.Stop();
                mListener = null;
            }
            mUdp.Stop();

            // Remove all connected players and clear the list of channels
            for (var i = mPlayers.size; i > 0; ) RemovePlayer(mPlayers[--i]);
        }

        /// <summary>
        /// Stop listening to incoming connections but keep the server running.
        /// </summary>

        public void MakePrivate() { mListenerPort = 0; }

        /// <summary>
        /// Thread that will be processing incoming data.
        /// </summary>

        void ThreadFunction()
        {
            for (; ; )
            {
                Buffer buffer;
                var received = false;
                mTime = DateTime.Now.Ticks / 10000;
                IPEndPoint ip;

                // Stop the listener if the port is 0 (MakePrivate() was called)
                if (mListenerPort == 0)
                {
                    if (mListener != null)
                    {
                        mListener.Stop();
                        mListener = null;
                        if (lobbyLink != null) lobbyLink.Stop();
                        if (onShutdown != null) onShutdown();
                    }
                }
                else
                {
                    // Add all pending connections
                    while (mListener != null && mListener.Pending())
                    {
                        var p = AddPlayer(mListener.AcceptSocket());
                        Debug.Log("The address '" + p.address + "' has been connected");
                    }
                }

                // Process datagrams first
                while (mUdp.listeningPort != 0 && mUdp.ReceivePacket(out buffer, out ip))
                {
                    if (buffer.size > 0)
                    {
                        var player = GetPlayer(ip);

                        if (player != null)
                        {
                            try
                            {
                                if (ProcessPlayerPacket(buffer, player, false))
                                    received = true;
                            }
                            catch (System.Exception) { RemovePlayer(player); }
                        }
                    }
                    buffer.Recycle();
                }

                // Process player connections next
                for (var i = 0; i < mPlayers.size; )
                {
                    var player = mPlayers[i];

                    // Process up to 100 packets at a time
                    for (var b = 0; b < 100 && player.ReceivePacket(out buffer); ++b)
                    {
                        if (buffer.size > 0)
                        {
                            try
                            {
                                if (ProcessPlayerPacket(buffer, player, true))
                                    received = true;
                            }
                            catch (System.Exception ex) { 
                                Debug.LogError("ERROR (ProcessPlayerPacket): " + ex.Message);
                                RemovePlayer(player); 
                            }
                        }
                        buffer.Recycle();
                    }

                    // Time out -- disconnect this player
                    if (player.stage == TcpProtocol.Stage.Connected)
                    {
                        // Up to 10 seconds can go without a single packet before the player is removed
                        if (player.lastReceivedTime + 10000 < mTime)
                        {
                            Debug.Log("The address '" + player.address + "' has timed out");
                            RemovePlayer(player);
                            continue;
                        }
                    }
                    else if (player.lastReceivedTime + 2000 < mTime)
                    {
                        Debug.Log("The address '" + player.address + "' has timed out");
                        RemovePlayer(player);
                        continue;
                    }
                    ++i;
                }
                if (!received) Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Log an error message.
        /// </summary>

        void Error(string error) {
            Error(null, error);
        }

        /// <summary>
        /// Log an error message.
        /// </summary>

        void Error(TcpPlayer p, string error) {
#if UNITY_EDITOR
            if (p != null) UnityEngine.Debug.LogError(error + " (" + p.address + ")");
            else UnityEngine.Debug.LogError(error);
#elif STANDALONE
            if (p != null) 
		        Console.WriteLine(error + " (" + p.address + ")");
            else 
		        Console.WriteLine("ERROR: " + error);
#endif
        }

        /// <summary>
        /// Add a new player entry.
        /// </summary>

        TcpPlayer AddPlayer(Socket socket)
        {
            var player = new TcpPlayer();
            player.StartReceiving(socket);
            mPlayers.Add(player);
            return player;
        }

        /// <summary>
        /// Remove the specified player.
        /// </summary>

        void RemovePlayer(TcpPlayer p)
        {
            if (p != null)
            {
                SendLeaveChannel(p, false);
                Debug.Log("The address '" + p.address + "' has been disconnected");
                p.Release();
                mPlayers.Remove(p);

                if (p.udpEndPoint != null)
                {
                    mDictionaryEP.Remove(p.udpEndPoint);
                    p.udpEndPoint = null;
                }

                if (p.id != 0)
                {
                    if (mDictionaryID.Remove(p.id))
                    {
                        if (lobbyLink != null) lobbyLink.SendUpdate(this);
                        if (onPlayerDisconnect != null) onPlayerDisconnect(p);
                    }
                    p.id = 0;
                }
            }
        }

        /// <summary>
        /// Retrieve a player by their ID.
        /// </summary>

        TcpPlayer GetPlayer(int id)
        {
            TcpPlayer p = null;
            mDictionaryID.TryGetValue(id, out p);
            return p;
        }

        /// <summary>
        /// Retrieve a player by their UDP end point.
        /// </summary>

        TcpPlayer GetPlayer(IPEndPoint ip)
        {
            TcpPlayer p = null;
            mDictionaryEP.TryGetValue(ip, out p);
            return p;
        }

        /// <summary>
        /// Change the player's UDP end point and update the local dictionary.
        /// </summary>

        void SetPlayerUdpEndPoint(TcpPlayer player, IPEndPoint udp)
        {
            if (player.udpEndPoint != null) mDictionaryEP.Remove(player.udpEndPoint);
            player.udpEndPoint = udp;
            if (udp != null) mDictionaryEP[udp] = player;
        }

        /// <summary>
        /// Start the sending process.
        /// </summary>

        BinaryWriter BeginSend(Packet type)
        {
            mBuffer = Buffer.Create();
            var writer = mBuffer.BeginPacket(type);
            return writer;
        }

        /// <summary>
        /// Send the outgoing buffer to the specified remote destination.
        /// </summary>

        void EndSend(IPEndPoint ip)
        {
            mBuffer.EndPacket();
            mUdp.Send(mBuffer, ip);
            mBuffer.Recycle();
            mBuffer = null;
        }

        /// <summary>
        /// Send the outgoing buffer to the specified player.
        /// </summary>

        void EndSend(bool reliable, TcpPlayer player)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024) reliable = true;

            if (reliable || player.udpEndPoint == null || !mAllowUdp)
            {
                player.SendTcpPacket(mBuffer);
            }
            else mUdp.Send(mBuffer, player.udpEndPoint);

            mBuffer.Recycle();
            mBuffer = null;
        }

        /// <summary>
        /// Send the outgoing buffer to all players in the specified channel.
        /// </summary>

        void EndSend(bool reliable, Channel channel, TcpPlayer exclude)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024) reliable = true;

            for (var i = 0; i < channel.players.size; ++i)
            {
                var player = channel.players[i];

                if (player.stage == TcpProtocol.Stage.Connected && player != exclude)
                {
                    if (reliable || player.udpEndPoint == null || !mAllowUdp)
                    {
                        player.SendTcpPacket(mBuffer);
                    }
                    else mUdp.Send(mBuffer, player.udpEndPoint);
                }
            }

            mBuffer.Recycle();
            mBuffer = null;
        }

        /// <summary>
        /// Send the outgoing buffer to all connected players.
        /// </summary>

        void EndSend(bool reliable)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024) reliable = true;

            for (var b = 0; b < mChannel.players.size; ++b)
            {
                var player = mChannel.players[b];

                if (player.stage == TcpProtocol.Stage.Connected)
                {
                    if (reliable || player.udpEndPoint == null || !mAllowUdp)
                    {
                        player.SendTcpPacket(mBuffer);
                    }
                    else mUdp.Send(mBuffer, player.udpEndPoint);
                }
            }
            mBuffer.Recycle();
            mBuffer = null;
        }

        /// <summary>
        /// Send the outgoing buffer to all players in the specified channel.
        /// </summary>

        void SendToChannel(bool reliable, Channel channel, Buffer buffer)
        {
            mBuffer.MarkAsUsed();
            if (mBuffer.size > 1024) reliable = true;

            for (var i = 0; i < channel.players.size; ++i)
            {
                var player = channel.players[i];

                if (player.stage == TcpProtocol.Stage.Connected)
                {
                    if (reliable || player.udpEndPoint == null || !mAllowUdp)
                    {
                        player.SendTcpPacket(mBuffer);
                    }
                    else mUdp.Send(mBuffer, player.udpEndPoint);
                }
            }
            mBuffer.Recycle();
        }


        /// <summary>
        /// Leave the channel the player is in.
        /// </summary>

        void SendLeaveChannel(TcpPlayer player, bool notify)
        {
            var ch = player.channel;
            if (ch != null)
            {
                // Remove this player from the channel
                ch.RemovePlayer(player);
                player.channel = null;
                
                // Are there other players left?
                if (ch.players.size > 0)
                {
                    // Inform everyone of this player leaving the channel
                    BinaryWriter writer = BeginSend(Packet.ResponsePlayerLeft);
                    writer.Write(player.id);
                    EndSend(true, ch, null);
                }

                // Notify the player that they have left the channel
                if (notify && player.isConnected)
                {
                    BeginSend(Packet.ResponseLeaveChannel);
                    EndSend(true, player);
                }
            }
        }

        static public void SelectTeamRole(Channel channel, TcpPlayer newPlayer) {
            var selectTeamId = channel.players.size%2;

            newPlayer.teamId = selectTeamId == 0 ? 0 : 1;

            var playersInTeam = new NetList<TcpPlayer>();

            foreach (TcpPlayer p in channel.players)
            {
                if(p.teamId == newPlayer.teamId)
                    playersInTeam.Add(p);
            }

            newPlayer.role = playersInTeam.size;

        }


        /// <summary>
        /// Join the specified channel.
        /// </summary>

        void SendJoinChannel(TcpPlayer player, Channel channel)
        {
            if (player.channel == null || player.channel != channel)
            {
                
                // Set the player's channel
                player.channel = channel;

                // Set the player's team and role
                SelectTeamRole(channel, player);

                // Everything else gets sent to the player, so it's faster to do it all at once
                player.FinishJoiningChannel();


                // Inform the channel that a new player is joining
                var writer = BeginSend(Packet.ResponsePlayerJoined);
                {
                    writer.Write(player.id);
                    writer.Write(string.IsNullOrEmpty(player.name) ? "Guest" : player.name);
                    writer.Write(player.teamId);
                    writer.Write(player.house);
                    writer.Write(player.role);
                }
                EndSend(true, channel, null);

                // Add this player to the channel now that the joining process is complete
                channel.players.Add(player);
            }
        }

        /// <summary>
        /// Receive and process a single incoming packet.
        /// Returns 'true' if a packet was received, 'false' otherwise.
        /// </summary>

        bool ProcessPlayerPacket(Buffer buffer, TcpPlayer player, bool reliable)
        {
            var reader = buffer.BeginReading();
            var request = (Packet)reader.ReadByte();

            // If the player has not yet been verified, the first packet must be an ID request
            if (player.stage == TcpProtocol.Stage.Verifying)
            {
                if (player.VerifyRequestID(request, reader, true))
                {
                    mDictionaryID.Add(player.id, player);
                    if (lobbyLink != null) lobbyLink.SendUpdate(this);
                    if (onPlayerConnect != null) onPlayerConnect(player);
                    return true;
                }
                Debug.LogWarning("O endereco '" +player.address + "' falhou na etapa de verificacao");
                RemovePlayer(player);
                return false;
            }

            switch (request)
            {
                case Packet.Empty:
                    {
                        break;
                    }
                case Packet.Error:
                    {
                        Error(player, reader.ReadString());
                        break;
                    }
                case Packet.Disconnect:
                    {
                        RemovePlayer(player);
                        break;
                    }
                case Packet.RequestPing:
                    {
                        // Respond with a ping back
                        BeginSend(Packet.ResponsePing);
                        EndSend(true, player);
                        break;
                    }
                case Packet.RequestSetUDP:
                    {
                        int port = reader.ReadUInt16();

                        if (port != 0)
                        {
                            var ip = new IPAddress(player.tcpEndPoint.Address.GetAddressBytes());
                            SetPlayerUdpEndPoint(player, new IPEndPoint(ip, port));
                        }
                        else SetPlayerUdpEndPoint(player, null);

                        // Let the player know if we are hosting an active UDP connection
                        var udp = mUdp.isActive ? (ushort)mUdp.listeningPort : (ushort)0;
                        BeginSend(Packet.ResponseSetUDP).Write(udp);
                        EndSend(true, player);

                        // Send an empty packet to the target player to open up UDP for communication
                        if (player.udpEndPoint != null) mUdp.SendEmptyPacket(player.udpEndPoint);
                        break;
                    }

                case Packet.RequestJoinChannel:
                    {
                        // Join the specified channel
                        var levelName = reader.ReadString();

                        if (player.channel == null)
                        {
                            if (mChannel == null)
                            {
                                var writer = BeginSend(Packet.ResponseJoinChannel);
                                writer.Write(false);
                                writer.Write("O channel nao foi encontrado");
                                EndSend(true, player);
                            }

                            if (!string.IsNullOrEmpty(levelName))
                                mChannel.level = levelName;
                            SendLeaveChannel(player, false);
                            SendJoinChannel(player, mChannel);
                        }
                        break;
                    }
                case Packet.RequestSetRole:
                    {
                        // Troca o role do jogador
                        player.role = reader.ReadInt32();

                        var writer = BeginSend(Packet.ResponseSetRole);
                        writer.Write(player.id);
                        writer.Write(player.role);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestSetHouse:
                    {
                        // Troca o house do jogador
                        player.house = reader.ReadInt32();

                        var writer = BeginSend(Packet.ResponseSetHouse);
                        writer.Write(player.id);
                        writer.Write(player.house);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestSetSynchronized: {
                        // Troca o status de sincronizado do jogador
                        player.isSynchronized = reader.ReadBoolean();

                        var writer = BeginSend(Packet.ResponseSetSynchronized);
                        writer.Write(player.id);
                        writer.Write(player.isSynchronized);

                        if (player.channel != null) {
                            EndSend(true, player.channel, null);
                        }
                        else {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestSetReady:
                    {
                        // Troca o status de ready do jogador
                        player.isReady = reader.ReadBoolean();

                        var writer = BeginSend(Packet.ResponseSetReady);
                        writer.Write(player.id);
                        writer.Write(player.isReady);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestSetTeam:
                    {
                        // Change the player's name
                        player.teamId = reader.ReadInt32();

                        var writer = BeginSend(Packet.ResponseSetTeam);
                        writer.Write(player.id);
                        writer.Write(player.teamId);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestSetTurnEnded:
                    {
                        // O turno que o jogador finalizou
                        var turn = reader.ReadInt32();

                        var writer = BeginSend(Packet.ResponseSetTurnEnded);
                        writer.Write(player.id);
                        writer.Write(turn);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, player);
                        }
                        break;
                    }
                case Packet.RequestSetPing:
                    {
                        // Change the player's ping
                        player.ping = reader.ReadInt32();

                        var writer = BeginSend(Packet.ResponsePingPlayer);
                        writer.Write(player.id);
                        writer.Write(player.ping);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        } 
                        break;
                    }
                case Packet.RequestSetName:
                    {
                        // Change the player's name
                        player.name = reader.ReadString();

                        var writer = BeginSend(Packet.ResponseRenamePlayer);
                        writer.Write(player.id);
                        writer.Write(player.name);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestAllSynchronize:
                    {
                        BeginSend(Packet.ResponseAllSynchronize);

                        if (player.channel != null)
                        {
                            EndSend(true, player.channel, null);
                        }
                        else
                        {
                            EndSend(true, player);
                        }
                        break;
                    }
                case Packet.RequestNoDelay:
                    {
                        player.noDelay = reader.ReadBoolean();
                        break;
                    }
                case Packet.ForwardToPlayer:
                    {
                        // Forward this packet to the specified player
                        var target = GetPlayer(reader.ReadInt32());

                        if (target != null && target.isConnected)
                        {
                            // Reset the position back to the beginning (4 bytes for size, 1 byte for ID, 4 bytes for player)
                            buffer.position = buffer.position - 9;
                            target.SendTcpPacket(buffer);
                        }
                        break;
                    }
                default:
                    {
                        if (player.channel != null)
                        {
                            // Other packets can only be processed while in a channel
                            if ((int)request >= (int)Packet.ForwardToAll)
                            {
                                ProcessForwardPacket(player, buffer, reader, request, reliable);
                            }
                            else
                            {
                                ProcessChannelPacket(player, buffer, reader, request);
                            }
                        }
                        else if (onCustomPacket != null)
                        {
                            onCustomPacket(player, buffer, reader, request, reliable);
                        }
                        break;
                    }
            }
            return true;
        }

        /// <summary>
        /// Process a packet that's meant to be forwarded.
        /// </summary>

        void ProcessForwardPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request, bool reliable)
        {
            // We can't send unreliable packets if UDP is not active
            if (!mUdp.isActive || buffer.size > 1024) reliable = true;

            // We want to exclude the player if the request was to forward to others
            var exclude = (
                request == Packet.ForwardToOthers) ? player : null;

            // 4 bytes for size, 1 byte for ID
            buffer.position -= 5;

            // Forward the packet to everyone except the sender
            for (var i = 0; i < player.channel.players.size; ++i)
            {
                var tp = player.channel.players[i];

                if (tp != exclude)
                {
                    if (reliable || tp.udpEndPoint == null || !mAllowUdp)
                    {
                        tp.SendTcpPacket(buffer);
                    }
                    else mUdp.Send(buffer, tp.udpEndPoint);
                }
            }
        }

        /// <summary>
        /// Process a packet from the player.
        /// </summary>

        void ProcessChannelPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request)
        {
            switch (request)
            {
                case Packet.RequestLoadLevel:
                    {
                        // Change the currently loaded level
                        player.channel.Reset();
                        player.channel.level = reader.ReadString();

                        var writer = BeginSend(Packet.ResponseLoadLevel);
                        writer.Write(string.IsNullOrEmpty(player.channel.level) ? "" : player.channel.level);
                        EndSend(true, player.channel, null);
                        break;
                    }
                case Packet.RequestLeaveChannel:
                    {
                        SendLeaveChannel(player, true);
                        break;
                    }
            }
        }




    }
}
