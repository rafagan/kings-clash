//---------------------------------------------
//            Network
//---------------------------------------------

using System.IO;
using UnityEngine;
using Net;
using System.Net;

/// <summary>
/// Network Manager é responsável por gerenciar toda a parte de network do jogo.
/// </summary>
[AddComponentMenu("Net/Network Manager")]
public class NetManager : MonoBehaviour {

    /// <summary>
    /// Configuração da rede
    /// </summary>
    public float SyncTimeSecs = 3;
    public float turnTimeMs = 200;
    public int serverTcpPort = 5127;
    public string MenuSceneName = "MainMenuFinal";
    public string GameSceneName = "MainGame";

    // Network client
    private readonly GameClient mClient = new GameClient();

    // Static player, apenas para o GetPlayer() funcionar mesmo se a instancia nao existir.
    private static readonly Player mPlayer = new Player("Guest");

    // Player list que contem apenas o player local. Pela mesma razao do 'mPlayer'.
    private static NetList<Player> mPlayers;

    // Instance pointer
    private static NetManager mInstance;

    /// <summary>
    /// Net Client usado para communicação.
    /// </summary>
    public static GameClient client {
        get { return (mInstance != null) ? mInstance.mClient : null; }
    }

    /// <summary>
    /// Verifica se esta conectado em algum servidor.
    /// </summary>
    public static bool isConnected {
        get { return mInstance != null && mInstance.mClient.isConnected; }
    }

    /// <summary>
    /// Se esta tentando estabelecer uma nova conexão.
    /// </summary>
    public static bool isTryingToConnect {
        get { return mInstance != null && mInstance.mClient.isTryingToConnect; }
    }

    /// <summary>
    /// Se o jogador é o host.
    /// </summary>
    public static bool isHosting {
        get { return mInstance == null || mInstance.mClient.isHosting; }
    }

    /// <summary>
    /// Se o jogador esta no canal de comunicação.
    /// </summary>
    public static bool isInChannel {
        get { return mInstance != null && mInstance.mClient.isConnected && mInstance.mClient.isInChannel; }
    }

    /// <summary>
    /// Ativa ou desativa o Nagle's buffering algorithm (aka NO_DELAY flag).
    /// Ativando este flag vai melhorar a latencia em troca do custo de largura de banda.
    /// http://en.wikipedia.org/wiki/Nagle's_algorithm
    /// </summary>
    public static bool noDelay {
        get { return mInstance != null && mInstance.mClient.noDelay; }
        set {
            if (mInstance != null) {
                mInstance.mClient.noDelay = value;
            }
        }
    }

    /// <summary>
    /// Ping atual do servidor.
    /// </summary>
    public static int ping {
        get { return mInstance != null ? mInstance.mClient.ping : 0; }
    }

    /// <summary>
    /// Se pode usar UDP para comunicar com o servidor.
    /// </summary>
    public static bool canUseUDP {
        get { return mInstance != null && mInstance.mClient.canUseUDP; }
    }


    /// <summary>
    /// Porta para pacotes UDP. Iniciado atraves do NetManager.StartUDP().
    /// </summary>
    public static int listeningPort {
        get { return mInstance != null ? mInstance.mClient.listeningPort : 0; }
    }

    /// <summary>
    /// Identificador unico do player.
    /// </summary>
    public static int playerID {
        get { return isConnected ? mInstance.mClient.playerID : mPlayer.id; }
    }

    /// <summary>
    /// Turno atual do jogador.
    /// </summary>
    public static int playerTurn {
        get { return isConnected ? mInstance.mClient.playerTurn : 0; }
    }

    /// <summary>
    /// Se o turno atual do jogador finalizou.
    /// </summary>
    public static bool allTurnsEnded {
        get { return mInstance.mClient.allTurnsEnded; }
    }

    /// <summary>
    /// House atual do jogador
    /// </summary>
    public static int playerHouse {
        get { return isConnected ? mInstance.mClient.playerHouse : mPlayer.house; }
        set {
            if (playerHouse != value) {
                mPlayer.house = value;
                if (isConnected) {
                    mInstance.mClient.playerHouse = value;
                }
            }
        }
    }

    /// <summary>
    /// Role atual do jogador
    /// </summary>
    public static int playerRole {
        get { return isConnected ? mInstance.mClient.playerRole : mPlayer.role; }
        set {
            if (playerRole != value) {
                mPlayer.role = value;
                if (isConnected) {
                    mInstance.mClient.playerRole = value;
                }
            }
        }
    }

