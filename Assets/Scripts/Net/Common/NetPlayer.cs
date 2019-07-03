//---------------------------------------------
//            Network
//---------------------------------------------

namespace Net
{
/// <summary>
/// Class containing basic information about a remote player.
/// </summary>

public class Player
{
	static protected int mPlayerCounter = 0;

	/// <summary>
	/// Protocol version.
	/// </summary>

	public const int version = 7;

	/// <summary>
	/// All players have a unique identifier given by the server.
	/// </summary>

	public int id = 1;

	/// <summary>
	/// All players have a name that they chose for themselves.
	/// </summary>

	public string name = "Guest";

	/// <summary>
	/// Id do time
	/// </summary>

    public int teamId = 0;

	/// <summary>
	/// Ping atual do jogador
	/// </summary>

    public int ping = 0;

    /// <summary>
    /// Turno atual do jogador
    /// </summary>

    public int turn = 0;

    /// <summary>
    /// Se o turno atual do jogador finalizou
    /// </summary>

    public bool turnEnded = false;

    /// <summary>
    /// Se o jogador esta sincronizado com os outros
    /// </summary>

    public bool isSynchronized = false;

    /// <summary>
    /// House atual do jogador
    /// </summary>

    public int house = 0;

    /// <summary>
    /// Role atual do jogador
    /// </summary>

    public int role = 0;

    /// <summary>
    /// Se esta pronto para iniciar o jogo
    /// </summary>

    public bool isReady = false;

    /// <summary>
    /// Custom data
    /// </summary>

    public object data = null;


    /// <summary>
    /// Reseta todas as propriedades e o sistema de atualizacao do jogador
    /// </summary>

    public void resetAll() {
        house = 0;
        role = 0;
        teamId = 0;
        name = "Guest";
        resetSystem();
    }

    /// <summary>
    /// Reseta as variaveis do sistema de atualizacao do jogador
    /// </summary>

    public void resetSystem() {
        isReady = false;
        isSynchronized = false;
        turnEnded = false;
        turn = 0;
    }

	public Player () { }
	public Player (string playerName) { name = playerName; }
}
}
