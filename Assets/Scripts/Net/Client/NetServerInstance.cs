//---------------------------------------------
//            Network
//---------------------------------------------

using UnityEngine;
using Net;
using System.IO;
using System.Collections;
using System.Net;

/// <summary>
/// Network server adapitado para a Unity.
/// </summary>

public class NetServerInstance : MonoBehaviour
{
	static NetServerInstance mInstance;

	public enum Type
	{
		Lan,
		Udp,
		Tcp,
	}

	public enum State
	{
		Inactive,
		Starting,
		Active,
	}

	GameServer mGame = new GameServer();
	LobbyServer mLobby;
	UPnP mUp = new UPnP();

	/// <summary>
    /// Acesso a Instance é somente interno porem todas as funcoes sao estaticas.
	/// </summary>

	static NetServerInstance instance
	{
		get
		{
			if (mInstance == null)
			{
				GameObject go = new GameObject("_Server");
				mInstance = go.AddComponent<NetServerInstance>();
				DontDestroyOnLoad(go);
			}
			return mInstance;
		}
	}

	/// <summary>
    /// Se o servidor esta ativo.
	/// </summary>

	static public bool isActive { get { return (mInstance != null) && mInstance.mGame.isActive; } }

	/// <summary>
    /// Se o servidor esta disponivel para novas conexoes.
	/// </summary>

	static public bool isListening { get { return (mInstance != null) && mInstance.mGame.isListening; } }

	/// <summary>
    /// Porta usada para escutar novas conexoes TCP.
    /// </summary>

	static public int listeningPort { get { return (mInstance != null) ? mInstance.mGame.tcpPort : 0; } }

	/// <summary>
	/// Nome do servidor.
	/// </summary>

	static public string serverName { get { return (mInstance != null) ? mInstance.mGame.name : null; } set { if (instance != null) mInstance.mGame.name = value; } }
  
    /// <summary>
    /// Senha do servidor.
    /// </summary>

    static public string serverPassword { get { return (mInstance != null) ? mInstance.mGame.password : null; } set { if (instance != null) mInstance.mGame.password = value; } }
   
    /// <summary>
    /// Senha do servidor.
    /// </summary>

    static public string serverPlayerHost { get { return (mInstance != null) ? mInstance.mGame.host : null; } set { if (instance != null) mInstance.mGame.host = value; } }

    /// <summary>
    /// Verifica se o servidor esta protegido por senha
    /// </summary>

    static public bool isPassProtected { get { return (mInstance != null) && mInstance.mGame.isPassProtected; } }
 
    /// <summary>
    /// Quantos jogadores poderão conectar ao servidor.
    /// </summary>

    static public int playerLimit { get { return (mInstance != null) ? mInstance.mGame.playerLimit : 65535; } set { if (instance != null) mInstance.mGame.playerLimit = value; } }

    /// <summary>
    /// Jogadores poderam entrar mesmo depois da partida iniciada
    /// </summary>

    static public bool joinInGame { get { return (mInstance != null) ? mInstance.mGame.joinInGame : true; } set { if (instance != null) mInstance.mGame.joinInGame = value; } }

	/// <summary>
    /// Quantos jogadores estão atualmente conectados no servidor.
	/// </summary>

	static public int playerCount { get { return (mInstance != null) ? mInstance.mGame.playerCount : 0; } }

	/// <summary>
    /// Game server ativo.
	/// </summary>

	static public GameServer game { get { return (mInstance != null) ? mInstance.mGame : null; } }

	/// <summary>
    /// Lobby server ativo.
	/// </summary>

	static public LobbyServer lobby { get { return (mInstance != null) ? mInstance.mLobby : null; } }

    /// <summary>
    /// Define os dados do servidor
    /// </summary>

    static public void SetData(string name, string pass, int limit, bool join, string host)
    {
        serverName = name;
        serverPassword = pass;
        playerLimit = limit;
        joinInGame = join;
        serverPlayerHost = host;
    }

	/// <summary>
    /// Inicia um servidor local na porta TCP especifica.
	/// </summary>

	static public bool Start (int tcpPort)
	{
		return instance.StartLocal(tcpPort, 0, 0);
	}

    /// <summary>
    /// Inicia um servidor local nas portas TCP e UDP especificas.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort)
	{
		return instance.StartLocal(tcpPort, udpPort, 0);
	}


    /// <summary>
    /// Inicia um servidor local nas portas TCP, UDP e broadcast especificas.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort, int lanBroadcastPort)
	{
		return instance.StartLocal(tcpPort, udpPort, lanBroadcastPort);
	}

	/// <summary>
    /// Inicia um servidor local
	/// </summary>

	bool StartLocal (int tcpPort, int udpPort, int lobbyPort)
	{
        // Verifica primeiro se tudo foi parado
		if (mGame.isActive) Disconnect();

        // Se existe uma porta para o lobby, deve primeiro iniciar o lobby server/link.
        // Fazendo isso vai permitir que informemos o lobby que um novo server será iniciado.

		if (lobbyPort > 0)
		{
			mLobby = new UdpLobbyServer();

            // Inicia um lobby server local
			if (mLobby.Start(lobbyPort))
			{
				mUp.OpenUDP(lobbyPort);
			}
			else
			{
				mLobby = null;
				return false;
			}
            // Cria um lobby link local
			mGame.lobbyLink = new LobbyServerLink(mLobby);
		}

        // Inicia o servidor
		if (mGame.Start(tcpPort, udpPort))
		{
			mUp.OpenTCP(tcpPort);
			mUp.OpenUDP(udpPort);
			return true;
		}

        // Alguma coisa deu errado, para tudo
		Disconnect();
		return false;
	}

	/// <summary>
    /// Para o servidor.
	/// </summary>

	static public void Stop () { if (mInstance != null) mInstance.Disconnect(); }

	/// <summary>
    /// Tranca o ervidor para nao aceitar mais conexoes.
	/// </summary>

	static public void MakePrivate () { if (mInstance != null) mInstance.mGame.MakePrivate(); }

	/// <summary>
    /// Para tudo.
    /// </summary>

	void Disconnect ()
	{
		mGame.Stop();

		if (mLobby != null)
		{
			mLobby.Stop();
			mLobby = null;
		}
		mUp.Close();
	}

	/// <summary>
    /// Verifica se os servidores foram parados quando a instancia for destruida.
	/// </summary>

	void OnDestroy ()
	{
		Disconnect();
		mUp.WaitForThreads();
	}
}