    /// <summary>
    /// Se o jogador esta sincronizado com os demais
    /// </summary>
    public static bool playerSynchronized {
        get { return (isConnected) ? mInstance.mClient.playerSynchronized : mPlayer.isSynchronized; }
        set {
            if (playerSynchronized != value) {
                mPlayer.isSynchronized = value;
                if (isConnected) {
                    mInstance.mClient.playerSynchronized = value;
                }
            }
        }
    }

    /// <summary>
    /// Se o jogador esta pronto para iniciar o jogo
    /// </summary>
    public static bool playerReady {
        get { return (isConnected) ? mInstance.mClient.playerReady : mPlayer.isReady; }
        set {
            if (playerReady != value) {
                mPlayer.isReady = value;
                if (isConnected) {
                    mInstance.mClient.playerReady = value;
                }
            }
        }
    }

    /// <summary>
    /// Nome do jogaodor
    /// </summary>
    public static string playerName {
        get { return (isConnected) ? mInstance.mClient.playerName : mPlayer.name; }
        set {
            if (playerName != value) {
                mPlayer.name = value;
                if (isConnected) {
                    mInstance.mClient.playerName = value;
                }
            }
        }
    }

    /// <summary>
    /// Time do jogador
    /// </summary>
    public static int playerTeamID {
        get { return (isConnected) ? mInstance.mClient.playerTeamID : mPlayer.teamId; }
        set {
            if (playerTeamID != value) {
                mPlayer.teamId = value;
                if (isConnected) {
                    mInstance.mClient.playerTeamID = value;
                }
            }
        }
    }

    /// <summary>
    /// Lista dos jogadores que estao no mesmo channel que o jogador.
    /// </summary>
    public static NetList<Player> players {
        get {
            if (isConnected) {
                return mInstance.mClient.players;
            }
            return mPlayers ?? (mPlayers = new NetList<Player>());
        }
    }


    /// <summary>
    /// Lista dos servidores que estao disponiveis.
    /// </summary>
    public static NetList<ServerList.Entry> serverList {
        get { return NetLobbyClient.knownServers.list; }
    }

    /// <summary>
    /// Retorna o Player local.
    /// </summary>
    public static Player player {
        get { return isConnected ? mInstance.mClient.player : mPlayer; }
    }

    /// <summary>
    /// Retorna o Player a partir do ID especificado.
    /// </summary>
    public static Player GetPlayer(int id) {
        if (isConnected) {
            return mInstance.mClient.GetPlayer(id);
        }
        if (id == mPlayer.id) {
            return mPlayer;
        }
        return null;
    }

    /// <summary>
    /// Start listening for incoming UDP packets on the specified port.
    /// </summary>
    private static bool StartUDP(int udpPort) {
        return (mInstance != null) ? mInstance.mClient.StartUDP(udpPort) : false;
    }

    /// <summary>
    /// Stop listening to incoming UDP packets.
    /// </summary>
    private static void StopUDP() {
        if (mInstance != null) {
            mInstance.mClient.StopUDP();
        }
    }

    /// <summary>
    /// Tempo de duracao de cada turno
    /// </summary>
    public static float timeEachTurn {
        get { return mInstance.turnTimeMs; }
    }

    /// <summary>
    /// Tempo para sincronizar os jogadores
    /// </summary>
    public static float syncronizeTime {
        get { return mInstance.SyncTimeSecs; }
    }

    /// <summary>
    /// Nome da scene do menu para referencia
    /// </summary>
    public static string menuScene {
        get { return mInstance.MenuSceneName; }
    }

    /// <summary>
    /// Nome da scene do lobby para referencia
    /// </summary>
    public static string gameScene {
        get { return mInstance.GameSceneName; }
    }


    /// <summary>
    /// Host a new server game
    /// </summary>
    /// <param name="serverName">Nome do servidor, o que os outros jogadores verão na lista.</param>
    /// <param name="serverPassword">Senha do servidor, se for vazia não haverá senha.</param>
    /// <param name="playerLimit">Numero maximo de jogadores que poderao estar conectados.</param>
    /// <param name="joinInGame">Se os jogadores poderão entrar mesmo se a partida já iniciou.</param>
    public static void HostGame(int lobbyPort, string serverName, string serverPassword, int playerLimit,
        bool joinInGame) {
        if (mInstance != null) {
            NetServerInstance.SetData(serverName, serverPassword, playerLimit, joinInGame, playerName);

            int udpPort = Random.Range(10000, 40000);
            NetServerInstance.Start(mInstance.serverTcpPort, udpPort, lobbyPort);

            // Connect to local
            Connect("127.0.0.1");
        }
    }


    /// <summary>
    /// Connect to the specified remote destination password.
    /// </summary>
    public static bool ConnectGame(ServerList.Entry entry, string password) {
        if (string.IsNullOrEmpty(entry.password) || (entry.password == password)) {
            if (entry.joinInGame) {
                IPEndPoint externalIP = entry.externalAddress;
                IPEndPoint internalIP = entry.internalAddress;

                if (mInstance != null) {
                    mInstance.mClient.playerName = mPlayer.name;
                    mInstance.mClient.Connect(externalIP, internalIP);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Entra na partida do jogo
    /// Apenas o host pode dar o comando de entrar no jogo
    /// </summary>
    public static void StartGame() {
        if (isConnected && isHosting) {
            if (mInstance != null) {
                LoadLevel(gameScene);
            }
        }
    }

    /// <summary>
    /// Conecta ao destino a partir de um endereço e porta especificados.
    /// </summary>
    public static void Connect(string address, int port) {
        IPEndPoint ip = Tools.ResolveEndPoint(address, port);

        if (ip == null) {
            if (mInstance != null) {
                mInstance.OnConnect(false, "Nao foi possivel estabelecer uma conexao com " + address);
            }
        } else if (mInstance != null) {
            mInstance.mClient.playerName = mPlayer.name;
            mInstance.mClient.playerTeamID = mPlayer.teamId;
            mInstance.mClient.playerHouse = mPlayer.house;
            mInstance.mClient.playerRole = mPlayer.role;
            mInstance.mClient.Connect(ip, null);
        }
    }


    /// <summary>
    /// Conecta ao destino a partir de um endereço e porta especificados (Exemplo: "255.255.255.255:255").
    /// </summary>
    public static void Connect(string address) {
        if (mInstance != null) {
            string[] split = address.Split(new[] {':'});
            if (split.Length > 1) {
                int.TryParse(split[1], out mInstance.serverTcpPort);
            }
            Connect(split[0], mInstance.serverTcpPort);
        }
    }

    /// <summary>
    /// Desconecta do servidor.
    /// Se for o host, desconecta e encerra o servidor.
    /// </summary>
    public static void Disconnect() {
        if (mInstance != null) {
            // Desconecta do servidor
            mInstance.mClient.Disconnect();
            // Para se houver um servidor rodando
            if (NetServerInstance.isActive) {
                NetServerInstance.Stop();
            }
        }
    }

    /// <summary>
    /// Load the specified level.
    /// </summary>
    public static void LoadLevel(string levelName) {
        if (isConnected) {
            mInstance.mClient.LoadLevel(levelName);
        } else {
            Application.LoadLevel(levelName);
        }
    }

    /// <summary>
    /// Begin sending a new packet to the server.
    /// </summary>
    public static BinaryWriter BeginSend(Packet type) {
        return mInstance.mClient.BeginSend(type);
    }

    /// <summary>
    /// Begin sending a new packet to the server.
    /// </summary>
    public static BinaryWriter BeginSend(byte packetID) {
        return mInstance.mClient.BeginSend(packetID);
    }

    /// <summary>
    /// Send the outgoing buffer.
    /// </summary>
    public static void EndSend() {
        mInstance.mClient.EndSend(true);
    }

    /// <summary>
    /// Send the outgoing buffer.
    /// </summary>
    public static void EndSend(bool reliable) {
        mInstance.mClient.EndSend(reliable);
    }

    /// <summary>
    /// Broadcast the packet to everyone on the LAN.
    /// </summary>
    public static void EndSend(int port) {
        mInstance.mClient.EndSend(port);
    }

    /// <summary>
    /// Broadcast the packet to the specified endpoint via UDP.
    /// </summary>
    public static void EndSend(IPEndPoint target) {
        mInstance.mClient.EndSend(target);
    }

    #region MonoBehaviour Functions

    /// <summary>
    /// Ensure that there is only one instance of this class present.
    /// </summary>
    private void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        } else {
            mInstance = this;
            DontDestroyOnLoad(gameObject);

            mClient.onError += OnError;
            mClient.onConnect += OnConnect;
            mClient.onDisconnect += OnDisconnect;
            mClient.onJoinChannel += OnJoinChannel;
            mClient.onLeftChannel += OnLeftChannel;
            mClient.onLoadLevel += OnLoadLevel;
            mClient.onPlayerJoined += OnPlayerJoined;
            mClient.onPlayerLeft += OnPlayerLeft;
            mClient.onRenamePlayer += OnRenamePlayer;
            mClient.onChangedPlayerTeam += OnChangedPlayerTeam;
            mClient.onForwardedPacket += OnForwardedPacket;
        }
    }

    /// <summary>
    /// Start listening for incoming UDP packets
    /// </summary>
    private void Start() {
        if (Application.isPlaying) {
            // Make it possible to use UDP using a random port
            //StartUDP(serverUdpPort);

            StartUDP(Random.Range(10000, 50000));
        }
    }

    /// <summary>
    /// Make sure we disconnect on exit.
    /// </summary>
    private void OnDestroy() {
        if (mInstance == this) {
            if (isConnected) {
                mClient.Disconnect();
            }
            StopUDP();
            mInstance = null;
        }
    }

    /// <summary>
    /// If custom functionality is needed, all unrecognized packets will arrive here.
    /// </summary>
    private void OnForwardedPacket(BinaryReader reader) {
        uint objID;
        byte funcID;
        NetObject.DecodeUID(reader.ReadUInt32(), out objID, out funcID);

        if (funcID == 0) {
            NetObject.FindAndExecute(objID, reader.ReadString(), UnityTools.Read(reader));
        } else {
            NetObject.FindAndExecute(objID, funcID, UnityTools.Read(reader));
        }
    }

    /// <summary>
    /// Process incoming packets in the update function.
    /// </summary>
    private void Update() {
        mClient.ProcessPackets();
    }

    #endregion

    #region Callbacks -- Modificar aqui a abordagem dos broadcasts

    /// <summary>
    /// Error notification.
    /// </summary>
    private void OnError(string err) {
        UnityTools.Broadcast("OnNetworkError", err);
    }

    /// <summary>
    /// Connection result notification.
    /// </summary>
    private void OnConnect(bool success, string message) {
        UnityTools.Broadcast("OnNetworkConnect", success, message);
    }

    /// <summary>
    /// Notification that happens when the client gets disconnected from the server.
    /// </summary>
    private void OnDisconnect() {
        if (mInstance != null) {
            mInstance.mClient.player.resetAll();
        }
        UnityTools.Broadcast("OnNetworkDisconnect");
    }

    /// <summary>
    /// Notification sent when attempting to join a channel, indicating a success or failure.
    /// </summary>
    private void OnJoinChannel(bool success, string message) {
        UnityTools.Broadcast("OnNetworkJoinChannel", success, message);
    }

    /// <summary>
    /// Notification sent when leaving a channel.
    /// Also sent just before a disconnect (if inside a channel when it happens).
    /// </summary>
    private void OnLeftChannel() {
        UnityTools.Broadcast("OnNetworkLeaveChannel");
    }

    /// <summary>
    /// Notification sent when a level is changing.
    /// </summary>
    private void OnLoadLevel(string levelName) {
        if (!string.IsNullOrEmpty(levelName)) {
            if (levelName.Equals(gameScene)) {
                mInstance.mClient.player.resetSystem();
            }
            Application.LoadLevel(levelName);
        }
    }

    /// <summary>
    /// Notification of a new player joining the channel.
    /// </summary>
    private void OnPlayerJoined(Player p) {
        UnityTools.Broadcast("OnNetworkPlayerJoin", p);
    }

    /// <summary>
    /// Notification of another player leaving the channel.
    /// </summary>
    private void OnPlayerLeft(Player p) {
        UnityTools.Broadcast("OnNetworkPlayerLeave", p);
    }

    /// <summary>
    /// Notification of a player being renamed.
    /// </summary>
    private void OnRenamePlayer(Player p, string previous) {
        mPlayer.name = p.name;
        UnityTools.Broadcast("OnNetworkPlayerRenamed", p, previous);
    }

    /// <summary>
    /// Notification of a player changing their name.
    /// </summary>
    private void OnChangedPlayerTeam(Player p, int previous) {
        mPlayer.teamId = p.teamId;
        UnityTools.Broadcast("OnNetworkChangedPlayerTeam", p, previous);
    }

    #endregion
}
